#include "pch.h"
#include "MiniDump.h"

LONG WINAPI UnHandledExceptionFilter(struct _EXCEPTION_POINTERS* exceptionInfo)
{
    HMODULE    DllHandle = NULL;

    // Windows 2000 ???? ?? DBGHELP? ???? ??? ??? ??.
    DllHandle = LoadLibraryA("DBGHELP.DLL");

    if (DllHandle)
    {
        MINIDUMPWRITEDUMP Dump = (MINIDUMPWRITEDUMP)GetProcAddress(DllHandle, "MiniDumpWriteDump");

        if (Dump)
        {
            char DumpPath[MAX_PATH] = { 0, };
            SYSTEMTIME SystemTime;

            GetLocalTime(&SystemTime);

            sprintf_s(DumpPath, MAX_PATH, "%d-%d-%d_%dh%dm%ds.dmp",
                SystemTime.wYear,
                SystemTime.wMonth,
                SystemTime.wDay,
                SystemTime.wHour,
                SystemTime.wMinute,
                SystemTime.wSecond);

            HANDLE FileHandle = CreateFileA(
                DumpPath,
                GENERIC_WRITE,
                FILE_SHARE_WRITE,
                NULL, CREATE_ALWAYS,
                FILE_ATTRIBUTE_NORMAL,
                NULL);

            if (FileHandle != INVALID_HANDLE_VALUE)
            {
                _MINIDUMP_EXCEPTION_INFORMATION MiniDumpExceptionInfo;

                MiniDumpExceptionInfo.ThreadId = GetCurrentThreadId();
                MiniDumpExceptionInfo.ExceptionPointers = exceptionInfo;
                MiniDumpExceptionInfo.ClientPointers = NULL;

                BOOL Success = Dump(
                    GetCurrentProcess(),
                    GetCurrentProcessId(),
                    FileHandle,
                    (_MINIDUMP_TYPE)(MiniDumpWithUnloadedModules +
                        MiniDumpWithHandleData +
                        MiniDumpWithFullMemory), //webzen...
                    &MiniDumpExceptionInfo,
                    NULL,
                    NULL);

                if (Success)
                {
                    CloseHandle(FileHandle);

                    return EXCEPTION_CONTINUE_SEARCH;// EXCEPTION_EXECUTE_HANDLER;
                }
            }

            CloseHandle(FileHandle);
        }
    }

    return EXCEPTION_CONTINUE_SEARCH;
}
