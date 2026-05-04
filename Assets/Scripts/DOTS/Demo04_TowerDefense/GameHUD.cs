using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.Demo04
{
    [AddComponentMenu("DOTS Demo/Demo04 Game HUD")]
    public sealed class GameHUD : MonoBehaviour
    {
        private const float RefreshInterval = 0.25f;
        private float _timer;
        private float _fpsTimer;
        private int _fpsFrames;
        private int _fps;
        private GameState _gameState;
        private BaseHealth _baseHealth;
        private bool _hasGameState;
        private bool _hasBaseHealth;
        private GUIStyle _panelStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _bannerStyle;

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            _fpsTimer += Time.unscaledDeltaTime;
            _fpsFrames++;

            if (_fpsTimer >= 0.5f)
            {
                _fps = Mathf.RoundToInt(_fpsFrames / _fpsTimer);
                _fpsFrames = 0;
                _fpsTimer = 0f;
            }

            if (_timer >= RefreshInterval)
            {
                _timer = 0f;
                RefreshState();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            Rect panel = new Rect(16f, 136f, 260f, 132f);
            GUI.Box(panel, GUIContent.none, _panelStyle);

            string wave = _hasGameState
                ? $"Wave {_gameState.CurrentWave} / {_gameState.TotalWaves}"
                : "Wave - / -";
            string enemies = _hasGameState ? $"Enemies: {_gameState.EnemyAliveCount}" : "Enemies: -";
            string kills = _hasGameState ? $"Kills: {_gameState.KillCount}" : "Kills: -";
            string baseHp = _hasBaseHealth
                ? $"Base HP: {_baseHealth.CurrentHP} / {_baseHealth.MaxHP}"
                : "Base HP: - / -";

            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 10f, panel.width - 24f, panel.height - 20f),
                $"{wave}\n{enemies}\n{kills}\n{baseHp}\nFPS: {_fps}",
                _bodyStyle);

            if (!_hasGameState ||
                (_gameState.Phase != GamePhase.Victory && _gameState.Phase != GamePhase.Defeat))
            {
                return;
            }

            string message = _gameState.Phase == GamePhase.Victory
                ? "VICTORY - Press R to restart"
                : "DEFEAT - Press R to restart";
            Rect banner = new Rect(0f, Screen.height * 0.5f - 34f, Screen.width, 68f);
            GUI.Label(banner, message, _bannerStyle);
        }

        private void RefreshState()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                _hasGameState = false;
                _hasBaseHealth = false;
                return;
            }

            EntityManager entityManager = world.EntityManager;
            using EntityQuery gameQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<GameState>());
            using EntityQuery baseQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<BaseHealth>());

            NativeArray<GameState> gameStates = gameQuery.ToComponentDataArray<GameState>(Allocator.Temp);
            NativeArray<BaseHealth> baseHealths = baseQuery.ToComponentDataArray<BaseHealth>(Allocator.Temp);

            _hasGameState = gameStates.Length > 0;
            _hasBaseHealth = baseHealths.Length > 0;
            if (_hasGameState)
            {
                _gameState = gameStates[0];
            }

            if (_hasBaseHealth)
            {
                _baseHealth = baseHealths[0];
            }

            gameStates.Dispose();
            baseHealths.Dispose();
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 10, 10)
            };
            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            _bannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
