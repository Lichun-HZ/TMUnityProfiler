using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System;
public class ComHUDDebugger : MonoBehaviour
{
    /// <summary>
    /// 是否允许调试
    /// </summary>
    public bool AllowDebugging = true;
    private DebugType _debugType = DebugType.Console;
    private List<LogData> _logInformations = new List<LogData>();
    private int _currentLogIndex = -1;
    private int _infoLogCount = 0;
    private int _warningLogCount = 0;
    private int _errorLogCount = 0;
    private int _fatalLogCount = 0;
    private bool _showInfoLog = true;
    private bool _showWarningLog = true;
    private bool _showErrorLog = true;
    private bool _showFatalLog = true;
    private Vector2 _scrollLogView = Vector2.zero;
    private Vector2 _scrollCurrentLogView = Vector2.zero;
    private Vector2 _scrollSystemView = Vector2.zero;
    private Vector2 _scrollAssetView = Vector2.zero;
    private Vector2 _scrollAssetPackageView = Vector2.zero;
    private bool _expansion = false;
    private Rect _windowRect = new Rect(0, 0, 100, 60);
    private int _fps = 0;
    private Color _fpsColor = Color.white;
    private int _frameNumber = 0;
    private float _lastShowFPSTime = 0f;
    private int _RunningTaskLimit = 2;
    private float _WindowScale = 1f;
    private List<string> _AssetList = new List<string>();
    private List<string> _AssetPackageList = new List<string>();

    private GameObject _UIRoot = null;

