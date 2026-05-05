using System.Text;
using Unity.Entities;
using UnityEngine;

namespace DOTSDemo.Shared
{
    /// <summary>
    /// Lightweight runtime HUD for DOTS demo scenes.
    ///
    /// Example scene presets:
    /// Demo01
    /// - demoName = "Moving Cubes"
    /// - techDescription = "IJobEntity + Burst + ECB\n1000+ parallel entities\nBoundary wrap"
    ///
    /// Demo02
    /// - demoName = "Bouncing Balls"
    /// - techDescription = "Unity Physics\nPhysicsCollider + PhysicsVelocity\nPhysicsDamping"
    ///
    /// Demo03
    /// - demoName = "Flocking Agents"
    /// - techDescription = "Boids algorithm\nSeparation + Alignment + Cohesion\nNeighbor sampling"
    ///
    /// Demo04
    /// - demoName = "Tower Defense"
    /// - techDescription = "ECS gameplay loop\nWave spawning + Tower targeting\nProjectile + Damage + Cleanup"
    ///
    /// Usage:
    /// 1. Create an empty GameObject in the scene root.
    /// 2. Attach DemoHUD.
    /// 3. Fill the inspector fields for the active demo.
    /// 4. Press Play to view the overlay in Game view.
    /// </summary>
    [AddComponentMenu("DOTS Demo/Demo HUD")]
    public sealed class DemoHUD : MonoBehaviour
    {
        private const string DefaultDemoName = "DOTS Demo";
        private const string FpsLabel = "FPS: ";
        private const string EntitiesLabel = "Entities: ";
        private const float FpsRefreshInterval = 0.5f;
        private const float EntityRefreshInterval = 1f;
        private const float Margin = 16f;
        private const float TopPanelWidth = 360f;
        private const float BottomPanelWidth = 520f;

        [SerializeField] private string demoName = DefaultDemoName;
        [SerializeField, TextArea(3, 6)] private string techDescription = string.Empty;
        [SerializeField] private string controlsHint = "Press R to reset";
        [SerializeField] private bool showEntityCount = true;

        private readonly StringBuilder _statsBuilder = new StringBuilder(64);
        private readonly GUIContent _demoNameContent = new GUIContent();
        private readonly GUIContent _statsContent = new GUIContent();
        private readonly GUIContent _techContent = new GUIContent();
        private readonly GUIContent _controlsContent = new GUIContent();

        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _hintStyle;

        private string _cachedDemoName;
        private string _cachedTechDescription;
        private string _cachedControlsHint;
        private bool _cachedShowEntityCount;

        private int _frameCounter;
        private float _fpsTimer;
        private float _displayedFps;
        private float _entityTimer;
        private int _entityCount;

        private World _cachedWorld;
        private EntityQuery _entityCountQuery;

        private void OnEnable()
        {
            _frameCounter = 0;
            _fpsTimer = 0f;
            _displayedFps = 0f;
            _entityTimer = EntityRefreshInterval;
            _entityCount = 0;
            RefreshStaticContent(force: true);
            RefreshStatsContent();
        }

        private void Update()
        {
            _frameCounter++;
            _fpsTimer += Time.unscaledDeltaTime;
            _entityTimer += Time.unscaledDeltaTime;

            if (_fpsTimer >= FpsRefreshInterval)
            {
                _displayedFps = _frameCounter / _fpsTimer;
                _frameCounter = 0;
                _fpsTimer = 0f;
                RefreshStatsContent();
            }

            if (_entityTimer >= EntityRefreshInterval)
            {
                _entityTimer = 0f;
                _entityCount = QueryEntityCount();
                RefreshStatsContent();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            RefreshStaticContent(force: false);

            float leftWidth = Mathf.Min(TopPanelWidth, Screen.width - (Margin * 2f));
            float rightWidth = leftWidth;
            float bottomWidth = Mathf.Min(BottomPanelWidth, Screen.width - (Margin * 2f));

            Rect leftPanelRect = new Rect(Margin, Margin, leftWidth, 108f);
            Rect titleRect = new Rect(leftPanelRect.x + 12f, leftPanelRect.y + 10f, leftPanelRect.width - 24f, 30f);
            Rect statsRect = new Rect(leftPanelRect.x + 12f, leftPanelRect.y + 44f, leftPanelRect.width - 24f, leftPanelRect.height - 52f);

            Rect rightPanelRect = new Rect(Screen.width - rightWidth - Margin, Margin, rightWidth, 108f);
            Rect rightTextRect = new Rect(rightPanelRect.x + 12f, rightPanelRect.y + 10f, rightPanelRect.width - 24f, rightPanelRect.height - 20f);

            Rect bottomPanelRect = new Rect((Screen.width - bottomWidth) * 0.5f, Screen.height - 52f - Margin, bottomWidth, 52f);
            Rect bottomTextRect = new Rect(bottomPanelRect.x + 12f, bottomPanelRect.y + 12f, bottomPanelRect.width - 24f, bottomPanelRect.height - 24f);

            GUI.Box(leftPanelRect, GUIContent.none, _panelStyle);
            GUI.Label(titleRect, _demoNameContent, _titleStyle);
            GUI.Label(statsRect, _statsContent, _bodyStyle);

            GUI.Box(rightPanelRect, GUIContent.none, _panelStyle);
            GUI.Label(rightTextRect, _techContent, _bodyStyle);

            GUI.Box(bottomPanelRect, GUIContent.none, _panelStyle);
            GUI.Label(bottomTextRect, _controlsContent, _hintStyle);
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelStyle = GUIStyleHelper.LightPanel();
            _titleStyle = GUIStyleHelper.TitleLabel(20);
            _bodyStyle = GUIStyleHelper.BodyLabel(13);
            _hintStyle = GUIStyleHelper.HintLabel(13);
        }

        private void RefreshStaticContent(bool force)
        {
            if (!force &&
                _cachedDemoName == demoName &&
                _cachedTechDescription == techDescription &&
                _cachedControlsHint == controlsHint &&
                _cachedShowEntityCount == showEntityCount)
            {
                return;
            }

            _cachedDemoName = demoName;
            _cachedTechDescription = techDescription;
            _cachedControlsHint = controlsHint;
            _cachedShowEntityCount = showEntityCount;

            _demoNameContent.text = string.IsNullOrWhiteSpace(demoName) ? DefaultDemoName : demoName;
            _techContent.text = techDescription ?? string.Empty;
            _controlsContent.text = controlsHint ?? string.Empty;

            RefreshStatsContent();
        }

        private void RefreshStatsContent()
        {
            _statsBuilder.Length = 0;
            _statsBuilder.Append(FpsLabel);
            _statsBuilder.Append(Mathf.RoundToInt(_displayedFps));

            if (showEntityCount)
            {
                _statsBuilder.Append('\n');
                _statsBuilder.Append(EntitiesLabel);
                _statsBuilder.Append(_entityCount);
            }

            _statsContent.text = _statsBuilder.ToString();
        }

        private int QueryEntityCount()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                _cachedWorld = null;
                _entityCountQuery = default;
                return 0;
            }

            if (_cachedWorld != world)
            {
                _cachedWorld = world;
                _entityCountQuery = world.EntityManager.UniversalQuery;
            }

            return _entityCountQuery.CalculateEntityCount();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(demoName))
            {
                demoName = DefaultDemoName;
            }

            techDescription ??= string.Empty;
            controlsHint ??= string.Empty;
        }
#endif
    }
}
