#include <windows.h>

DWORD payload (LPVOID data) {
  MessageBox(0, L"Testing", L"Test Message Box", 0);
  return 42;
}