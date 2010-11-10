#include <windows.h>
#include "C:\python27\include\python.h"

DWORD __stdcall payload (HWND rpcWindow) {
  while (!Py_IsInitialized())
    Sleep(100);

  UINT WM_RPC_MESSAGE = RegisterWindowMessage(L"ShootBlues.RPCMessage");
  MSG msg;

  PeekMessage(&msg, 0, WM_RPC_MESSAGE, WM_RPC_MESSAGE, 0);
  PostMessage(rpcWindow, WM_RPC_MESSAGE, 0, 0);

  BOOL result;
  while ((result = GetMessage( &msg, 0, WM_RPC_MESSAGE, WM_RPC_MESSAGE )) != 0) { 
    if (result == -1)
      break;

    char * script = reinterpret_cast<char *>(msg.wParam);
    DWORD scriptSize = *reinterpret_cast<DWORD *>(&(msg.lParam));
    
    PyGILState_STATE gil = PyGILState_Ensure();

    PyCodeObject * codeObject = (PyCodeObject *)Py_CompileStringFlags(script, "shootblues", Py_file_input, 0);
    VirtualFree(script, scriptSize, MEM_RELEASE);
    if (codeObject) {

      PyObject * module = PyImport_AddModule("__main__");
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
