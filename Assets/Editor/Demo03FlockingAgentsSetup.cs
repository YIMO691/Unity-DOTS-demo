using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DOTSDemo.Shared;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityDotsDemo.Demo03;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.EditorTools
{
    [InitializeOnLoad]
    public static class Demo03FlockingAgentsSetup
    {
        private const string DemoAssetRoot = "Assets/DOTS_DemoAssets/Demo03";
        private const string BoidMaterialPath = DemoAssetRoot + "/Demo03_Boid_Material.mat";
        private const string BoundsMaterialPath = DemoAssetRoot + "/Demo03_Bounds_Material.mat";
        private const string BoidPrefabPath = DemoAssetRoot + "/Demo03_Boid.prefab";
        private const string UrpAssetPath = "Assets/Settings/Demo_URP_Asset.asset";
        private const string UrpRendererPath = "Assets/Settings/Demo_URP_Asset_Renderer.asset";
        private const string MainScenePath = "Assets/Scenes/Demo03_FlockingAgents.unity";
        private const string SubScenePath = "Assets/Scenes/Demo03_FlockingAgents/Demo03_FlockingAgents_SubScene.unity";
        private const string MarkerPath = DemoAssetRoot + "/.demo03_setup_complete";

        static Demo03FlockingAgentsSetup()
        {
            EditorApplication.delayCall += AutoCreateOnce;
        }

        [MenuItem("DOTS Demos/Rebuild Demo 03 Flocking Agents")]
        public static void RebuildDemo03()
        {
            SetupDemo(force: true);
        }

        public static void RepairDemo03()
        {
            SetupDemo(force: false);
        }

        private static void AutoCreateOnce()
        {
            if (IsAutomatedRun() || IsImportWorker())
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

            if (File.Exists(MarkerPath) && DemoSceneIsCurrent())
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

                Material boidMaterial = CreateOrUpdateMaterial(
                    BoidMaterialPath,
                    "Demo03_Boid_Material",
                    new Color(0.18f, 0.92f, 0.74f, 1f));
                Material boundsMaterial = CreateOrUpdateMaterial(
                    BoundsMaterialPath,
                    "Demo03_Bounds_Material",
                    new Color(0.15f, 0.26f, 0.52f, 1f));

                GameObject boidPrefab = CreateOrUpdateBoidPrefab(boidMaterial);
                EnsureUrpAsset();
                CreateOrUpdateScenes(boidPrefab, boundsMaterial, force);

                File.WriteAllText(MarkerPath, DateTime.Now.ToString("O"));
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Demo 03 Flocking Agents is ready. Open {MainScenePath} and press Play.");
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

        private static bool IsAutomatedRun()
        {
            return Application.isBatchMode ||
                   Environment.CommandLine.IndexOf("-runTests", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool DemoSceneIsCurrent()
        {
            return File.Exists(MainScenePath) &&
                   File.Exists(SubScenePath) &&
                   SceneFileContains(MainScenePath, "Demo03_FlockingAgents_SubScene") &&
                   SceneFileContains(MainScenePath, "Demo03 Boid Mode Switcher") &&
                   SceneFileContains(MainScenePath, "Demo03 HUD");
        }

        private static bool SceneFileContains(string path, string value)
        {
            return File.Exists(path) && File.ReadAllText(path).Contains(value);
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets", "DOTS_DemoAssets");
            CreateFolderIfMissing("Assets/DOTS_DemoAssets", "Demo03");
            CreateFolderIfMissing("Assets", "Settings");
            CreateFolderIfMissing("Assets", "Scenes");
            CreateFolderIfMissing("Assets/Scenes", "Demo03_FlockingAgents");
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

        private static GameObject CreateOrUpdateBoidPrefab(Material boidMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoidPrefabPath);
            GameObject boid = prefab == null
                ? GameObject.CreatePrimitive(PrimitiveType.Capsule)
                : PrefabUtility.LoadPrefabContents(BoidPrefabPath);

            boid.name = "Demo03_Boid";
            boid.transform.localPosition = Vector3.zero;
            boid.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            boid.transform.localScale = new Vector3(0.35f, 0.6f, 0.35f);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(boid);

            foreach (Collider collider in boid.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshFilter meshFilter = boid.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = boid.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Capsule);

            MeshRenderer meshRenderer = boid.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = boid.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterial = boidMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;

            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(boid, BoidPrefabPath);
                UnityEngine.Object.DestroyImmediate(boid);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(boid, BoidPrefabPath);
                PrefabUtility.UnloadPrefabContents(boid);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoidPrefabPath);
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

        private static void CreateOrUpdateScenes(GameObject boidPrefab, Material boundsMaterial, bool force)
        {
            Scene mainScene = OpenOrCreateMainScene(force);
            Scene subScene = OpenOrCreateSubScene(force);

            CreateOrUpdateSpawner(subScene, boidPrefab);
            CreateOrUpdateBoundsVisualization(subScene, boundsMaterial);
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
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
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

        private static void CreateOrUpdateSpawner(Scene subScene, GameObject boidPrefab)
        {
            GameObject spawner = FindRootObject(subScene, "DOTS Boid Spawner");
            if (spawner == null)
            {
                spawner = new GameObject("DOTS Boid Spawner");
                SceneManager.MoveGameObjectToScene(spawner, subScene);
            }

            BoidSpawnerAuthoring authoring = EnsureSingleComponent<BoidSpawnerAuthoring>(spawner);

            authoring.BoidPrefab = boidPrefab;
            authoring.Count = 500;
            authoring.Center = new Vector3(0f, 6f, 0f);
            authoring.BoundsExtents = new Vector3(24f, 8f, 24f);
            authoring.MinSpeed = 3f;
            authoring.MaxSpeed = 7f;
            authoring.NeighborRadius = 4f;
            authoring.SeparationRadius = 1.2f;
            authoring.SeparationWeight = 1.7f;
            authoring.AlignmentWeight = 0.7f;
            authoring.CohesionWeight = 0.6f;
            authoring.BoundsWeight = 4f;
            authoring.RandomSeed = 4242;

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(authoring);
        }

        private static void CreateOrUpdateBoundsVisualization(Scene subScene, Material boundsMaterial)
        {
            const string rootName = "Boid Bounds";
            GameObject root = FindRootObject(subScene, rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
                SceneManager.MoveGameObjectToScene(root, subScene);
            }

            root.transform.SetPositionAndRotation(new Vector3(0f, 6f, 0f), Quaternion.identity);
            root.transform.localScale = Vector3.one;

            CreateOrUpdateBoundsBar(root.transform, "Floor", new Vector3(0f, -8f, 0f), new Vector3(48f, 0.2f, 48f), boundsMaterial);
            CreateOrUpdateBoundsBar(root.transform, "Ceiling", new Vector3(0f, 8f, 0f), new Vector3(48f, 0.2f, 48f), boundsMaterial);
            CreateOrUpdateBoundsBar(root.transform, "Back", new Vector3(0f, 0f, 24f), new Vector3(48f, 16f, 0.2f), boundsMaterial);
            CreateOrUpdateBoundsBar(root.transform, "Front", new Vector3(0f, 0f, -24f), new Vector3(48f, 16f, 0.2f), boundsMaterial);
            CreateOrUpdateBoundsBar(root.transform, "Left", new Vector3(-24f, 0f, 0f), new Vector3(0.2f, 16f, 48f), boundsMaterial);
            CreateOrUpdateBoundsBar(root.transform, "Right", new Vector3(24f, 0f, 0f), new Vector3(0.2f, 16f, 48f), boundsMaterial);

            EditorUtility.SetDirty(root);
        }

        private static void CreateOrUpdateBoundsBar(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            Transform existing = parent.Find(objectName);
            GameObject bar = existing == null
                ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                : existing.gameObject;

            if (existing == null)
            {
                bar.name = objectName;
                bar.transform.SetParent(parent, false);
            }

            foreach (Collider collider in bar.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshFilter meshFilter = bar.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Cube);

            MeshRenderer meshRenderer = bar.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            bar.transform.localPosition = localPosition;
            bar.transform.localRotation = Quaternion.identity;
            bar.transform.localScale = localScale;

            EditorUtility.SetDirty(bar);
            EditorUtility.SetDirty(meshFilter);
            EditorUtility.SetDirty(meshRenderer);
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
            camera.transform.position = new Vector3(0f, 18f, -42f);
            camera.transform.LookAt(new Vector3(0f, 6f, 0f));
            camera.fieldOfView = 46f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 300f;
            camera.clearFlags = CameraClearFlags.Skybox;

            Light light = FindObjectInScene<Light>(mainScene);
            if (light == null)
            {
                GameObject lightObject = new GameObject("Directional Light", typeof(Light));
                SceneManager.MoveGameObjectToScene(lightObject, mainScene);
                light = lightObject.GetComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(44f, -30f, 0f);

            GameObject switcherObject = FindRootObject(mainScene, "Demo03 Boid Mode Switcher");
            if (switcherObject == null)
            {
                switcherObject = new GameObject("Demo03 Boid Mode Switcher");
                SceneManager.MoveGameObjectToScene(switcherObject, mainScene);
            }

            EnsureSingleComponent<BoidModeSwitcher>(switcherObject);

            EditorUtility.SetDirty(switcherObject);

            GameObject hudObject = FindRootObject(mainScene, "Demo03 HUD");
            if (hudObject == null)
            {
                hudObject = new GameObject("Demo03 HUD");
                SceneManager.MoveGameObjectToScene(hudObject, mainScene);
            }

            DemoHUD hud = EnsureSingleComponent<DemoHUD>(hudObject);

            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("demoName").stringValue = "Flocking Agents";
            serializedHud.FindProperty("techDescription").stringValue =
                "Boids simulation\nSeparation + Alignment + Cohesion\nBasic vs SpatialHash modes";
            serializedHud.FindProperty("controlsHint").stringValue =
                "M: switch mode; [ / ]: change spatial hash cell size";
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            EnsureSingleComponent<DemoBackButton>(hudObject);

            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(hudObject);
        }

        private static T EnsureSingleComponent<T>(GameObject gameObject) where T : Component
        {
            T[] components = gameObject.GetComponents<T>();
            if (components.Length == 0)
            {
                return gameObject.AddComponent<T>();
            }

            T keep = components[0];
            for (int i = 1; i < components.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(components[i]);
            }

            return keep;
        }

        private static void CreateOrUpdateSubSceneReference(Scene mainScene)
        {
            SceneAsset subSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SubScenePath);
            GameObject subSceneObject = FindRootObject(mainScene, "Demo03_FlockingAgents_SubScene");

            if (subSceneObject == null)
            {
                subSceneObject = new GameObject("Demo03_FlockingAgents_SubScene");
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
