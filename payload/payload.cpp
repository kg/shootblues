#include <windows.h>
#include "C:\python27\include\python.h"

static HWND g_rpcWindow;
static int g_rpcMessageId;

static PyObject * g_excValueError;

PyObject * rpcSend(PyObject *self, PyObject *args) {
    const char *messageBody;
    if (!PyArg_ParseTuple(args, "s", &messageBody)) {
      PyErr_SetString(g_excValueError, "rpcSend requires a message body string as its only argument");
      return NULL;
    }

    // Allocate enough memory to hold our message body and store it there
    size_t regionSize = strlen(messageBody) + 1;
    LPVOID region = VirtualAlloc(0, regionSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

    strcpy((char *)region, messageBody);

    // Post a message to our parent process (it will free our memory region once it gets the message)
    PostMessage(
      g_rpcWindow, g_rpcMessageId, 
      reinterpret_cast<WPARAM>(region), 
      *reinterpret_cast<LPARAM*>(&regionSize)
    );

    return PyBool_FromLong(1);
}

static PyMethodDef PythonMethods[] = {
    {"rpcSend", rpcSend, METH_VARARGS, "Send an RPC message to the parent process."},
    {NULL, NULL, 0, NULL}
};

DWORD __stdcall payload (HWND rpcWindow) {
  // Wait for an initialized python environment in our host process
  while (!Py_IsInitialized())
    Sleep(100);

  g_rpcWindow = rpcWindow;
  g_rpcMessageId = RegisterWindowMessage(L"ShootBlues.RPCMessage");

  MSG msg;
  // Create our thread message queue
  PeekMessage(&msg, 0, g_rpcMessageId, g_rpcMessageId, 0);

  // Initialize our python extensions
  PyGILState_STATE gil = PyGILState_Ensure();
  PyObject * exceptionsModule = PyImport_ImportModule("exceptions");
  g_excValueError = PyObject_GetAttrString(exceptionsModule, "ValueError");
  Py_InitModule("shootblues", PythonMethods);
  PyGILState_Release(gil);

  // Post a null message to alert the parent process that we are alive and ready for messages
  PostMessage(rpcWindow, g_rpcMessageId, 0, 0);

  BOOL result;
  while ((result = GetMessage( &msg, 0, g_rpcMessageId, g_rpcMessageId )) != 0) {
    // We've been told to terminate our message queue
    if (result == -1)
      break;

    char * script = reinterpret_cast<char *>(msg.wParam);
    DWORD scriptSize = *reinterpret_cast<DWORD *>(&(msg.lParam));
    
    PyGILState_STATE gil = PyGILState_Ensure();

    PyCodeObject * codeObject = (PyCodeObject *)Py_CompileStringFlags(
      script, "shootblues", Py_file_input, 0
    );

    // Free the memory block containing the message body since we've parsed it now
    VirtualFree(script, scriptSize, MEM_RELEASE);

    if (codeObject) {

      PyObject * module = PyImport_AddModule("shootblues");
      if (module != NULL) {

        PyObject * globals = PyModule_GetDict(module);
        PyObject * result = PyEval_EvalCode(codeObject, globals, globals);

        if (result == NULL) {
          PyErr_Print();
        } else {
          Py_DECREF(result);
          PyErr_Clear();
        }
      } 

      Py_DECREF(codeObject);
    } else {
      PyErr_Print();
    }

    PyGILState_Release(gil);
  }

  return 0;
}