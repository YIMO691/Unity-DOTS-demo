using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UnityDotsDemo.Tests.PlayMode
{
    public sealed class DemoSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator Demo01RunsFor300Frames()
        {
            yield return LoadAndRunFrames("Demo01_MovingCubes");
        }

        [UnityTest]
        public IEnumerator Demo02RunsFor300Frames()
        {
            yield return LoadAndRunFrames("Demo02_BouncingBalls");
        }

        [UnityTest]
        public IEnumerator Demo03RunsFor300Frames()
        {
            yield return LoadAndRunFrames("Demo03_FlockingAgents");
        }

        [UnityTest]
        public IEnumerator Demo04RunsFor300Frames()
        {
            yield return LoadAndRunFrames("Demo04_TowerDefense");
        }

        [UnityTest]
        public IEnumerator Demo05RunsFor300Frames()
        {
            yield return LoadAndRunFrames("Demo05_Pathfinding");
        }

        [UnityTest]
        public IEnumerator DemoHubRunsFor300Frames()
        {
            yield return LoadAndRunFrames("DemoHub");
        }

        private static IEnumerator LoadAndRunFrames(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            yield return null;

            for (int i = 0; i < 300; i++)
            {
                yield return null;
            }
        }
    }
}
