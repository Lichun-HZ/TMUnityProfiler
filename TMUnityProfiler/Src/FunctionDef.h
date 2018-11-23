#pragma once

typedef void(*F_script_profiler_install_allocation)(gpointer callback);
typedef void(*F_script_profiler_install_enter_leave)(gpointer enter, gpointer fleave);

typedef guint(*F_script_object_get_size)(gpointer object);
typedef const char*(*F_script_class_get_name)(gpointer kClass);
typedef const char*(*F_script_method_get_name)(gpointer method);

typedef gpointer(*F_capture_memory_snapshot)();
typedef void(*F_free_captured_memory_snapshot)(gpointer snapshot);

typedef gint64(*F_gc_get_heap_size)(void);
typedef gint64(*F_gc_get_used_size)(void);
typedef gint64(*F_gc_get_smallobj_freelist_size)(void);
typedef gint64(*F_gc_get_wasted_since_gc)(void);

#ifdef UNITY_MONO
#define PROFILER_INSTALL_ALLOCATION  "mono_profiler_install_allocation"
#define PROFILER_INSTALL_ENTER_LEAVE "mono_profiler_install_enter_leave"
#define OBJECT_GET_SIZE  "mono_object_get_size"
#define CLASS_GET_NAME   "mono_class_get_name"
#define METHOD_GET_NAME  "mono_method_get_name"

#define CAPTURE_MEMORY_SNAPSHOT			"mono_unity_capture_memory_snapshot"
#define FREE_CAPTURED_MEMORY_SNAPSHOT   "mono_unity_free_captured_memory_snapshot"

#define GC_GET_HEAP_SIZE		  		"mono_gc_get_heap_size"
#define GC_GET_USED_SIZE		  		"mono_gc_get_used_size"
#define GC_GET_SMALLOBJ_FREELIST_SIZE	"mono_gc_get_smallobj_freelist_size"
#define GC_GET_WASTED_SINCE_GC			"mono_gc_get_words_wasted_since_gc"
#elif UNITY_IL2CPP
#endif

class ScriptFunctions
{
public:
	F_script_profiler_install_allocation  script_profiler_install_allocation;
	F_script_profiler_install_enter_leave script_profiler_install_enter_leave;

	F_script_object_get_size script_object_get_size;
	F_script_class_get_name  script_class_get_name;
	F_script_method_get_name script_method_get_name;

	F_capture_memory_snapshot capture_memory_snapshot;
	F_free_captured_memory_snapshot free_captured_memory_snapshot;

	F_gc_get_heap_size gc_get_heap_size;
	F_gc_get_used_size gc_get_used_size;
	F_gc_get_smallobj_freelist_size gc_get_smallobj_freelist_size;
	F_gc_get_wasted_since_gc gc_get_wasted_since_gc;
};

extern ScriptFunctions scriptFunction;