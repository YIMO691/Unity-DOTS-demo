using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DOTSDemo.Shared;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityDotsDemo.Demo05;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.EditorTools
{
    [InitializeOnLoad]
    public static class Demo05PathfindingSetup
    {
        private const string DemoAssetRoot = "Assets/DOTS_DemoAssets/Demo05";
        private const string AgentMaterialPath = DemoAssetRoot + "/Demo05_Agent_Material.mat";
        private const string AgentPrefabPath = DemoAssetRoot + "/Demo05_Agent.prefab";
        private const string UrpAssetPath = "Assets/Settings/Demo_URP_Asset.asset";
        private const string MainScenePath = "Assets/Scenes/Demo05_Pathfinding.unity";
        private const string SubScenePath = "Assets/Scenes/Demo05_Pathfinding/Demo05_Pathfinding_SubScene.unity";
        private const string MarkerPath = DemoAssetRoot + "/.demo05_setup_complete";

        static Demo05PathfindingSetup()
        {
            EditorApplication.delayCall += AutoCreateOnce;
        }

        [MenuItem("DOTS Demos/Rebuild Demo 05 Pathfinding")]
        public static void RebuildDemo05()
        {
            SetupDemo(force: true);
        }

        public static void RepairDemo05()
        {
            SetupDemo(force: false);
        }

        private static void AutoCreateOnce()
        {
            if (IsAutomatedRun() || IsImportWorker() || EditorApplication.isPlayingOrWillChangePlaymode)
            {
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

        private static void SetupDemo(bool force)
        {
            try
            {
                EnsureFolders();

                Material agentMaterial = CreateOrUpdateAgentMaterial();
                GameObject agentPrefab = CreateOrUpdateAgentPrefab(agentMaterial);
                EnsureUrpAsset();
                CreateOrUpdateScenes(agentPrefab, force);

                File.WriteAllText(MarkerPath, DateTime.Now.ToString("O"));
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Demo 05 Pathfinding is ready. Open {MainScenePath} and press Play.");
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
                   SceneFileContains(MainScenePath, "Demo05_Pathfinding_SubScene") &&
                   SceneFileContains(MainScenePath, "Demo05 HUD");
        }

        private static bool SceneFileContains(string path, string value)
        {
            return File.Exists(path) && File.ReadAllText(path).Contains(value);
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets", "DOTS_DemoAssets");
            CreateFolderIfMissing("Assets/DOTS_DemoAssets", "Demo05");
            CreateFolderIfMissing("Assets/Scenes", "Demo05_Pathfinding");
        }

        private static void CreateFolderIfMissing(string parent, string folderName)
        {
            string path = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static Material CreateOrUpdateAgentMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AgentMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "Demo05_Agent_Material",
                    color = new Color(1f, 0.55f, 0.1f, 1f)
                };
                AssetDatabase.CreateAsset(material, AgentMaterialPath);
            }
            else
            {
                material.shader = shader;
                material.color = new Color(1f, 0.55f, 0.1f, 1f);
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static GameObject CreateOrUpdateAgentPrefab(Material agentMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AgentPrefabPath);
            if (prefab != null)
            {
                return prefab;
            }

            GameObject agent = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            agent.name = "Demo05_Agent";
            agent.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);

            Collider collider = agent.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshRenderer renderer = agent.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = agentMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;

            prefab = PrefabUtility.SaveAsPrefabAsset(agent, AgentPrefabPath);
            UnityEngine.Object.DestroyImmediate(agent);
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

        private static void CreateOrUpdateScenes(GameObject agentPrefab, bool force)
        {
            Scene mainScene = OpenOrCreateMainScene(force);
            Scene subScene = OpenOrCreateSubScene(force);

            CreateOrUpdateSubSceneObjects(subScene, agentPrefab);
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

        private static void CreateOrUpdateSubSceneObjects(Scene subScene, GameObject agentPrefab)
        {
            CreateOrUpdatePathfindingGrid(subScene);
            CreateOrUpdateAgentSpawner(subScene, agentPrefab);
        }

        private static void CreateOrUpdatePathfindingGrid(Scene subScene)
        {
            GameObject gridObject = FindRootObject(subScene, "Pathfinding Grid");
            if (gridObject == null)
            {
                gridObject = new GameObject("Pathfinding Grid");
                SceneManager.MoveGameObjectToScene(gridObject, subScene);
            }

            PathfindingAuthoring authoring = EnsureSingleComponent<PathfindingAuthoring>(gridObject);
            authoring.GridWidth = 40;
            authoring.GridHeight = 40;
            authoring.CellSize = 1f;
            authoring.WorldOrigin = new Vector3(-20f, 0f, -20f);
            authoring.TargetPosition = new Vector3(0f, 0f, 0f);

            EditorUtility.SetDirty(gridObject);
            EditorUtility.SetDirty(authoring);
        }

        private static void CreateOrUpdateAgentSpawner(Scene subScene, GameObject agentPrefab)
        {
            GameObject spawnerObject = FindRootObject(subScene, "Agent Spawner");
            if (spawnerObject == null)
            {
                spawnerObject = new GameObject("Agent Spawner");
                SceneManager.MoveGameObjectToScene(spawnerObject, subScene);
            }

            AgentSpawnerAuthoring authoring = EnsureSingleComponent<AgentSpawnerAuthoring>(spawnerObject);
            authoring.AgentPrefab = agentPrefab;
            authoring.Count = 200;
            authoring.MoveSpeed = 3f;
            authoring.SpawnRadius = 18f;
            authoring.RandomSeed = 42;

            EditorUtility.SetDirty(spawnerObject);
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
            camera.transform.position = new Vector3(0f, 30f, -25f);
            camera.transform.LookAt(Vector3.zero);
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
            light.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = new Color(1f, 0.95f, 0.85f);

            GameObject hudObject = FindRootObject(mainScene, "Demo05 HUD");
            if (hudObject == null)
            {
                hudObject = new GameObject("Demo05 HUD");
                SceneManager.MoveGameObjectToScene(hudObject, mainScene);
            }

            DemoHUD hud = EnsureSingleComponent<DemoHUD>(hudObject);
            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("demoName").stringValue = "Flow Field Pathfinding";
            serializedHud.FindProperty("techDescription").stringValue =
                "BFS gradient field\nBufferLookup + Burst\n200+ agents in parallel";
            serializedHud.FindProperty("controlsHint").stringValue =
                "Move target in Scene view to redirect agents";
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);

            EnsureSingleComponent<DemoBackButton>(hudObject);

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
            GameObject subSceneObject = FindRootObject(mainScene, "Demo05_Pathfinding_SubScene");

            if (subSceneObject == null)
            {
                subSceneObject = new GameObject("Demo05_Pathfinding_SubScene");
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
