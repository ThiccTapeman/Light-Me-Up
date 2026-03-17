using System.Collections.Generic;
using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using ThiccTapeman.Input;
using UnityEditor;
using UnityEngine;

namespace Editor.ReactiveWorld
{
    /// <summary>
    /// A debug overlay for inspecting and interacting with the Reactive World system at runtime.
    /// Displays registered reactors, their details, and performance metrics. Toggle with F9.
    /// </summary>

    [InitializeOnLoad]
    internal static class ReactiveWorldDebugOverlayBootstrap
    {
        private const string OverlayObjectName = "ReactiveWorldDebugOverlay";
        private static ReactiveWorldDebugOverlay _overlay;

        static ReactiveWorldDebugOverlayBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EnsureOverlay();
                return;
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
                DestroyOverlay();
        }

        private static void EnsureOverlay()
        {
            if (_overlay != null)
                return;

            var overlayObject = new GameObject(OverlayObjectName)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            Object.DontDestroyOnLoad(overlayObject);
            _overlay = overlayObject.AddComponent<ReactiveWorldDebugOverlay>();
        }

        private static void DestroyOverlay()
        {
            if (_overlay == null)
                return;

            Object.DestroyImmediate(_overlay.gameObject);
            _overlay = null;
        }
    }

    internal enum ReactorGroupMode
    {
        None,
        ByArea,
        ByType
    }

    internal enum ReactorOrderMode
    {
        ByName,
        ByEnabled
    }

    internal enum SortDirection
    {
        Ascending,
        Descending
    }

    internal enum PerformanceSortMode
    {
        ByDelay,
        ByCalls
    }

    internal sealed partial class ReactiveWorldDebugOverlay : MonoBehaviour
    {
        private enum ReactorDetailTab
        {
            Trace,
            Overview,
            Stats
        }

        private enum OverlayMainTab
        {
            List,
            Performance
        }

        private readonly List<IReactor> _reactorBuffer = new();
        private readonly List<IReactor> _performanceBuffer = new();
        private readonly Dictionary<string, bool> _expandedAreas = new();
        private readonly Dictionary<string, bool> _expandedTypes = new();
        private readonly Dictionary<string, bool> _expandedTraceEvents = new();
        private readonly Dictionary<string, bool> _expandedTraceCalls = new();

        private GUIStyle _windowStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _subtleLabelStyle;
        private GUIStyle _tabStyle;
        private GUIStyle _activeTabStyle;
        private GUIStyle _pillStyle;
        private GUIStyle _searchFieldStyle;
        private GUIStyle _faultedRowStyle;
        private GUIStyle _faultedSectionStyle;
        private GUIStyle _stackTraceStyle;
        private GUIStyle _redLabelStyle;
        private GUIStyle _richLabelStyle;
        private GUIStyle _linkStyle;

        private InputItem _toggleOverlayInput;
        private InputItem _freezeTimeLeftControlInput;
        private InputItem _freezeTimeRightControlInput;
        private Vector2 _mainScrollPosition;
        private Vector2 _detailScrollPosition;
        private bool _isVisible = true;
        private IReactor _selectedReactor;
        private OverlayMainTab _selectedReactorSourceTab = OverlayMainTab.List;
        private ReactorGroupMode _groupMode = ReactorGroupMode.ByArea;
        private ReactorOrderMode _orderMode = ReactorOrderMode.ByName;
        private SortDirection _orderDirection = SortDirection.Ascending;
        private ReactorGroupMode _performanceGroupMode = ReactorGroupMode.None;
        private ReactorOrderMode _performanceOrderMode = ReactorOrderMode.ByName;
        private PerformanceSortMode _performanceSortMode = PerformanceSortMode.ByDelay;
        private SortDirection _performanceSortDirection = SortDirection.Descending;
        private ReactorDetailTab _detailTab = ReactorDetailTab.Stats;
        private OverlayMainTab _mainTab = OverlayMainTab.List;
        private string _searchTerm = string.Empty;
        private string _performanceSearchTerm = string.Empty;
        private bool _isTimeFrozenByOverlay;
        private float _timeScaleBeforeFreeze = 1f;

        private void Update()
        {
            EnsureInput();

            if (_toggleOverlayInput != null && _toggleOverlayInput.Triggered())
                _isVisible = !_isVisible;

            UpdateTimeFreeze();
        }

        private void OnDisable()
        {
            RestoreTimeScale();
        }

        private void OnGUI()
        {
            if (!_isVisible || !Application.isPlaying)
                return;

            var worldManager = FindFirstObjectByType<WorldManager>();
            if (worldManager == null)
                return;

            SnapshotReactors(worldManager);
            ValidateSelection();
            EnsureStyles();

            const float width = 420f;
            var area = new Rect(12f, 12f, width, Mathf.Min(Screen.height - 24f, Screen.height * 0.82f));

            GUILayout.BeginArea(area, "Reactive World Debug", _windowStyle);

            if (_selectedReactor != null)
                DrawDetailView(worldManager, _selectedReactor);
            else
                DrawMainView(worldManager);

            GUILayout.EndArea();
        }

        private void EnsureInput()
        {
            if (_toggleOverlayInput != null &&
                _freezeTimeLeftControlInput != null &&
                _freezeTimeRightControlInput != null)
                return;

            _toggleOverlayInput = InputManager.GetInstance().GetTempAction("ReactiveWorldDebug.ToggleOverlay", "<Keyboard>/f9");
            _freezeTimeLeftControlInput = InputManager.GetInstance().GetTempAction("ReactiveWorldDebug.FreezeTimeLeftControl", "<Keyboard>/leftCtrl");
            _freezeTimeRightControlInput = InputManager.GetInstance().GetTempAction("ReactiveWorldDebug.FreezeTimeRightControl", "<Keyboard>/rightCtrl");
        }

        private void SnapshotReactors(WorldManager worldManager)
        {
            _reactorBuffer.Clear();
            _performanceBuffer.Clear();

            foreach (var reactor in worldManager.Reactors)
            {
                _performanceBuffer.Add(reactor);

                if (MatchesSearch(reactor, _searchTerm))
                    _reactorBuffer.Add(reactor);
            }

            SortReactors(_reactorBuffer, _groupMode, _orderMode, _orderDirection);
            SortPerformanceReactors(worldManager, _performanceBuffer, _performanceSortMode, _performanceOrderMode, _performanceSortDirection);
        }

        private void ValidateSelection()
        {
            if (_selectedReactor == null)
                return;

            if (!_performanceBuffer.Contains(_selectedReactor))
                _selectedReactor = null;
        }

        private void EnsureStyles()
        {
            if (_windowStyle != null)
                return;

            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(14, 14, 16, 14)
            };

            _sectionStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 6, 6)
            };

            _rowStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 4, 4)
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            _subtleLabelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(0.78f, 0.78f, 0.78f) }
            };

            _tabStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 28
            };

            _activeTabStyle = new GUIStyle(_tabStyle)
            {
                fontStyle = FontStyle.Bold
            };

            _pillStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 24,
                fontSize = 11
            };

            _searchFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fixedHeight = 24
            };

            _faultedRowStyle = new GUIStyle(_rowStyle);
            _faultedSectionStyle = new GUIStyle(_sectionStyle);
            _stackTraceStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = false,
                richText = false
            };

            _redLabelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(1f, 0.35f, 0.35f) },
                fontStyle = FontStyle.Bold
            };

            _richLabelStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                wordWrap = true
            };

            _linkStyle = new GUIStyle(EditorStyles.linkLabel)
            {
                richText = true
            };
        }

        private void UpdateTimeFreeze()
        {
            var shouldFreeze = _isVisible && IsFreezeInputPressed();
            if (shouldFreeze)
            {
                if (_isTimeFrozenByOverlay)
                    return;

                _timeScaleBeforeFreeze = Time.timeScale;
                Time.timeScale = 0f;
                _isTimeFrozenByOverlay = true;
                return;
            }

            RestoreTimeScale();
        }

        private bool IsFreezeInputPressed()
        {
            return IsInputPressed(_freezeTimeLeftControlInput) || IsInputPressed(_freezeTimeRightControlInput);
        }

        private static bool IsInputPressed(InputItem inputItem)
        {
            if (inputItem == null)
                return false;

            return inputItem.ReadValue<float>() > 0.5f;
        }

        private void RestoreTimeScale()
        {
            if (!_isTimeFrozenByOverlay)
                return;

            Time.timeScale = _timeScaleBeforeFreeze;
            _isTimeFrozenByOverlay = false;
        }
    }
}
