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
            if (IsAutomatedRun() || IsImportWorker() || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoCreateOnce;
                return;
            }

            if (File.Exists(MarkerPath) && File.Exists(MainScenePath))
            {
                EnsureAllScenesInBuildSettings();
                EnsureBackButtons();
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

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != "Main Camera")
                {
                    continue;
                }

                Camera camera = root.GetComponent<Camera>();
                if (camera == null)
                {
                    continue;
                }

                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
                camera.tag = "MainCamera";
                EditorUtility.SetDirty(camera);
            }

            GameObject hubObject = FindRootObject(scene, "DemoHub UI");
            if (hubObject == null)
            {
                hubObject = new GameObject("DemoHub UI");
                SceneManager.MoveGameObjectToScene(hubObject, scene);
            }

            if (hubObject.GetComponent<DemoHubUI>() == null)
            {
                hubObject.AddComponent<DemoHubUI>();
            }

            EditorUtility.SetDirty(hubObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static void EnsureAllScenesInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            bool changed = false;

            string[] requiredPaths = new[] { MainScenePath }.Concat(DemoScenePaths).ToArray();
            foreach (string path in requiredPaths)
            {
                int sceneIndex = scenes.FindIndex(scene => scene.path == path);
                if (sceneIndex >= 0)
                {
                    if (!scenes[sceneIndex].enabled)
                    {
                        scenes[sceneIndex] = new EditorBuildSettingsScene(path, true);
                        changed = true;
                    }

                    continue;
                }

                scenes.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
            }

            int hubIndex = scenes.FindIndex(scene => scene.path == MainScenePath);
            if (hubIndex > 0)
            {
                EditorBuildSettingsScene hubScene = scenes[hubIndex];
                scenes.RemoveAt(hubIndex);
                scenes.Insert(0, hubScene);
                changed = true;
            }

            if (changed)
            {
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        public static void EnsureBackButtons()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            foreach (string scenePath in DemoScenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    bool changed = EnsureSingleBackButton(scene);
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

        private static bool EnsureSingleBackButton(Scene scene)
        {
            DemoBackButton[] buttons = FindBackButtons(scene);
            if (buttons.Length == 0)
            {
                GameObject buttonObject = FindRootObject(scene, "Demo Back Button");
                if (buttonObject == null)
                {
                    buttonObject = new GameObject("Demo Back Button");
                    SceneManager.MoveGameObjectToScene(buttonObject, scene);
                }

                buttonObject.AddComponent<DemoBackButton>();
                EditorUtility.SetDirty(buttonObject);
                return true;
            }

            if (buttons.Length == 1)
            {
                return false;
            }

            DemoBackButton keep = buttons.FirstOrDefault(button => button.gameObject.name != "Demo Back Button") ??
                                  buttons[0];
            foreach (DemoBackButton duplicate in buttons)
            {
                if (duplicate == keep)
                {
                    continue;
                }

                GameObject duplicateObject = duplicate.gameObject;
                if (duplicateObject.scene == scene &&
                    duplicateObject.name == "Demo Back Button" &&
                    duplicateObject.transform.parent == null)
                {
                    UnityEngine.Object.DestroyImmediate(duplicateObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(duplicate);
                }
            }

            EditorUtility.SetDirty(keep.gameObject);
            return true;
        }

        private static DemoBackButton[] FindBackButtons(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<DemoBackButton>(includeInactive: true))
                .ToArray();
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

        private static GameObject FindRootObject(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(root => root.name == name);
        }
    }
}
