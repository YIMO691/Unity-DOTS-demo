using System;
using System.IO;
using System.Linq;
using DOTSDemo.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityDotsDemo.EditorTools
{
    [InitializeOnLoad]
    public static class DemoHubSetup
    {
        private const string MainScenePath = "Assets/Scenes/DemoHub.unity";
        private const string MarkerPath = "Assets/DOTS_DemoAssets/.demohub_setup_complete";

        private static readonly string[] DemoScenePaths =
        {
            "Assets/Scenes/Demo01_MovingCubes.unity",
            "Assets/Scenes/Demo02_BouncingBalls.unity",
            "Assets/Scenes/Demo03_FlockingAgents.unity",
            "Assets/Scenes/Demo04_TowerDefense.unity"
        };

        static DemoHubSetup()
        {
            EditorApplication.delayCall += AutoCreateOnce;
        }

        [MenuItem("DOTS Demos/Rebuild Demo Hub")]
        public static void RebuildDemoHub()
        {
            SetupDemoHub(force: true);
        }

        [MenuItem("DOTS Demos/Add Back Buttons To Demo Scenes")]
        public static void AddBackButtons()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Back buttons can only be added in Edit Mode.");
                return;
            }

            EnsureBackButtons();
            Debug.Log("Back buttons added to all demo scenes.");
        }

        private static void AutoCreateOnce()
        {
            if (IsImportWorker() || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoCreateOnce;
                return;
            }

            if (File.Exists(MarkerPath) && File.Exists(MainScenePath))
            {
                // Setup already done. Do NOT call SaveAssets or modify scenes
                // on every domain reload — that triggers an infinite reload loop.
                return;
            }

            SetupDemoHub(force: false);
        }

        private static void SetupDemoHub(bool force)
        {
            try
            {
                CreateOrUpdateScene(force);
                EnsureAllScenesInBuildSettings();
                EnsureBackButtons();

                File.WriteAllText(MarkerPath, DateTime.Now.ToString("O"));
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();

                Debug.Log($"Demo Hub is ready. Open {MainScenePath} and press Play.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void CreateOrUpdateScene(bool force)
        {
            if (force && File.Exists(MainScenePath))
            {
                AssetDatabase.DeleteAsset(MainScenePath);
            }

            // Use Single mode — untitled default scenes can't be closed and block additive creation.
            Scene scene;
            if (File.Exists(MainScenePath))
            {
                scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, MainScenePath);
            }

            // Remove default objects we don't need
            foreach (GameObject root in scene.GetRootGameObjects().ToList())
            {
                if (root.name == "Main Camera")
                {
                    Camera cam = root.GetComponent<Camera>();
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
                        cam.tag = "MainCamera";
                    }
                }
            }

            // Add Hub UI
            GameObject hubObject = FindRootObject(scene, "DemoHub UI");
            if (hubObject == null)
            {
                hubObject = new GameObject("DemoHub UI");
                SceneManager.MoveGameObjectToScene(hubObject, scene);
            }

            DemoHubUI hubUI = hubObject.GetComponent<DemoHubUI>();
            if (hubUI == null)
            {
                hubUI = hubObject.AddComponent<DemoHubUI>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static void EnsureAllScenesInBuildSettings()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;

            // Put DemoHub first, then demo scenes
            string[] allPaths = new[] { MainScenePath }.Concat(DemoScenePaths).ToArray();

            foreach (string path in allPaths)
            {
                if (!existing.Any(s => s.path == path))
                {
                    existing = existing.Concat(
                        new[] { new EditorBuildSettingsScene(path, true) }).ToArray();
                }
            }

            // Move DemoHub to index 0
            int hubIndex = Array.FindIndex(existing, s => s.path == MainScenePath);
            if (hubIndex > 0)
            {
                var list = existing.ToList();
                var hub = list[hubIndex];
                list.RemoveAt(hubIndex);
                list.Insert(0, hub);
                existing = list.ToArray();
            }

            EditorBuildSettings.scenes = existing;
        }

        /// <summary>
        /// <summary>
        /// Patches existing demo scenes to include a DemoBackButton component on their HUD GameObjects.
        /// Safe to call multiple times — never duplicates.
        /// </summary>
        public static void EnsureBackButtons()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            foreach (string scenePath in DemoScenePaths)
            {
                if (!File.Exists(scenePath)) continue;

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    bool changed = false;

                    // Remove any orphaned "Demo Back Button" GameObjects from previous broken patcher.
                    foreach (GameObject root in scene.GetRootGameObjects().ToList())
                    {
                        if (root.name == "Demo Back Button")
                        {
                            UnityEngine.Object.DestroyImmediate(root);
                            changed = true;
                        }
                    }

                    // Add DemoBackButton to the existing HUD GameObject.
                    DemoHUD hud = FindHudInScene(scene);
                    if (hud != null && hud.GetComponent<DemoBackButton>() == null)
                    {
                        hud.gameObject.AddComponent<DemoBackButton>();
                        EditorUtility.SetDirty(hud.gameObject);
                        changed = true;
                    }

                    if (changed)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene, scenePath);
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        private static DemoHUD FindHudInScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                DemoHUD hud = root.GetComponentInChildren<DemoHUD>(includeInactive: true);
                if (hud != null) return hud;
            }
            return null;
        }

        private static bool IsImportWorker()
        {
            string cmd = Environment.CommandLine;
            return cmd.IndexOf("AssetImportWorker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   cmd.IndexOf("-assetImportWorker", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static GameObject FindRootObject(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(r => r.name == name);
        }
    }
}
