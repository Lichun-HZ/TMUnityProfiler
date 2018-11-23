#if _MSC_VER // this is defined when compiling with Visual Studio
#define EXPORT_API __declspec(dllexport) // Visual Studio needs annotating exported functions with this
#else
#define EXPORT_API // XCode does not need annotating exported functions, so define is empty
#endif

#include "Init.h"
#include "mono_memory_info.h"

#if _MSC_VER
#include<string>
#else
#include<string.h>
#include<string>
#include<iosfwd>
#endif

#ifdef _WIN32
#include "windows.h"
#endif

/*

#ifdef UNITY_MONO
#include "mono/mini/jit.h"
#include "mono/metadata/profiler.h"
#elif UNITY_IL2CPP
#include "il2cpp-api.h"
#include "vm/MemoryInformation.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-api-types.h"
#endif*/

// ------------------------------------------------------------------------
// Plugin itself
// Link following functions C-style (required for plugins)
#ifdef __cplusplus
extern "C"
{
#endif

	const char* g_MethodIn = 0;
	const char* g_MethodOut = 0;
#ifdef UNITY_MONO
	guint g_totalAllocSize = 0;
#elif UNITY_IL2CPP
	Il2CppManagedMemorySnapshot* snapShot = NULL;
	uint32_t g_totalAllocSize = 1;
#endif

	EXPORT_API bool InitProfiler()
	{
		if (!RegisterFunctions())
			return false;

		return true;
	}

	EXPORT_API unsigned int GetTotalAllocSize()
	{
		return g_totalAllocSize;
	}

	static void AllocFunc(gpointer prof, gpointer obj, gpointer klass)
	{
		guint objSize = scriptFunction.script_object_get_size(obj);
		g_totalAllocSize += objSize;;
	}

	static void MethodEnter(gpointer prof, gpointer method)
	{
		g_MethodIn = scriptFunction.script_method_get_name(method);
	}

	static void MethodLeave(gpointer prof, gpointer method)
	{
		g_MethodOut = scriptFunction.script_method_get_name(method);
	}


	EXPORT_API int GetLastMethodIn(char* str)
	{
		char attr[1024];
		memset(attr, 0, sizeof(attr));

		if (g_MethodIn != 0)
		{
			int strSize = strlen(g_MethodIn);
			memcpy(attr, g_MethodIn, strSize);
			memcpy(str, attr, strSize);
			return strSize;
		}
		else
		{
			return 0;
		}
	}

#define TOMB(size) ( size * 1.0f/ (1024.0f * 1024.0f))

	EXPORT_API int ManagedMemorySnapshot(char* str)
	{
		char attr[1024];
		memset(attr, 0, sizeof(attr));

#ifdef UNITY_MONO

		MonoManagedMemorySnapshot* snapShot = (MonoManagedMemorySnapshot*)scriptFunction.capture_memory_snapshot();
		if(snapShot == NULL)
			return 0;

		int SectionSize = 0;
		MonoManagedMemorySection* sections = snapShot->heap.sections;
		for (int i = 0; i < snapShot->heap.sectionCount; i++)
		{
			SectionSize += sections->sectionSize;
			sections++;
		}

		LOGE("SectionSize: %.2f MB, UsedSize: %.2f MB, SmallObjFree: %.2f MB, Wasted: %.2f MB", TOMB(SectionSize), TOMB(scriptFunction.gc_get_used_size()),
			TOMB(scriptFunction.gc_get_smallobj_freelist_size()), TOMB(scriptFunction.gc_get_wasted_since_gc()));

		scriptFunction.free_captured_memory_snapshot(snapShot);

		return 1;

#elif UNITY_IL2CPP
		if (snapShot != NULL) {
			il2cpp_free_captured_memory_snapshot(snapShot);
		}
		snapShot = il2cpp_capture_memory_snapshot();
		Il2CppMetadataType* metaDataType = snapShot->metadata.types;
		const char *assemblyName = metaDataType->assemblyName;
		
		LOGE("metadata typesNum  %d", snapShot->metadata.typeCount);
		for (int i=0;i< snapShot->metadata.typeCount;i++)
		{
			LOGE("metadata:  %s", metaDataType->name);
			metaDataType++;
		}

		LOGE("SectionNum  %d", snapShot->heap.sectionCount);
		Il2CppManagedMemorySection* sections = snapShot->heap.sections;
		for (int i = 0; i < snapShot->heap.sectionCount; i++)
		{
			LOGE("SectionSize: %d, StartAddress: %x", sections->sectionSize, sections->sectionStartAddress);
			sections++;
		}

		if (assemblyName != 0)
		{
			int strSize = strlen(assemblyName);
			LOGE("assemblyName:-->>   %s", assemblyName);
			LOGE("sizeof(assemblyName) = %d", sizeof(strSize));
			memcpy(attr, assemblyName, strSize);
			memcpy(str, attr, strSize);
			return strSize;
		}
		else
		{
			return 0;
		}
#endif
	}

	EXPORT_API void RegisterProfilerFunction()
	{
		scriptFunction.script_profiler_install_allocation((gpointer)AllocFunc);
		scriptFunction.script_profiler_install_enter_leave((gpointer)MethodEnter, (gpointer)MethodLeave);
	}

#ifdef __cplusplus
} // end of export C block
#endif