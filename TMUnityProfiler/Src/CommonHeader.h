#pragma once

#include <stdint.h>

/* STDCALL on windows, CDECL everywhere else to work with XPCOM and MainWin COM */
#ifdef  _WIN32
#define STDCALL __stdcall
#include <stdio.h>
#include <tchar.h>
#else
#define STDCALL
#include <android/log.h>
#endif

/*
 * Basic data types
 */
typedef int            gint;
typedef unsigned int   guint;
typedef short          gshort;
typedef unsigned short gushort;
typedef long           glong;
typedef unsigned long  gulong;
typedef void *         gpointer;
typedef const void *   gconstpointer;
typedef char           gchar;
typedef unsigned char  guchar;

/* Types defined in terms of the stdint.h */
typedef int8_t         gint8;
typedef uint8_t        guint8;
typedef int16_t        gint16;
typedef uint16_t       guint16;
typedef int32_t        gint32;
typedef uint32_t       guint32;
typedef int64_t        gint64;
typedef uint64_t       guint64;
typedef float          gfloat;
typedef double         gdouble;
typedef int32_t        gboolean;

typedef guint16 gunichar2;
typedef guint32 gunichar;

typedef void * gpointer;

#define TAG "TMUnityProfiler"

#ifdef  _WIN32
	#ifdef UNITY_MONO
	#define LIBNAME "mono.dll"
	#endif
#define LOGD(...) 
#define LOGE(...)  {TCHAR sOut[256];_stprintf_s(sOut, __VA_ARGS__);MessageBox(NULL, sOut, "Error", MB_OK);}    
#else
	#ifdef UNITY_MONO
	#define LIBNAME "libmono.so"
	#endif
#define LOGD(...) ((void)__android_log_print(ANDROID_LOG_DEBUG, TAG, __VA_ARGS__))
#define LOGE(...) ((void)__android_log_print(ANDROID_LOG_ERROR, TAG, __VA_ARGS__))
#endif