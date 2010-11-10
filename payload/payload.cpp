#include <windows.h>
#include "C:\python27\include\python.h"

DWORD __stdcall payload (LPVOID data) {wait 
  while (!Py_IsInitialized())
    Sleep(100);

  PyGILState_STATE gil = PyGILState_Ensure();

  PyRun_SimpleString("print 5 + 5");

  PyGILState_Release(gil);

  return 42;
}
