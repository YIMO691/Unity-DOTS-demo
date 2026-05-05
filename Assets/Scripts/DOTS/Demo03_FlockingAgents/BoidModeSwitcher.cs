using DOTSDemo.Shared;
using Unity.Entities;
using UnityEngine;

namespace UnityDotsDemo.Demo03
{
    [AddComponentMenu("DOTS Demo/Demo03 Boid Mode Switcher")]
    public sealed class BoidModeSwitcher : MonoBehaviour
    {
        [SerializeField] private BoidSimulationMode mode = BoidSimulationMode.Basic;
        [SerializeField, Min(0f)] private float cellSize = 4f;

        private Entity _modeEntity = Entity.Null;
        private BoidSimulationMode _lastMode;
        private float _lastCellSize;
        private GUIStyle _panelStyle;
        private GUIStyle _labelStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                mode = mode == BoidSimulationMode.Basic
                    ? BoidSimulationMode.SpatialHash
                    : BoidSimulationMode.Basic;
            }

            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                cellSize = Mathf.Max(0.25f, cellSize - 0.25f);
            }

            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                cellSize += 0.25f;
            }

            SyncModeEntity();
        }

        private void OnGUI()
        {
            EnsureStyles();
            Rect panel = new Rect(16f, 280f, 280f, 82f);
            GUI.Box(panel, GUIContent.none, _panelStyle);
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 10f, panel.width - 24f, panel.height - 20f),
                $"Boid Mode: {mode}\nCell Size: {cellSize:0.00}\nM: switch  [ / ]: cell size",
                _labelStyle);
        }

        private void SyncModeEntity()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                _modeEntity = Entity.Null;
                return;
            }

            EntityManager entityManager = world.EntityManager;
            if (_modeEntity == Entity.Null || !entityManager.Exists(_modeEntity))
            {
                using EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadWrite<BoidSimulationModeData>());
                if (!query.IsEmpty)
                {
                    _modeEntity = query.GetSingletonEntity();
                }
                else
                {
                    _modeEntity = entityManager.CreateEntity(typeof(BoidSimulationModeData));
                }

                _lastMode = (BoidSimulationMode)byte.MaxValue;
                _lastCellSize = -1f;
            }

            if (_lastMode == mode && Mathf.Approximately(_lastCellSize, cellSize))
            {
                return;
            }

            entityManager.SetComponentData(_modeEntity, new BoidSimulationModeData
            {
                Mode = mode,
                CellSize = cellSize
            });
            _lastMode = mode;
            _lastCellSize = cellSize;
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelStyle = GUIStyleHelper.LightPanel();
            _labelStyle = GUIStyleHelper.TitleLabel(13);
        }
    }
}