    private void Start()
    {
        if (AllowDebugging)
        {
            Application.logMessageReceived += LogHandler;
        }
    }
    private void Update()
    {
        if (AllowDebugging)
        {
            _frameNumber += 1;
            float time = Time.realtimeSinceStartup - _lastShowFPSTime;
            if (time >= 1)
            {
                _fps = (int)(_frameNumber / time);
                _frameNumber = 0;
                _lastShowFPSTime = Time.realtimeSinceStartup;
            }
        }
    }
    private void OnDestory()
    {
        if (AllowDebugging)
        {
            Application.logMessageReceived -= LogHandler;
        }
    }
    private void LogHandler(string condition, string stackTrace, LogType type)
    {
        LogData log = new LogData();
        log.time = DateTime.Now.ToString("HH:mm:ss");
        log.message = condition;
        log.stackTrace = stackTrace;
        if (type == LogType.Assert)
        {
            log.type = "Fatal";
            _fatalLogCount += 1;
        }
        else if (type == LogType.Exception || type == LogType.Error)
        {
            log.type = "Error";
            _errorLogCount += 1;
        }
        else if (type == LogType.Warning)
        {
            log.type = "Warning";
            _warningLogCount += 1;
        }
        else if (type == LogType.Log)
        {
            log.type = "Info";
            _infoLogCount += 1;
        }
        _logInformations.Add(log);
        ///if (_warningLogCount > 0)
        ///{
        ///    _fpsColor = Color.yellow;
        ///}
        ///if (_errorLogCount > 0)
        ///{
        ///    _fpsColor = Color.red;
        ///}
    }
    private void OnGUI()
    {
        Matrix4x4 cachedMatrix = GUI.matrix;

        _WindowScale = _TouchScale(_WindowScale);
        GUI.matrix = Matrix4x4.Scale(new Vector3(_WindowScale, _WindowScale, 1f));

        if (_fps >= 18)
        {
            _fpsColor = Color.Lerp(Color.yellow, Color.green, (_fps - 18) / 12f);
        }
        else if (_fps >= 10)
        {
            _fpsColor = Color.Lerp(Color.red, Color.yellow, (_fps - 10) / 8f);
        }
        else
            _fpsColor = Color.red;

        if (AllowDebugging)
        {
            if (_expansion)
            {

                _windowRect = GUI.Window(0, _windowRect, ExpansionGUIWindow, "DEBUGGER");
            }
            else
            {
                _windowRect = GUI.Window(0, _windowRect, ShrinkGUIWindow, "DEBUGGER");
            }
        }

        GUI.matrix = cachedMatrix;
    }
    private void ExpansionGUIWindow(int windowId)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 40));
        GUILayout.Space(20);
        #region title
        GUILayout.BeginHorizontal();
        GUI.contentColor = _fpsColor;
        if (GUILayout.Button("FPS:" + _fps, GUILayout.Height(30)))
        {
            _expansion = false;
            _windowRect.width = 100;
            _windowRect.height = 60;
        }
        GUI.contentColor = (_debugType == DebugType.Console ? Color.white : Color.gray);
        if (GUILayout.Button("Console", GUILayout.Height(30)))
        {
            _debugType = DebugType.Console;
        }
        GUI.contentColor = (_debugType == DebugType.Memory ? Color.white : Color.gray);
        if (GUILayout.Button("Memory", GUILayout.Height(30)))
        {
            _debugType = DebugType.Memory;
        }
        GUI.contentColor = (_debugType == DebugType.System ? Color.white : Color.gray);
        if (GUILayout.Button("System", GUILayout.Height(30)))
        {
            _debugType = DebugType.System;
        }
        GUI.contentColor = (_debugType == DebugType.Screen ? Color.white : Color.gray);
        if (GUILayout.Button("Screen", GUILayout.Height(30)))
        {
            _debugType = DebugType.Screen;
        }
        GUI.contentColor = (_debugType == DebugType.Quality ? Color.white : Color.gray);
        if (GUILayout.Button("Quality", GUILayout.Height(30)))
        {
            _debugType = DebugType.Quality;
        }
        GUI.contentColor = (_debugType == DebugType.Environment ? Color.white : Color.gray);
        if (GUILayout.Button("Environment", GUILayout.Height(30)))
        {
            _debugType = DebugType.Environment;
        }
        GUI.contentColor = (_debugType == DebugType.Asset ? Color.white : Color.gray);
        if (GUILayout.Button("Asset", GUILayout.Height(30)))
        {
            _debugType = DebugType.Asset;
        }
        GUI.contentColor = Color.white;
        GUILayout.EndHorizontal();
        #endregion
        #region console
        if (_debugType == DebugType.Console)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear"))
            {
                _logInformations.Clear();
                _fatalLogCount = 0;
                _warningLogCount = 0;
                _errorLogCount = 0;
                _infoLogCount = 0;
                _currentLogIndex = -1;
                _fpsColor = Color.white;
            }
            GUI.contentColor = (_showInfoLog ? Color.white : Color.gray);
            _showInfoLog = GUILayout.Toggle(_showInfoLog, "Info [" + _infoLogCount + "]");
            GUI.contentColor = (_showWarningLog ? Color.white : Color.gray);
            _showWarningLog = GUILayout.Toggle(_showWarningLog, "Warning [" + _warningLogCount + "]");
            GUI.contentColor = (_showErrorLog ? Color.white : Color.gray);
            _showErrorLog = GUILayout.Toggle(_showErrorLog, "Error [" + _errorLogCount + "]");
            GUI.contentColor = (_showFatalLog ? Color.white : Color.gray);
            _showFatalLog = GUILayout.Toggle(_showFatalLog, "Fatal [" + _fatalLogCount + "]");
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();

            GUI.SetNextControlName("_scrollLogView");
            _scrollLogView = GUILayout.BeginScrollView(_scrollLogView, "Box", GUILayout.Height(165));

            if("_scrollLogView" == GUI.GetNameOfFocusedControl())
                _scrollLogView = _TouchRoll(_scrollLogView);
            for (int i = 0; i < _logInformations.Count; i++)
            {
                bool show = false;
                Color color = Color.white;
                switch (_logInformations[i].type)
                {
                    case "Fatal":
                        show = _showFatalLog;
                        color = Color.red;
                        break;
                    case "Error":
                        show = _showErrorLog;
                        color = Color.red;
                        break;
                    case "Info":
                        show = _showInfoLog;
                        color = Color.white;
                        break;
                    case "Warning":
                        show = _showWarningLog;
                        color = Color.yellow;
                        break;
                    default:
                        break;
                }
                if (show)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Toggle(_currentLogIndex == i, ""))
                    {
                        _currentLogIndex = i;
                    }
                    GUI.contentColor = color;
                    GUILayout.Label("[" + _logInformations[i].type + "] ");
                    GUILayout.Label("[" + _logInformations[i].time + "] ");
                    GUILayout.Label(_logInformations[i].message);
                    GUILayout.FlexibleSpace();
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
            GUI.SetNextControlName("_scrollCurrentLogView");
            _scrollCurrentLogView = GUILayout.BeginScrollView(_scrollCurrentLogView, "Box", GUILayout.Height(100));
            if("_scrollCurrentLogView" == GUI.GetNameOfFocusedControl())
                _scrollCurrentLogView = _TouchRoll(_scrollCurrentLogView);
            if (_currentLogIndex != -1)
            {
                GUILayout.Label(_logInformations[_currentLogIndex].message + "\r\n\r\n" + _logInformations[_currentLogIndex].stackTrace);
            }
            GUILayout.EndScrollView();
        }
        #endregion
        #region memory
        else if (_debugType == DebugType.Memory)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Memory Information");
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("Box");

            long MBbytes = 1024 * 1024;
            GUILayout.Label("总内存：" + Profiler.GetTotalReservedMemoryLong() / MBbytes + "MB");
            GUILayout.Label("已占用内存：" + Profiler.GetTotalAllocatedMemoryLong() / MBbytes + "MB");
            GUILayout.Label("空闲中内存：" + Profiler.GetTotalUnusedReservedMemoryLong() / MBbytes + "MB");
            GUILayout.Label("总Mono堆内存：" + Profiler.GetMonoHeapSizeLong() / MBbytes + "MB");
            GUILayout.Label("已占用Mono堆内存：" + Profiler.GetMonoUsedSizeLong() / MBbytes + "MB");

            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("卸载未使用的资源"))
            {
                Resources.UnloadUnusedAssets();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("使用GC垃圾回收"))
            {
                GC.Collect();
            }
            GUILayout.EndHorizontal();
        }
        #endregion
        #region system
        else if (_debugType == DebugType.System)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("System Information");
            GUILayout.EndHorizontal();
            _scrollSystemView = GUILayout.BeginScrollView(_scrollSystemView, "Box");
            GUILayout.Label("操作系统：" + SystemInfo.operatingSystem);
            GUILayout.Label("系统内存：" + SystemInfo.systemMemorySize + "MB");
            GUILayout.Label("处理器：" + SystemInfo.processorType);
            GUILayout.Label("处理器数量：" + SystemInfo.processorCount);
            GUILayout.Label("显卡：" + SystemInfo.graphicsDeviceName);
            GUILayout.Label("显卡类型：" + SystemInfo.graphicsDeviceType);
            GUILayout.Label("显存：" + SystemInfo.graphicsMemorySize + "MB");
            GUILayout.Label("显卡标识：" + SystemInfo.graphicsDeviceID);
            GUILayout.Label("显卡供应商：" + SystemInfo.graphicsDeviceVendor);
            GUILayout.Label("显卡供应商标识码：" + SystemInfo.graphicsDeviceVendorID);
            GUILayout.Label("设备模式：" + SystemInfo.deviceModel);
            GUILayout.Label("设备名称：" + SystemInfo.deviceName);
            GUILayout.Label("设备类型：" + SystemInfo.deviceType);
            GUILayout.Label("设备标识：" + SystemInfo.deviceUniqueIdentifier);
            GUILayout.EndScrollView();
        }
        #endregion
        #region screen
        else if (_debugType == DebugType.Screen)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Screen Information");
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("Box");
            GUILayout.Label("DPI：" + Screen.dpi);
            GUILayout.Label("分辨率：" + Screen.currentResolution.ToString());
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("全屏"))
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, !Screen.fullScreen);
            }
            GUILayout.EndHorizontal();
        }
        #endregion
        #region Quality
        else if (_debugType == DebugType.Quality)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Quality Information");
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("Box");
            string value = "";
            if (QualitySettings.GetQualityLevel() == 0)
            {
                value = " [最低]";
            }
            else if (QualitySettings.GetQualityLevel() == QualitySettings.names.Length - 1)
            {
                value = " [最高]";
            }
            GUILayout.Label("图形质量：" + QualitySettings.names[QualitySettings.GetQualityLevel()] + value);
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("降低一级图形质量"))
            {
                QualitySettings.DecreaseLevel();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("提升一级图形质量"))
            {
                QualitySettings.IncreaseLevel();
            }
            GUILayout.EndHorizontal();
        }
        #endregion
        #region Environment
        else if (_debugType == DebugType.Environment)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Environment Information");
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("Box");
            GUILayout.Label("项目名称：" + Application.productName);
            GUILayout.Label("项目ID：" + Application.identifier);
            GUILayout.Label("项目版本：" + Application.version);
            GUILayout.Label("Unity版本：" + Application.unityVersion);
            GUILayout.Label("公司名称：" + Application.companyName);
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("退出程序"))
            {
                Application.Quit();
            }
            if (GUILayout.Button("隐藏UI"))
            {
                if (null == _UIRoot)
                    _UIRoot = GameObject.Find("UIRoot");

                if (null != _UIRoot)
                    _UIRoot.SetActive(!_UIRoot.activeSelf);
            }
            GUILayout.EndHorizontal();
        }
        #endregion
        #region Asset
        else if (_debugType == DebugType.Asset)
        {
            /*GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("当前代理数量：{0}", _RunningTaskLimit));

            if (GUILayout.Button("增加代理数量"))
            {
                ++_RunningTaskLimit;

                AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.RunningTaskLimit = _RunningTaskLimit;
                AsyncLoadTaskAllocator<AssetBundleCreateRequestWrapper, AssetBundle>.instance.RunningTaskLimit = _RunningTaskLimit;
                AsyncLoadTaskAllocator<AssetBundleResquestWrapper, UnityEngine.Object>.instance.RunningTaskLimit = _RunningTaskLimit;

                _RunningTaskLimit = AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.RunningTaskLimit;
            }
            if (GUILayout.Button("减少代理数量"))
            {
                --_RunningTaskLimit;

                AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.RunningTaskLimit = _RunningTaskLimit;
                AsyncLoadTaskAllocator<AssetBundleCreateRequestWrapper, AssetBundle>.instance.RunningTaskLimit = _RunningTaskLimit;
                AsyncLoadTaskAllocator<AssetBundleResquestWrapper, UnityEngine.Object>.instance.RunningTaskLimit = _RunningTaskLimit;

                _RunningTaskLimit = AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.RunningTaskLimit;
            }
            
            if (GUILayout.Button("Snap Asset Usage"))
            {
                AssetLoader.instance.DumpAssetInfo(ref _AssetList);
                AssetPackageManager.instance.DumpAssetPackageInfo(ref _AssetPackageList);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("已加载资源数量：{0}", _AssetList.Count));
            GUILayout.Label(string.Format("已加载资源包数量：{0}", _AssetPackageList.Count));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            int assetWaitingTaskCount = AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.LoadingTaskCount + AsyncLoadTaskAllocator<AssetBundleResquestWrapper, UnityEngine.Object>.instance.LoadingTaskCount;
            int assetLoadingTaskCount = AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.RunningTaskCount + AsyncLoadTaskAllocator<AssetBundleResquestWrapper, UnityEngine.Object>.instance.RunningTaskCount;
            int assetPackageWaitingTaskCount = AsyncLoadTaskAllocator<AssetBundleCreateRequestWrapper, AssetBundle>.instance.LoadingTaskCount;
            int assetPackageLoadingTaskCount = AsyncLoadTaskAllocator<AssetBundleCreateRequestWrapper, AssetBundle>.instance.RunningTaskCount;

            int assetCompeleteTaskCount = AsyncLoadTaskAllocator<ResourceRequestWrapper, UnityEngine.Object>.instance.CompleteTaskCount + AsyncLoadTaskAllocator<AssetBundleResquestWrapper, UnityEngine.Object>.instance.CompleteTaskCount;
            int assetPackageCompeleteTaskCount = AsyncLoadTaskAllocator<AssetBundleCreateRequestWrapper, AssetBundle>.instance.CompleteTaskCount;

            int goPoolLoadingTaskCount = CGameObjectPool.instance.LoadingTaskCount;
            int goPoolCompeleteTaskCount = CGameObjectPool.instance.CompleteTaskCount;

            int assetLoaderLoadingTaskCount = AssetLoader.instance.LoadingTaskCount;
            int assetLoaderCompeleteTaskCount = AssetLoader.instance.LoadingTaskCount;

            GUILayout.Label(string.Format("Asset:{0} ({2}) [{1}]", assetWaitingTaskCount, assetCompeleteTaskCount, assetLoadingTaskCount));
            GUILayout.Label(string.Format("AssetBundle:{0} ({2}) [{1}]", assetPackageWaitingTaskCount, assetPackageCompeleteTaskCount, assetPackageLoadingTaskCount));
            GUILayout.Label(string.Format("GameObjPool:{0} [{1}]", goPoolLoadingTaskCount, goPoolCompeleteTaskCount));
            GUILayout.Label(string.Format("AssetLoader:{0} [{1}]", assetLoaderLoadingTaskCount, assetLoaderCompeleteTaskCount));
            GUILayout.EndHorizontal();

            GUI.SetNextControlName("_scrollAssetView");
            _scrollAssetView = GUILayout.BeginScrollView(_scrollAssetView, "Box", GUILayout.Height(100));

            if ("_scrollAssetView" == GUI.GetNameOfFocusedControl())
                _scrollAssetView = _TouchRoll(_scrollAssetView);

            for (int i = 0; i < _AssetList.Count; i++)
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label(_AssetList[i]);
                GUILayout.FlexibleSpace();
                GUI.contentColor = Color.white;
            }

            GUILayout.EndScrollView();

            GUI.SetNextControlName("_scrollAssetPackageView");
            _scrollAssetPackageView = GUILayout.BeginScrollView(_scrollAssetPackageView, "Box", GUILayout.Height(100));
            if ("_scrollAssetPackageView" == GUI.GetNameOfFocusedControl())
                _scrollAssetPackageView = _TouchRoll(_scrollAssetPackageView);

            for (int i = 0; i < _AssetPackageList.Count; i++)
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label(_AssetPackageList[i]);
                GUILayout.FlexibleSpace();
                GUI.contentColor = Color.white;
            }

            GUILayout.EndScrollView();*/
        }
        #endregion
    }


    private void ShrinkGUIWindow(int windowId)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
        GUI.contentColor = _fpsColor;
        if (GUILayout.Button("FPS:" + _fps, GUILayout.Width(80), GUILayout.Height(30)))
        {
            _expansion = true;
            _windowRect.width = 600;
            _windowRect.height = 360;
        }
        GUI.contentColor = Color.white;
    }

    private Vector2 _TouchRoll(Vector2 input)
    {
        Vector2 newPos = input;
        if(2 == Input.touchCount)
        {
            newPos += (Input.touches[0].deltaPosition + Input.touches[1].deltaPosition) * 0.5f;
        }

        return newPos;
    }

    private float _TouchScale(float origin)
    {
        float originScale = origin;
        if (2 == Input.touchCount)
        {
            originScale += Mathf.Sqrt(Vector2.SqrMagnitude(Input.touches[0].deltaPosition - Input.touches[1].deltaPosition));
        }

        return origin;
    }

    public struct LogData
    {
        public string time;
        public string type;
        public string message;
        public string stackTrace;
    }
    public enum DebugType
    {
        Console,
        Memory,
        System,
        Screen,
        Quality,
        Environment,
        Asset,
    }
}
