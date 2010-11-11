#include <windows.h>
#include "C:\python27\include\python.h"

enum RPCMessageType : unsigned int {
  RMT_Run = 0,
  RMT_AddModule = 1,
  RMT_RemoveModule = 2,
  RMT_ReloadModules = 3
};

#pragma pack(push)
#pragma pack(1)
struct RPCMessage {
  RPCMessageType type;
  const char * moduleName;
  const char * text;
};
#pragma pack(pop)

static HWND g_rpcWindow;
static int g_rpcMessageId;

static PyObject * g_excException;
static PyObject * g_excValueError;
static PyObject * g_excImportError;
static PyObject * g_sysModule;
static PyObject * g_module;
static PyObject * g_moduleDict;

void runString (const char * script) {
  PyCodeObject * codeObject = (PyCodeObject *)Py_CompileStringFlags(
    script, "shootblues", Py_file_input, 0
  );

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
}

PyObject * run (PyObject * self, PyObject * args) {
  const char * script;
  if (!PyArg_ParseTuple(args, "s", &script)) {
    PyErr_SetString(g_excValueError, "run requires a script string as its only argument");
    return NULL;
  }

  runString(script);

  return Py_BuildValue("");
}

PyObject * rpcSend (PyObject * self, PyObject * args) {
    const char *messageBody;
    if (!PyArg_ParseTuple(args, "s", &messageBody)) {
      PyErr_SetString(g_excValueError, "rpcSend requires a message body string as its only argument");
      return NULL;
    }

    // Allocate enough memory to hold our message body and store it there
    size_t regionSize = strlen(messageBody) + 1;
    LPVOID region = VirtualAlloc(0, regionSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
    if (region) {
      strcpy((char *)region, messageBody);

      // Post a message to our parent process (it will free our memory region once it gets the message)
      if (PostMessage(
        g_rpcWindow, g_rpcMessageId, 
        reinterpret_cast<WPARAM>(region), 
        *reinterpret_cast<LPARAM*>(&regionSize)
      ))
        return Py_BuildValue("");
    }

    PyErr_SetFromWindowsErr(GetLastError());
    return NULL;
}

int addModuleString (const char * moduleName, const char * script) {
  PyObject * scriptString = PyString_FromString(script);
  int result = PyDict_SetItemString(g_moduleDict, moduleName, scriptString);
  Py_DECREF(scriptString);
  return result;
}

PyObject * addModule (PyObject * self, PyObject * args) {
  const char *moduleName, *script;
  if (!PyArg_ParseTuple(args, "ss", &moduleName, &script)) {
    PyErr_SetString(g_excValueError, "addModule expected args (moduleName, script)");
    return NULL;
  }

  if (addModuleString(moduleName, script)) {
    PyErr_SetString(g_excException, "addModule failed to add module");
    return NULL;
  } else
    return Py_BuildValue("");
}

int removeModuleString (const char * moduleName) {
  return PyDict_DelItemString(g_moduleDict, moduleName);
}

PyObject * removeModule (PyObject * self, PyObject * args) {
  const char *moduleName;
  if (!PyArg_ParseTuple(args, "s", &moduleName)) {
    PyErr_SetString(g_excValueError, "removeModule expects a module name as a string");
    return NULL;
  }

  if (removeModuleString(moduleName)) {
    PyErr_SetString(g_excException, "removeModule failed to remove module");
    return NULL;
  } else
    return Py_BuildValue("");
}

PyObject * reloadModules (PyObject * self, PyObject * args) {
  PyErr_SetString(g_excException, "Not implemented");
  return NULL;
}

PyObject * findModule (PyObject * self, PyObject * args, PyObject * kwargs) {
  static char * kwlist[] = {"fullname", "path", NULL};
  char *moduleName = 0;
  PyObject * path = 0;
  if (!PyArg_ParseTupleAndKeywords(args, kwargs, "s|O", kwlist, &moduleName, &path)) {
    PyErr_SetString(g_excValueError, "find_module expected (fullname, path)");
    return NULL;
  }

  if (strstr(moduleName, "shootblues.") != moduleName)
    return Py_BuildValue("");

  if (PyMapping_HasKeyString(g_moduleDict, moduleName + 11)) {
    Py_INCREF(g_module);
    return g_module;
  } else
    return Py_BuildValue("");
}

PyObject * loadModule (PyObject * self, PyObject * args) {
  // Inexplicably HasKeyString takes char* and not const char*
  char *moduleName;
  if (!PyArg_ParseTuple(args, "s", &moduleName)) {
    PyErr_SetString(g_excValueError, "load_module expects a module name as a string");
    return NULL;
  }

  if (strstr(moduleName, "shootblues.") != moduleName) {
    PyErr_SetString(g_excImportError, "load_module can only load child modules of shootblues");
    return NULL;
  }

  PyObject * scriptText = PyMapping_GetItemString(g_moduleDict, moduleName + 11);
  if (!scriptText) {
    PyErr_SetString(g_excImportError, "Module not found");
    return NULL;
  }

  PyObject * sysModules = PyObject_GetAttrString(g_sysModule, "modules");
  if (PyMapping_HasKeyString(sysModules, moduleName)) {
    PyObject * result = PyMapping_GetItemString(sysModules, moduleName);
    Py_DECREF(sysModules);
    return result;
  }

  PyCodeObject * codeObject = (PyCodeObject *)Py_CompileStringFlags(
    PyString_AsString(scriptText), moduleName + 11, Py_file_input, 0
  );

  if (codeObject) {
    PyObject * module = PyImport_AddModule(moduleName);
    if (module != NULL) {
      PyObject_SetAttrString(module, "__file__", PyString_FromString(moduleName + 11));
      PyObject_SetAttrString(module, "__name__", PyString_FromString(moduleName + 11));
      PyObject_SetAttrString(module, "__loader__", g_module);

      PyObject_SetAttrString(g_module, moduleName + 11, module);

      PyMapping_SetItemString(sysModules, moduleName, module);
      Py_DECREF(sysModules);

      PyObject * globals = PyModule_GetDict(module);
      PyObject * result = PyEval_EvalCode(codeObject, globals, globals);

      if (result == NULL) {
        Py_DECREF(sysModules);
        Py_DECREF(codeObject);
        return NULL;
      } else {
        Py_DECREF(codeObject);
        Py_DECREF(result);
      }
    } else {
      Py_DECREF(sysModules);
      Py_DECREF(codeObject);
      PyErr_SetString(g_excImportError, "Unable to create module object");
      return NULL;
    }

    Py_INCREF(module);
    return module;
  } else {
    return NULL;
  }
}

static PyMethodDef PythonMethods[] = {
    {"rpcSend", rpcSend, METH_VARARGS, "Send an RPC message to the parent process."},
    {"run", run, METH_VARARGS, "Compiles and runs a script block."},
    {"addModule", addModule, METH_VARARGS, "Adds a new script module or replaces an existing script module."},
    {"removeModule", removeModule, METH_VARARGS, "Removes an existing script module."},
    {"reloadModules", reloadModules, METH_VARARGS, "Reloads all script modules."},
    {"find_module", (PyCFunction)findModule, METH_KEYWORDS, "Implements the Finder protocol (PEP 302)."},
    {"load_module", loadModule, METH_VARARGS, "Implements the Loader protocol (PEP 302)."},
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
  g_excException = PyObject_GetAttrString(exceptionsModule, "Exception");
  g_excValueError = PyObject_GetAttrString(exceptionsModule, "ValueError");
  g_excImportError = PyObject_GetAttrString(exceptionsModule, "ImportError");

  g_module = Py_InitModule3("shootblues", PythonMethods, "Shoot Blues");
  // Setting these three values causes our find_module hook to be invoked for child modules :|
  PyObject_SetAttrString(g_module, "__package__", PyString_FromString("shootblues"));
  PyObject_SetAttrString(g_module, "__file__", PyString_FromString("shootblues"));
  PyObject_SetAttrString(g_module, "__path__", PyString_FromString("shootblues"));

  g_moduleDict = PyDict_New();
  PyObject_SetAttrString(g_module, "modules", g_moduleDict);

  // Install our import hook
  g_sysModule = PyImport_ImportModule("sys");
  PyObject * metaPath = PyObject_GetAttrString(g_sysModule, "meta_path");
  PyList_Append(metaPath, g_module);
  Py_DECREF(metaPath);

  PyGILState_Release(gil);

  // Post a null message to alert the parent process that we are alive and ready for messages
  PostMessage(rpcWindow, g_rpcMessageId, 0, 0);

  BOOL result;
  while ((result = GetMessage(&msg, 0, g_rpcMessageId, g_rpcMessageId)) != 0) {
    // We've been told to terminate our message queue
    if (result == -1)
      break;

    RPCMessage * rpc = reinterpret_cast<RPCMessage *>(msg.wParam);
    DWORD rpcSize = *reinterpret_cast<DWORD *>(&(msg.lParam));

    PyGILState_STATE gil = PyGILState_Ensure();

    switch (rpc->type) {
      case RMT_Run:
        runString(rpc->text);
        break;
      case RMT_AddModule:
        addModuleString(rpc->moduleName, rpc->text);
        break;
      case RMT_RemoveModule:
        removeModuleString(rpc->moduleName);
        break;
      case RMT_ReloadModules:
        reloadModules(0, 0);
        break;
    }

    PyGILState_Release(gil);

    // Free the memory block containing the message body since we've parsed it now
    VirtualFree(rpc, rpcSize, MEM_RELEASE);
  }

  return 0;
}