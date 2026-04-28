using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityDotsDemo.Demo04;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.EditorTools
{
    [InitializeOnLoad]
    public static class Demo04TowerDefenseSetup
    {
        private const string DemoAssetRoot = "Assets/DOTS_DemoAssets/Demo04";
        private const string EnemyMaterialPath = DemoAssetRoot + "/Demo04_Enemy_Material.mat";
        private const string TowerMaterialPath = DemoAssetRoot + "/Demo04_Tower_Material.mat";
        private const string ProjectileMaterialPath = DemoAssetRoot + "/Demo04_Projectile_Material.mat";
        private const string GroundMaterialPath = DemoAssetRoot + "/Demo04_Ground_Material.mat";
        private const string PathMaterialPath = DemoAssetRoot + "/Demo04_Path_Material.mat";
        private const string EnemyPrefabPath = DemoAssetRoot + "/Demo04_Enemy.prefab";
        private const string TowerPrefabPath = DemoAssetRoot + "/Demo04_Tower.prefab";
        private const string ProjectilePrefabPath = DemoAssetRoot + "/Demo04_Projectile.prefab";
        private const string UrpAssetPath = "Assets/Settings/Demo_URP_Asset.asset";
        private const string UrpRendererPath = "Assets/Settings/Demo_URP_Asset_Renderer.asset";
        private const string MainScenePath = "Assets/Scenes/Demo04_TowerDefense.unity";
        private const string SubScenePath = "Assets/Scenes/Demo04_TowerDefense/Demo04_TowerDefense_SubScene.unity";
        private const string MarkerPath = DemoAssetRoot + "/.demo04_setup_complete";

        static Demo04TowerDefenseSetup()
        {
            EditorApplication.delayCall += AutoCreateOnce;
        }

        [MenuItem("DOTS Demos/Rebuild Demo 04 Tower Defense")]
        public static void RebuildDemo04()
        {
            SetupDemo(force: true);
        }

        private static void AutoCreateOnce()
        {
            if (IsImportWorker())
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.playModeStateChanged -= RequeueAfterPlayMode;
                EditorApplication.playModeStateChanged += RequeueAfterPlayMode;
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoCreateOnce;
                return;
            }

            if (File.Exists(MarkerPath))
            {
                EnsureUrpAsset();
                AssetDatabase.SaveAssets();
                return;
            }

            SetupDemo(force: false);
        }

        private static void RequeueAfterPlayMode(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= RequeueAfterPlayMode;
            EditorApplication.delayCall += AutoCreateOnce;
        }

        private static void SetupDemo(bool force)
        {
            try
            {
                EnsureFolders();

                Material enemyMaterial = CreateOrUpdateMaterial(
                    EnemyMaterialPath,
                    "Demo04_Enemy_Material",
                    new Color(1f, 0.35f, 0.26f, 1f));
                Material towerMaterial = CreateOrUpdateMaterial(
                    TowerMaterialPath,
                    "Demo04_Tower_Material",
                    new Color(0.18f, 0.72f, 1f, 1f));
                Material projectileMaterial = CreateOrUpdateMaterial(
                    ProjectileMaterialPath,
                    "Demo04_Projectile_Material",
                    new Color(1f, 0.84f, 0.18f, 1f));
                Material groundMaterial = CreateOrUpdateMaterial(
                    GroundMaterialPath,
                    "Demo04_Ground_Material",
                    new Color(0.18f, 0.22f, 0.18f, 1f));
                Material pathMaterial = CreateOrUpdateMaterial(
                    PathMaterialPath,
                    "Demo04_Path_Material",
                    new Color(0.42f, 0.42f, 0.46f, 1f));

                GameObject enemyPrefab = CreateOrUpdateEnemyPrefab(enemyMaterial);
                GameObject towerPrefab = CreateOrUpdateTowerPrefab(towerMaterial);
                GameObject projectilePrefab = CreateOrUpdateProjectilePrefab(projectileMaterial);
                EnsureUrpAsset();
                CreateOrUpdateScenes(
                    enemyPrefab,
                    towerPrefab,
                    projectilePrefab,
                    groundMaterial,
                    pathMaterial,
                    towerMaterial,
                    force);

                File.WriteAllText(MarkerPath, DateTime.Now.ToString("O"));
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Demo 04 Tower Defense is ready. Open {MainScenePath} and press Play.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static bool IsImportWorker()
        {
            string commandLine = Environment.CommandLine;
            return commandLine.IndexOf("AssetImportWorker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   commandLine.IndexOf("-assetImportWorker", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets", "DOTS_DemoAssets");
            CreateFolderIfMissing("Assets/DOTS_DemoAssets", "Demo04");
            CreateFolderIfMissing("Assets", "Settings");
            CreateFolderIfMissing("Assets", "Scenes");
            CreateFolderIfMissing("Assets/Scenes", "Demo04_TowerDefense");
        }

        private static void CreateFolderIfMissing(string parent, string folderName)
        {
            string path = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static Material CreateOrUpdateMaterial(string path, string materialName, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader)
                {
                    name = materialName,
                    color = color
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
                material.color = color;
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static GameObject CreateOrUpdateEnemyPrefab(Material enemyMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            GameObject enemy = prefab == null
                ? GameObject.CreatePrimitive(PrimitiveType.Capsule)
                : PrefabUtility.LoadPrefabContents(EnemyPrefabPath);

            enemy.name = "Demo04_Enemy";
            enemy.transform.localPosition = Vector3.zero;
            enemy.transform.localRotation = Quaternion.identity;
            enemy.transform.localScale = new Vector3(0.85f, 1f, 0.85f);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(enemy);

            foreach (Collider collider in enemy.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshFilter meshFilter = enemy.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Capsule);

            MeshRenderer meshRenderer = enemy.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = enemyMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;

            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
                UnityEngine.Object.DestroyImmediate(enemy);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
                PrefabUtility.UnloadPrefabContents(enemy);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            }

            return prefab;
        }

        private static GameObject CreateOrUpdateTowerPrefab(Material towerMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TowerPrefabPath);
            GameObject tower = prefab == null
                ? GameObject.CreatePrimitive(PrimitiveType.Cylinder)
                : PrefabUtility.LoadPrefabContents(TowerPrefabPath);

            tower.name = "Demo04_Tower";
            tower.transform.localPosition = Vector3.zero;
            tower.transform.localRotation = Quaternion.identity;
            tower.transform.localScale = new Vector3(0.9f, 0.8f, 0.9f);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(tower);

            foreach (Collider collider in tower.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshFilter meshFilter = tower.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Cylinder);

            MeshRenderer meshRenderer = tower.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = towerMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;

            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(tower, TowerPrefabPath);
                UnityEngine.Object.DestroyImmediate(tower);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(tower, TowerPrefabPath);
                PrefabUtility.UnloadPrefabContents(tower);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TowerPrefabPath);
            }

            return prefab;
        }

        private static GameObject CreateOrUpdateProjectilePrefab(Material projectileMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            GameObject projectile = prefab == null
                ? GameObject.CreatePrimitive(PrimitiveType.Sphere)
                : PrefabUtility.LoadPrefabContents(ProjectilePrefabPath);

            projectile.name = "Demo04_Projectile";
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.localRotation = Quaternion.identity;
            projectile.transform.localScale = Vector3.one * 0.35f;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(projectile);

            foreach (Collider collider in projectile.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshFilter meshFilter = projectile.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Sphere);

            MeshRenderer meshRenderer = projectile.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = projectileMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(projectile, ProjectilePrefabPath);
                UnityEngine.Object.DestroyImmediate(projectile);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(projectile, ProjectilePrefabPath);
                PrefabUtility.UnloadPrefabContents(projectile);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            }

            return prefab;
        }

        private static void EnsureUrpAsset()
        {
            UniversalRenderPipelineAsset pipeline =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);

            if (pipeline == null)
            {
                ScriptableRendererData rendererData = CreateUniversalRendererData();
                SetForwardPlusRendering(rendererData);

                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, UrpAssetPath);
            }

            UniversalRendererData existingRenderer =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpRendererPath);
            if (existingRenderer != null)
            {
                SetForwardPlusRendering(existingRenderer);
            }

            GraphicsSettings.renderPipelineAsset = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static ScriptableRendererData CreateUniversalRendererData()
        {
            MethodInfo createRendererAsset = typeof(UniversalRenderPipelineAsset).GetMethod(
                "CreateRendererAsset",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (createRendererAsset == null)
            {
                throw new MissingMethodException(
                    nameof(UniversalRenderPipelineAsset),
                    "CreateRendererAsset");
            }

            return (ScriptableRendererData)createRendererAsset.Invoke(
                null,
                new object[] { UrpAssetPath, RendererType.UniversalRenderer, true, "Renderer" });
        }

        private static void SetForwardPlusRendering(UnityEngine.Object rendererData)
        {
            SerializedObject serializedRenderer = new SerializedObject(rendererData);
            SerializedProperty renderingMode = serializedRenderer.FindProperty("m_RenderingMode");
            if (renderingMode != null)
            {
                renderingMode.intValue = (int)RenderingMode.ForwardPlus;
                serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rendererData);
            }
        }

        private static void CreateOrUpdateScenes(
            GameObject enemyPrefab,
            GameObject towerPrefab,
            GameObject projectilePrefab,
            Material groundMaterial,
            Material pathMaterial,
            Material towerMaterial,
            bool force)
        {
            Scene mainScene = OpenOrCreateMainScene(force);
            Scene subScene = OpenOrCreateSubScene(force);

            List<Transform> waypoints = CreateOrUpdatePath(subScene, pathMaterial);
            List<Transform> towerPoints = CreateOrUpdateTowerPads(subScene, towerMaterial);
            CreateOrUpdateSpawner(subScene, enemyPrefab, towerPrefab, projectilePrefab, waypoints, towerPoints);
            CreateOrUpdateGround(subScene, groundMaterial);
            EditorSceneManager.SaveScene(subScene, SubScenePath);

            CreateOrUpdateMainSceneObjects(mainScene);
            CreateOrUpdateSubSceneReference(mainScene);
            AddSceneToBuildSettings(MainScenePath);

            EditorSceneManager.MarkSceneDirty(mainScene);
            EditorSceneManager.SaveScene(mainScene, MainScenePath);
            SceneManager.SetActiveScene(mainScene);
        }

        private static Scene OpenOrCreateMainScene(bool force)
        {
            if (force && File.Exists(MainScenePath))
            {
                AssetDatabase.DeleteAsset(MainScenePath);
            }

            Scene scene;
            if (File.Exists(MainScenePath))
            {
                scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Additive);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, MainScenePath);
            }

            return scene;
        }

        private static Scene OpenOrCreateSubScene(bool force)
        {
            if (force && File.Exists(SubScenePath))
            {
                AssetDatabase.DeleteAsset(SubScenePath);
            }

            Scene scene;
            if (File.Exists(SubScenePath))
            {
                scene = EditorSceneManager.OpenScene(SubScenePath, OpenSceneMode.Additive);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, SubScenePath);
            }

            SubSceneInspectorUtility.SetSceneAsSubScene(scene);
            return scene;
        }

        private static void CreateOrUpdateGround(Scene subScene, Material groundMaterial)
        {
            GameObject root = FindRootObject(subScene, "Battlefield");
            if (root == null)
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Cube);
                root.name = "Battlefield";
                SceneManager.MoveGameObjectToScene(root, subScene);
            }

            foreach (Collider collider in root.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            root.transform.SetPositionAndRotation(new Vector3(0f, -0.55f, 0f), Quaternion.identity);
            root.transform.localScale = new Vector3(34f, 1f, 20f);

            MeshFilter meshFilter = root.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Cube);

            MeshRenderer meshRenderer = root.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = groundMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(meshFilter);
            EditorUtility.SetDirty(meshRenderer);
        }

        private static List<Transform> CreateOrUpdatePath(Scene subScene, Material pathMaterial)
        {
            Vector3[] positions =
            {
                new Vector3(-12f, 0f, -5f),
                new Vector3(-4f, 0f, 5f),
                new Vector3(5f, 0f, 4f),
                new Vector3(12f, 0f, -3f)
            };

            GameObject root = FindRootObject(subScene, "Path");
            if (root == null)
            {
                root = new GameObject("Path");
                SceneManager.MoveGameObjectToScene(root, subScene);
            }

            List<Transform> waypoints = new List<Transform>();
            for (int i = 0; i < positions.Length; i++)
            {
                string waypointName = $"Waypoint_{i + 1}";
                Transform existing = root.transform.Find(waypointName);
                GameObject waypoint = existing == null ? new GameObject(waypointName) : existing.gameObject;
                if (existing == null)
                {
                    waypoint.transform.SetParent(root.transform, false);
                }

                waypoint.transform.localPosition = positions[i];
                waypoint.transform.localRotation = Quaternion.identity;
                waypoint.transform.localScale = Vector3.one;
                waypoints.Add(waypoint.transform);

                string markerName = "Marker";
                Transform markerTransform = waypoint.transform.Find(markerName);
                GameObject marker = markerTransform == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Sphere)
                    : markerTransform.gameObject;

                if (markerTransform == null)
                {
                    marker.name = markerName;
                    marker.transform.SetParent(waypoint.transform, false);
                }

                foreach (Collider collider in marker.GetComponents<Collider>())
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                marker.transform.localPosition = Vector3.up * 0.2f;
                marker.transform.localRotation = Quaternion.identity;
                marker.transform.localScale = Vector3.one * 0.45f;
                marker.GetComponent<MeshFilter>().sharedMesh = GetPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer markerRenderer = marker.GetComponent<MeshRenderer>();
                markerRenderer.sharedMaterial = pathMaterial;
                markerRenderer.shadowCastingMode = ShadowCastingMode.Off;
                markerRenderer.receiveShadows = false;

                EditorUtility.SetDirty(waypoint);
                EditorUtility.SetDirty(marker);
                EditorUtility.SetDirty(markerRenderer);
            }

            for (int i = 0; i < positions.Length - 1; i++)
            {
                string segmentName = $"Segment_{i + 1}";
                Transform existing = root.transform.Find(segmentName);
                GameObject segment = existing == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                    : existing.gameObject;

                if (existing == null)
                {
                    segment.name = segmentName;
                    segment.transform.SetParent(root.transform, false);
                }

                foreach (Collider collider in segment.GetComponents<Collider>())
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                Vector3 start = positions[i];
                Vector3 end = positions[i + 1];
                Vector3 midpoint = (start + end) * 0.5f;
                Vector3 direction = end - start;
                float length = direction.magnitude;

                segment.transform.localPosition = midpoint + Vector3.up * 0.05f;
                segment.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                segment.transform.localScale = new Vector3(0.6f, 0.1f, length);
                segment.GetComponent<MeshFilter>().sharedMesh = GetPrimitiveMesh(PrimitiveType.Cube);
                MeshRenderer segmentRenderer = segment.GetComponent<MeshRenderer>();
                segmentRenderer.sharedMaterial = pathMaterial;
                segmentRenderer.shadowCastingMode = ShadowCastingMode.Off;
                segmentRenderer.receiveShadows = false;

                EditorUtility.SetDirty(segment);
                EditorUtility.SetDirty(segmentRenderer);
            }

            EditorUtility.SetDirty(root);
            return waypoints;
        }

        private static List<Transform> CreateOrUpdateTowerPads(Scene subScene, Material towerMaterial)
        {
            Vector3[] positions =
            {
                new Vector3(-6f, 0f, 0f),
                new Vector3(1f, 0f, 1.5f),
                new Vector3(7f, 0f, 0f)
            };

            GameObject root = FindRootObject(subScene, "Tower Pads");
            if (root == null)
            {
                root = new GameObject("Tower Pads");
                SceneManager.MoveGameObjectToScene(root, subScene);
            }

            List<Transform> towerPoints = new List<Transform>();
            for (int i = 0; i < positions.Length; i++)
            {
                string towerName = $"TowerPoint_{i + 1}";
                Transform existing = root.transform.Find(towerName);
                GameObject towerPoint = existing == null ? new GameObject(towerName) : existing.gameObject;
                if (existing == null)
                {
                    towerPoint.transform.SetParent(root.transform, false);
                }

                towerPoint.transform.localPosition = positions[i];
                towerPoint.transform.localRotation = Quaternion.identity;
                towerPoint.transform.localScale = Vector3.one;
                towerPoints.Add(towerPoint.transform);

                string padName = "Pad";
                Transform padTransform = towerPoint.transform.Find(padName);
                GameObject pad = padTransform == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Cylinder)
                    : padTransform.gameObject;

                if (padTransform == null)
                {
                    pad.name = padName;
                    pad.transform.SetParent(towerPoint.transform, false);
                }

                foreach (Collider collider in pad.GetComponents<Collider>())
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                pad.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                pad.transform.localRotation = Quaternion.identity;
                pad.transform.localScale = new Vector3(1.5f, 0.08f, 1.5f);
                pad.GetComponent<MeshFilter>().sharedMesh = GetPrimitiveMesh(PrimitiveType.Cylinder);
                MeshRenderer padRenderer = pad.GetComponent<MeshRenderer>();
                padRenderer.sharedMaterial = towerMaterial;
                padRenderer.shadowCastingMode = ShadowCastingMode.Off;
                padRenderer.receiveShadows = true;

                EditorUtility.SetDirty(towerPoint);
                EditorUtility.SetDirty(pad);
                EditorUtility.SetDirty(padRenderer);
            }

            EditorUtility.SetDirty(root);
            return towerPoints;
        }

        private static void CreateOrUpdateSpawner(
            Scene subScene,
            GameObject enemyPrefab,
            GameObject towerPrefab,
            GameObject projectilePrefab,
            List<Transform> waypoints,
            List<Transform> towerPoints)
        {
            GameObject spawner = FindRootObject(subScene, "DOTS Wave Spawner");
            if (spawner == null)
            {
                spawner = new GameObject("DOTS Wave Spawner");
                SceneManager.MoveGameObjectToScene(spawner, subScene);
            }

            WaveSpawnerAuthoring authoring = spawner.GetComponent<WaveSpawnerAuthoring>();
            if (authoring == null)
            {
                authoring = spawner.AddComponent<WaveSpawnerAuthoring>();
            }

            authoring.EnemyPrefab = enemyPrefab;
            authoring.TowerPrefab = towerPrefab;
            authoring.ProjectilePrefab = projectilePrefab;
            authoring.Waypoints = waypoints;
            authoring.TowerPositions = towerPoints;
            authoring.EnemiesPerWave = 12;
            authoring.MaxWaves = 5;
            authoring.SpawnInterval = 0.45f;
            authoring.TimeBetweenWaves = 2f;
            authoring.EnemyHealth = 30f;
            authoring.EnemySpeed = 3f;
            authoring.TowerRange = 7f;
            authoring.TowerFireRate = 1.25f;
            authoring.ProjectileSpeed = 12f;
            authoring.ProjectileDamage = 10f;
            authoring.ProjectileLifetime = 3f;
            authoring.ProjectileHitRadius = 0.35f;

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(authoring);
        }

        private static Mesh GetPrimitiveMesh(PrimitiveType primitiveType)
        {
            GameObject temporary = GameObject.CreatePrimitive(primitiveType);
            temporary.hideFlags = HideFlags.HideAndDontSave;
            Mesh mesh = temporary.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(temporary);
            return mesh;
        }

        private static void CreateOrUpdateMainSceneObjects(Scene mainScene)
        {
            Camera camera = FindObjectInScene<Camera>(mainScene);
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                SceneManager.MoveGameObjectToScene(cameraObject, mainScene);
                camera = cameraObject.GetComponent<Camera>();
            }

            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 20f, -26f);
            camera.transform.LookAt(new Vector3(0f, 0f, 0f));
            camera.fieldOfView = 44f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            camera.clearFlags = CameraClearFlags.Skybox;

            Light light = FindObjectInScene<Light>(mainScene);
            if (light == null)
            {
                GameObject lightObject = new GameObject("Directional Light", typeof(Light));
                SceneManager.MoveGameObjectToScene(lightObject, mainScene);
                light = lightObject.GetComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        }

        private static void CreateOrUpdateSubSceneReference(Scene mainScene)
        {
            SceneAsset subSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SubScenePath);
            GameObject subSceneObject = FindRootObject(mainScene, "Demo04_TowerDefense_SubScene");

            if (subSceneObject == null)
            {
                subSceneObject = new GameObject("Demo04_TowerDefense_SubScene");
                SceneManager.MoveGameObjectToScene(subSceneObject, mainScene);
            }

            SubScene subScene = subSceneObject.GetComponent<SubScene>();
            if (subScene == null)
            {
                subScene = subSceneObject.AddComponent<SubScene>();
            }

            subScene.AutoLoadScene = true;
            subScene.SceneAsset = subSceneAsset;
            EditorUtility.SetDirty(subSceneObject);
            EditorUtility.SetDirty(subScene);
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            return scene.GetRootGameObjects().FirstOrDefault(root => root.name == objectName);
        }

        private static T FindObjectInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Any(scene => scene.path == scenePath))
            {
                return;
            }

            EditorBuildSettings.scenes = scenes
                .Concat(new[] { new EditorBuildSettingsScene(scenePath, true) })
                .ToArray();
        }
    }
}
