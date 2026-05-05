using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UnityDotsDemo.Demo04
{
    [AddComponentMenu("DOTS Demo/Demo04 Enemy Health Bars")]
    public sealed class EnemyHealthBars : MonoBehaviour
    {
        private const float Width = 44f;
        private const float Height = 6f;
        private GUIStyle _backgroundStyle;
        private GUIStyle _fillStyle;

        private void OnGUI()
        {
            Camera camera = Camera.main;
            World world = World.DefaultGameObjectInjectionWorld;
            if (camera == null || world == null || !world.IsCreated)
            {
                return;
            }

            EnsureStyles();

            EntityManager entityManager = world.EntityManager;
            using EntityQuery query = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<EnemyTag>(),
                ComponentType.ReadOnly<Health>(),
                ComponentType.ReadOnly<EnemyMaxHealth>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.Exclude<PooledEnemy>());

            NativeArray<Health> healths = query.ToComponentDataArray<Health>(Allocator.Temp);
            NativeArray<EnemyMaxHealth> maxHealths = query.ToComponentDataArray<EnemyMaxHealth>(Allocator.Temp);
            NativeArray<LocalTransform> transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            for (int i = 0; i < healths.Length; i++)
            {
                Vector3 worldPosition = transforms[i].Position + new Unity.Mathematics.float3(0f, 1.4f, 0f);
                Vector3 screen = camera.WorldToScreenPoint(worldPosition);
                if (screen.z <= 0f)
                {
                    continue;
                }

                float ratio = Mathf.Clamp01(healths[i].Value / Mathf.Max(1f, maxHealths[i].Value));
                Rect background = new Rect(screen.x - Width * 0.5f, Screen.height - screen.y, Width, Height);
                Rect fill = new Rect(background.x, background.y, Width * ratio, Height);
                GUI.Box(background, GUIContent.none, _backgroundStyle);
                GUI.Box(fill, GUIContent.none, _fillStyle);
            }

            healths.Dispose();
            maxHealths.Dispose();
            transforms.Dispose();
        }

        private void EnsureStyles()
        {
            if (_backgroundStyle != null)
            {
                return;
            }

            Texture2D background = Texture2D.whiteTexture;
            _backgroundStyle = new GUIStyle(GUI.skin.box);
            _backgroundStyle.normal.background = background;
            _fillStyle = new GUIStyle(GUI.skin.box);
            _fillStyle.normal.background = background;
        }
    }
}
