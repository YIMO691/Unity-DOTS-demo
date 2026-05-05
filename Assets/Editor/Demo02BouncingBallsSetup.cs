using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DOTSDemo.Shared;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityDotsDemo.Demo02;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.EditorTools
{
    [InitializeOnLoad]
    public static class Demo02BouncingBallsSetup
    {
        private const string DemoAssetRoot = "Assets/DOTS_DemoAssets/Demo02";
        private const string BallMaterialPath = DemoAssetRoot + "/Demo02_Ball_Material.mat";
        private const string ArenaMaterialPath = DemoAssetRoot + "/Demo02_Arena_Material.mat";
        private const string BallPrefabPath = DemoAssetRoot + "/Demo02_Ball.prefab";
        private const string UrpAssetPath = "Assets/Settings/Demo_URP_Asset.asset";
        private const string UrpRendererPath = "Assets/Settings/Demo_URP_Asset_Renderer.asset";
        private const string MainScenePath = "Assets/Scenes/Demo02_BouncingBalls.unity";
        private const string SubScenePath = "Assets/Scenes/Demo02_BouncingBalls/Demo02_BouncingBalls_SubScene.unity";
        private const string MarkerPath = DemoAssetRoot + "/.demo02_setup_complete";
        private const float WallCenterY = 4.75f;
        private const float WallHeight = 9.5f;

        static Demo02BouncingBallsSetup()
        {
            EditorApplication.delayCall += AutoCreateOnce;
        }

        [MenuItem("DOTS Demos/Rebuild Demo 02 Bouncing Balls")]
        public static void RebuildDemo02()
        {
            SetupDemo(force: true);
        }

        public static void RepairDemo02()
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

                Material ballMaterial = CreateOrUpdateMaterial(
                    BallMaterialPath,
                    "Demo02_Ball_Material",
                    new Color(1f, 0.42f, 0.12f, 1f));
                Material arenaMaterial = CreateOrUpdateMaterial(
                    ArenaMaterialPath,
                    "Demo02_Arena_Material",
                    new Color(0.16f, 0.19f, 0.22f, 1f));

                GameObject ballPrefab = CreateOrUpdateBallPrefab(ballMaterial);
                EnsureUrpAsset();
                CreateOrUpdateScenes(ballPrefab, arenaMaterial, force);

                File.WriteAllText(MarkerPath, DateTime.Now.ToString("O"));
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Demo 02 Bouncing Balls is ready. Open {MainScenePath} and press Play.");
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
                   SceneFileContains(MainScenePath, "Demo02_BouncingBalls_SubScene") &&
                   SceneFileContains(MainScenePath, "Demo02 HUD") &&
                   SceneFileContains(SubScenePath, "Back Wall") &&
                   SceneFileContains(SubScenePath, "SpawnCenter: {x: 0, y: 5, z: 0}") &&
                   SceneFileContains(SubScenePath, "SpawnSize: {x: 18, y: 7, z: 18}") &&
                   SceneFileContains(SubScenePath, "Size: {x: 22, y: 9.5, z: 1}");
        }

        private static bool SceneFileContains(string path, string value)
        {
            return File.Exists(path) && File.ReadAllText(path).Contains(value);
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets", "DOTS_DemoAssets");
            CreateFolderIfMissing("Assets/DOTS_DemoAssets", "Demo02");
            CreateFolderIfMissing("Assets", "Settings");
            CreateFolderIfMissing("Assets", "Scenes");
            CreateFolderIfMissing("Assets/Scenes", "Demo02_BouncingBalls");
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

        private static GameObject CreateOrUpdateBallPrefab(Material ballMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BallPrefabPath);
            GameObject ball = prefab == null
                ? GameObject.CreatePrimitive(PrimitiveType.Sphere)
                : PrefabUtility.LoadPrefabContents(BallPrefabPath);

            ball.name = "Demo02_Ball";
            ball.transform.localPosition = Vector3.zero;
            ball.transform.localRotation = Quaternion.identity;
            ball.transform.localScale = Vector3.one;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(ball);

            foreach (Collider collider in ball.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshFilter meshFilter = ball.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = ball.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Sphere);

            MeshRenderer meshRenderer = ball.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = ball.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterial = ballMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;

            BallPhysicsAuthoring physicsAuthoring = ball.GetComponent<BallPhysicsAuthoring>();
            if (physicsAuthoring == null)
            {
                physicsAuthoring = ball.AddComponent<BallPhysicsAuthoring>();
            }
            physicsAuthoring.Radius = 0.5f;
            physicsAuthoring.Mass = 1f;
            physicsAuthoring.Restitution = 0.9f;
            physicsAuthoring.Friction = 0.04f;
            physicsAuthoring.LinearDamping = 0.02f;
            physicsAuthoring.AngularDamping = 0.05f;

            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(ball, BallPrefabPath);
                UnityEngine.Object.DestroyImmediate(ball);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(ball, BallPrefabPath);
                PrefabUtility.UnloadPrefabContents(ball);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BallPrefabPath);
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

        private static void CreateOrUpdateScenes(GameObject ballPrefab, Material arenaMaterial, bool force)
        {
            Scene mainScene = OpenOrCreateMainScene(force);
            Scene subScene = OpenOrCreateSubScene(force);

            CreateOrUpdateSpawner(subScene, ballPrefab);
            CreateOrUpdateArena(subScene, arenaMaterial);
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

        private static void CreateOrUpdateSpawner(Scene subScene, GameObject ballPrefab)
        {
            GameObject spawner = FindRootObject(subScene, "DOTS Ball Spawner");
            if (spawner == null)
            {
                spawner = new GameObject("DOTS Ball Spawner");
                SceneManager.MoveGameObjectToScene(spawner, subScene);
            }

            BallSpawnerAuthoring authoring = EnsureSingleComponent<BallSpawnerAuthoring>(spawner);

            authoring.BallPrefab = ballPrefab;
            authoring.Count = 200;
            authoring.SpawnCenter = new Vector3(0f, 5f, 0f);
            authoring.SpawnSize = new Vector3(18f, 7f, 18f);
            authoring.ResetY = -6f;
            authoring.RandomSeed = 777;

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(authoring);
        }

        private static void CreateOrUpdateArena(Scene subScene, Material arenaMaterial)
        {
            CreateOrUpdateStaticBox(
                subScene,
                "Floor",
                new Vector3(0f, -0.5f, 0f),
                new Vector3(22f, 1f, 22f),
                arenaMaterial);
            CreateOrUpdateStaticBox(
                subScene,
                "Back Wall",
                new Vector3(0f, WallCenterY, 11f),
                new Vector3(22f, WallHeight, 1f),
                arenaMaterial);
            CreateOrUpdateStaticBox(
                subScene,
                "Front Wall",
                new Vector3(0f, WallCenterY, -11f),
                new Vector3(22f, WallHeight, 1f),
                arenaMaterial);
            CreateOrUpdateStaticBox(
                subScene,
                "Left Wall",
                new Vector3(-11f, WallCenterY, 0f),
                new Vector3(1f, WallHeight, 22f),
                arenaMaterial);
            CreateOrUpdateStaticBox(
                subScene,
                "Right Wall",
                new Vector3(11f, WallCenterY, 0f),
                new Vector3(1f, WallHeight, 22f),
                arenaMaterial);
        }

        private static void CreateOrUpdateStaticBox(
            Scene subScene,
            string objectName,
            Vector3 position,
            Vector3 size,
            Material arenaMaterial)
        {
            GameObject root = FindRootObject(subScene, objectName);
            if (root == null)
            {
                root = new GameObject(objectName);
                SceneManager.MoveGameObjectToScene(root, subScene);
            }

            root.transform.SetPositionAndRotation(position, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

            foreach (Collider collider in root.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshRenderer rootRenderer = root.GetComponent<MeshRenderer>();
            if (rootRenderer != null)
            {
                UnityEngine.Object.DestroyImmediate(rootRenderer);
            }

            MeshFilter rootFilter = root.GetComponent<MeshFilter>();
            if (rootFilter != null)
            {
                UnityEngine.Object.DestroyImmediate(rootFilter);
            }

            foreach (StaticBoxColliderAuthoring existingAuthoring in root.GetComponents<StaticBoxColliderAuthoring>())
            {
                UnityEngine.Object.DestroyImmediate(existingAuthoring);
            }

            StaticBoxColliderAuthoring colliderAuthoring = root.AddComponent<StaticBoxColliderAuthoring>();
            colliderAuthoring.Size = size;
            colliderAuthoring.Restitution = 0.85f;
            colliderAuthoring.Friction = 0.08f;

            Transform visualTransform = root.transform.Find("Visual");
            GameObject visual = visualTransform == null
                ? new GameObject("Visual", typeof(MeshFilter), typeof(MeshRenderer))
                : visualTransform.gameObject;

            if (visualTransform == null)
            {
                visual.transform.SetParent(root.transform, false);
            }

            MeshFilter meshFilter = visual.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = visual.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Cube);

            MeshRenderer meshRenderer = visual.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = visual.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterial = arenaMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;

            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = size;

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(colliderAuthoring);
            EditorUtility.SetDirty(visual);
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
            camera.transform.position = new Vector3(0f, 18f, -28f);
            camera.transform.LookAt(new Vector3(0f, 4f, 0f));
            camera.fieldOfView = 50f;
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
            light.intensity = 1.35f;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            GameObject hudObject = FindRootObject(mainScene, "Demo02 HUD");
            if (hudObject == null)
            {
                hudObject = new GameObject("Demo02 HUD");
                SceneManager.MoveGameObjectToScene(hudObject, mainScene);
            }

            DemoHUD hud = EnsureSingleComponent<DemoHUD>(hudObject);

            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("demoName").stringValue = "Bouncing Balls";
            serializedHud.FindProperty("techDescription").stringValue =
                "Unity Physics integration\nPhysicsCollider + PhysicsVelocity\nStatic collider arena";
            serializedHud.FindProperty("controlsHint").stringValue =
                "Reset volume below Y=-6; balls bounce inside the arena";
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
            GameObject subSceneObject = FindRootObject(mainScene, "Demo02_BouncingBalls_SubScene");

            if (subSceneObject == null)
            {
                subSceneObject = new GameObject("Demo02_BouncingBalls_SubScene");
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
