#include "Init.h"

#ifdef _WIN32
#include "windows.h"
#else
#include <dlfcn.h>
#endif

ScriptFunctions scriptFunction;

bool RegisterFunctions()
{
	return Mono_Init();
}

void* LoadDLL(const char* dllName)
{
#ifdef _WIN32
	return LoadLibrary(dllName);
#else
	return dlopen(LibMono, RTLD_LAZY);
#endif
}

void* GetFunctionAddress(void* handle, const char* funcName)
{
#ifdef _WIN32
	return GetProcAddress((HMODULE)handle, funcName);
#else
	return dlsym(handle, funcName);
#endif
}

void PrintCurrentErrorString()
{
#ifdef _WIN32
	LPTSTR lpMsgBuf = NULL;
	DWORD code = GetLastError();

	if (FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_IGNORE_INSERTS, NULL,
		code, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), lpMsgBuf, 0, NULL))
	{
		MessageBox(NULL, lpMsgBuf, "Error", MB_OK);
		LocalFree(lpMsgBuf);
	}
#else
	LOGE(dlerror());
#endif
}

#define LinkFuncAddress(member,type,name) \
	{ \
		scriptFunction.##member = (type)GetFunctionAddress(handle,name); \
		if(scriptFunction.##member == NULL) {LOGE("LinkFuncAddress Error: %s", name);PrintCurrentErrorString();return false;} \
	}

#ifdef UNITY_MONO
bool Mono_Init()
{
	void* handle = LoadDLL(LIBNAME);
	if (!handle)
	{
		LOGE("Unable to load library: %s", LIBNAME);
		PrintCurrentErrorString();
		return false;
	}

	LinkFuncAddress(script_profiler_install_allocation, F_script_profiler_install_allocation, PROFILER_INSTALL_ALLOCATION);
	LinkFuncAddress(script_profiler_install_enter_leave, F_script_profiler_install_enter_leave, PROFILER_INSTALL_ENTER_LEAVE);
	LinkFuncAddress(script_object_get_size, F_script_object_get_size, OBJECT_GET_SIZE);
	LinkFuncAddress(script_class_get_name, F_script_class_get_name, CLASS_GET_NAME);
	LinkFuncAddress(script_method_get_name, F_script_method_get_name, METHOD_GET_NAME);
	LinkFuncAddress(capture_memory_snapshot, F_capture_memory_snapshot, CAPTURE_MEMORY_SNAPSHOT);
	LinkFuncAddress(free_captured_memory_snapshot, F_free_captured_memory_snapshot, FREE_CAPTURED_MEMORY_SNAPSHOT);

	LinkFuncAddress(gc_get_heap_size, F_gc_get_heap_size, GC_GET_HEAP_SIZE);
	LinkFuncAddress(gc_get_used_size, F_gc_get_used_size, GC_GET_USED_SIZE);
	LinkFuncAddress(gc_get_smallobj_freelist_size, F_gc_get_smallobj_freelist_size, GC_GET_SMALLOBJ_FREELIST_SIZE);
	LinkFuncAddress(gc_get_wasted_since_gc, F_gc_get_wasted_since_gc, GC_GET_WASTED_SINCE_GC);

	return true;
}
#endif