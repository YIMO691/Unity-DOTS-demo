using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityDotsDemo.Demo01;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.EditorTools
{
    [InitializeOnLoad]
    public static class Demo01MovingCubesSetup
    {
        private const string DemoAssetRoot = "Assets/DOTS_DemoAssets/Demo01";
        private const string CubeMaterialPath = DemoAssetRoot + "/Demo01_Cube_Material.mat";
        private const string CubePrefabPath = DemoAssetRoot + "/Demo01_Cube.prefab";
        private const string UrpAssetPath = "Assets/Settings/Demo_URP_Asset.asset";
        private const string UrpRendererPath = "Assets/Settings/Demo_URP_Asset_Renderer.asset";
        private const string MainScenePath = "Assets/Scenes/Demo01_MovingCubes.unity";
        private const string SubScenePath = "Assets/Scenes/Demo01_MovingCubes/Demo01_MovingCubes_SubScene.unity";
        private const string MarkerPath = DemoAssetRoot + "/.demo01_setup_complete";

        static Demo01MovingCubesSetup()
        {
            EditorApplication.delayCall += AutoCreateOnce;
        }

        [MenuItem("DOTS Demos/Rebuild Demo 01 Moving Cubes")]
        public static void RebuildDemo01()
        {
            SetupDemo(force: true);
        }

        private static void AutoCreateOnce()
        {
            if (IsImportWorker() || EditorApplication.isPlayingOrWillChangePlaymode)
            {
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

        private static void SetupDemo(bool force)
        {
            try
            {
                EnsureFolders();

                Material cubeMaterial = CreateOrUpdateCubeMaterial();
                GameObject cubePrefab = CreateOrUpdateCubePrefab(cubeMaterial);
                EnsureUrpAsset();
                CreateOrUpdateScenes(cubePrefab, force);

                File.WriteAllText(MarkerPath, DateTime.Now.ToString("O"));
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Demo 01 Moving Cubes is ready. Open {MainScenePath} and press Play.");
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
            CreateFolderIfMissing("Assets/DOTS_DemoAssets", "Demo01");
            CreateFolderIfMissing("Assets", "Settings");
            CreateFolderIfMissing("Assets/Scenes", "Demo01_MovingCubes");
        }

        private static void CreateFolderIfMissing(string parent, string folderName)
        {
            string path = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static Material CreateOrUpdateCubeMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CubeMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "Demo01_Cube_Material",
                    color = new Color(0.15f, 0.62f, 1f, 1f)
                };
                AssetDatabase.CreateAsset(material, CubeMaterialPath);
            }
            else
            {
                material.shader = shader;
                material.color = new Color(0.15f, 0.62f, 1f, 1f);
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static GameObject CreateOrUpdateCubePrefab(Material cubeMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CubePrefabPath);
            if (prefab != null)
            {
                return prefab;
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Demo01_Cube";
            cube.transform.localScale = Vector3.one * 0.85f;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = cubeMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;

            prefab = PrefabUtility.SaveAsPrefabAsset(cube, CubePrefabPath);
            UnityEngine.Object.DestroyImmediate(cube);
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

        private static void CreateOrUpdateScenes(GameObject cubePrefab, bool force)
        {
            Scene mainScene = OpenOrCreateMainScene(force);
            Scene subScene = OpenOrCreateSubScene(force);

            CreateOrUpdateSpawner(subScene, cubePrefab);
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

        private static void CreateOrUpdateSpawner(Scene subScene, GameObject cubePrefab)
        {
            GameObject spawner = FindRootObject(subScene, "DOTS Cube Spawner");
            if (spawner == null)
            {
                spawner = new GameObject("DOTS Cube Spawner");
                SceneManager.MoveGameObjectToScene(spawner, subScene);
            }

            CubeSpawnerAuthoring authoring = spawner.GetComponent<CubeSpawnerAuthoring>();
            if (authoring == null)
            {
                authoring = spawner.AddComponent<CubeSpawnerAuthoring>();
            }

            authoring.CubePrefab = cubePrefab;
            authoring.SpawnCount = 1000;
            authoring.CountX = 25;
            authoring.CountZ = 40;
            authoring.Spacing = 1.4f;
            authoring.MinSpeed = 1.5f;
            authoring.MaxSpeed = 4.5f;
            authoring.AreaHalfSize = new Vector2(22f, 18f);
            authoring.RandomSeed = 12345;

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(authoring);
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
            camera.transform.position = new Vector3(0f, 24f, -31f);
            camera.transform.LookAt(Vector3.zero);
            camera.fieldOfView = 48f;
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
            light.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject hudObject = FindRootObject(mainScene, "Demo01 HUD");
            if (hudObject == null)
            {
                hudObject = new GameObject("Demo01 HUD");
                SceneManager.MoveGameObjectToScene(hudObject, mainScene);
            }

            Component hud = hudObject.GetComponent("DemoHUD");
            if (hud == null)
            {
                Type hudType = Type.GetType("DOTSDemo.Shared.DemoHUD, UnityDotsDemo");
                if (hudType != null)
                {
                    hud = hudObject.AddComponent(hudType);
                }
            }

            if (hud != null)
            {
                SerializedObject serializedHud = new SerializedObject(hud);
                serializedHud.FindProperty("demoName").stringValue = "Moving Cubes";
                serializedHud.FindProperty("techDescription").stringValue =
                    "IJobEntity + Burst + ECB\nParallel entity movement\nBoundary wrap";
                serializedHud.FindProperty("controlsHint").stringValue =
                    "SpawnCount test values: 1000 / 5000 / 10000 / 50000";
                serializedHud.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(hud);
            }

            EditorUtility.SetDirty(hudObject);
        }

        private static void CreateOrUpdateSubSceneReference(Scene mainScene)
        {
            SceneAsset subSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SubScenePath);
            GameObject subSceneObject = FindRootObject(mainScene, "Demo01_MovingCubes_SubScene");

            if (subSceneObject == null)
            {
                subSceneObject = new GameObject("Demo01_MovingCubes_SubScene");
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
