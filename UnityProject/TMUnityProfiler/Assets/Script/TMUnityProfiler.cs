using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TMUnityProfiler : MonoBehaviour
{

#if UNITY_IPHONE
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the library name.
       [DllImport ("__Internal")]
#else
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("TMUnityProfiler")]
#endif
    private static extern uint GetTotalAllocSize();

#if UNITY_IPHONE
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the library name.
       [DllImport ("__Internal")]
#else
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("TMUnityProfiler")]
#endif
    private static extern void RegisterProfilerFunction();

#if UNITY_IPHONE
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the library name.
       [DllImport ("__Internal")]
#else
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("TMUnityProfiler")]
#endif
    private static extern bool InitProfiler();


#if UNITY_IPHONE
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the library name.
       [DllImport ("__Internal")]
#else
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("TMUnityProfiler")]
#endif
    extern static int GetLastMethodIn(ref byte str);

#if UNITY_IPHONE
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the library name.
       [DllImport ("__Internal")]
#else
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("TMUnityProfiler")]
#endif
    extern static int ManagedMemorySnapshot(ref byte str);

#if UNITY_IPHONE
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the library name.
       [DllImport ("__Internal")]
#else
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("TMUnityProfiler")]
#endif
    extern static int AddTwoIntegers(int a, int b);

    private uint m_TotalGCSize = 0;
    private string m_LastMethodIn = "";
    private string m_LoadAssemblyName = "";

    private Rect m_Rect = new Rect(10, 10, 200, 50);
    private Rect m_RectMethod = new Rect(10, 80, 200, 50);
    private Rect m_RectMethod2 = new Rect(10, 150, 500, 100);
    private Rect m_RectMethod3 = new Rect(10, 260, 100, 100);


    private byte[] s = new byte[1024];
    private byte[] s1 = new byte[1024];

    bool bStarted = false;

    // Use this for initialization

    void Start()
    {
        if(!InitProfiler())
        {
            Debug.LogErrorFormat("InitProfiler Error!");
        }

        RegisterProfilerFunction();

        //Debug.LogErrorFormat("RegisterProfilerFunction");
        bStarted = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!bStarted)
        {
            //Debug.LogErrorFormat("RegisterProfilerFunction in Update");
            bStarted = true;
        }

        m_TotalGCSize = (uint)GetTotalAllocSize();

        int iSize = GetLastMethodIn(ref s[0]);//用字节数组接收动态库传过来的字符串
        if (iSize > 0)
        {
            m_LastMethodIn = System.Text.Encoding.Default.GetString(s, 0, s.Length); //将字节数组转换为字符串
            //Debug.LogError("iSize:\t" + iSize + "\t m_LastMethodIn:\t" + m_LastMethodIn);
        }

    }
    int clickTime = 0;
    private void OnGUI()
    {
        GUI.Label(m_Rect, m_TotalGCSize.ToString());
        GUI.Label(m_RectMethod, m_LastMethodIn);
        GUI.Label(m_RectMethod2, m_LoadAssemblyName);
        if (GUI.Button(m_RectMethod3, "收集内存快照"))
        {
            int iSize = ManagedMemorySnapshot(ref s1[0]);
            if (iSize > 0)
            {
                clickTime++;
                m_LoadAssemblyName = System.Text.Encoding.Default.GetString(s1, 0, s1.Length) + "   " + clickTime.ToString(); //将字节数组转换为字符串
                //Debug.LogError("iSize:\t" + iSize + "\t m_LoadAssemblyName:\t" + m_LoadAssemblyName);
            }
        }
    }
}
