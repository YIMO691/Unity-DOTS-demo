using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Unity.Entities;
using UnityDotsDemo.Demo03;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UnityDotsDemo.Tests.PlayMode
{
    /// <summary>
    /// Runs each demo scene with its default settings and measures
    /// average FPS, frame time, and GC allocation over a fixed window.
    /// Results are logged as BENCHMARK_RESULT: lines for easy parsing.
    /// Run in Unity Editor Test Runner for accurate rendering timings.
    /// </summary>
    public sealed class PerformanceBenchmarks
    {
        private const int WarmupFrames = 300;
        private const int MeasureFrames = 600;
        private const string Tag = "BENCHMARK_RESULT:";

        [UnityTest]
        public IEnumerator Demo01_MovingCubes_Default()
        {
            yield return MeasureScene("Demo01_MovingCubes", "MovingCubes", 1000, "Burst");
        }

        [UnityTest]
        public IEnumerator Demo02_BouncingBalls_Default()
        {
            yield return MeasureScene("Demo02_BouncingBalls", "BouncingBalls", 200, "Physics");
        }

        [UnityTest]
        public IEnumerator Demo03_FlockingAgents_Basic()
        {
            yield return MeasureScene("Demo03_FlockingAgents", "FlockingAgents", 500, "Basic");
        }

        [UnityTest]
        public IEnumerator Demo03_FlockingAgents_SpatialHash()
        {
            yield return MeasureScene("Demo03_FlockingAgents", "FlockingAgents", 500, "SpatialHash");
        }

        [UnityTest]
        public IEnumerator Demo04_TowerDefense_Default()
        {
            yield return MeasureScene("Demo04_TowerDefense", "TowerDefense", 0, "FullRun");
        }

        [UnityTest]
        public IEnumerator Demo05_FlowField_Default()
        {
            yield return MeasureScene("Demo05_Pathfinding", "FlowField", 200, "Default");
        }

        private static IEnumerator MeasureScene(
            string sceneName, string demo, int entityCount, string variant)
        {
            SceneManager.LoadScene(sceneName);
            yield return null;

            // Switch to SpatialHash mode for Demo03 variant
            if (demo == "FlockingAgents" && variant == "SpatialHash")
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world != null && world.IsCreated)
                {
                    var modeEntity = world.EntityManager.CreateEntity();
                    world.EntityManager.AddComponentData(modeEntity,
                        new UnityDotsDemo.Demo03.BoidSimulationModeData
                        {
                            Mode = UnityDotsDemo.Demo03.BoidSimulationMode.SpatialHash,
                            CellSize = 4f
                        });
                }
            }

            // Warmup
            for (int i = 0; i < WarmupFrames; i++)
                yield return null;

            // Measure
            var frameTimes = new List<float>(MeasureFrames);
            long gcStart = GC.GetTotalMemory(false);
            int peakEntityCount = 0;

            for (int i = 0; i < MeasureFrames; i++)
            {
                frameTimes.Add(Time.unscaledDeltaTime);

                // Track peak entity count for Demo04
                if (demo == "TowerDefense" && i % 60 == 0)
                {
                    var world = World.DefaultGameObjectInjectionWorld;
                    if (world != null && world.IsCreated)
                    {
                        int count = world.EntityManager.UniversalQuery.CalculateEntityCount();
                        if (count > peakEntityCount) peakEntityCount = count;
                    }
                }

                yield return null;
            }

            long gcEnd = GC.GetTotalMemory(false);

            // Final entity count
            int finalEntityCount = 0;
            var w = World.DefaultGameObjectInjectionWorld;
            if (w != null && w.IsCreated)
                finalEntityCount = w.EntityManager.UniversalQuery.CalculateEntityCount();

            // Stats
            frameTimes.Sort();
            float total = 0f;
            foreach (float t in frameTimes) total += t;
            float avgDt = total / frameTimes.Count;
            float avgFps = 1f / Mathf.Max(avgDt, 0.0001f);
            float frameMs = avgDt * 1000f;
            float p95Dt = frameTimes[(int)(frameTimes.Count * 0.95f)];
            float p95Ms = p95Dt * 1000f;
            long gcDelta = gcEnd - gcStart;
            float gcPerFrame = (float)gcDelta / frameTimes.Count;
            int reportedCount = demo == "TowerDefense" ? peakEntityCount : finalEntityCount;

            var sb = new StringBuilder();
            sb.Append(Tag);
            sb.Append(" demo="); sb.Append(demo);
            sb.Append(" variant="); sb.Append(variant);
            sb.Append(" entities="); sb.Append(reportedCount);
            sb.Append(" avgFps="); sb.Append(Mathf.RoundToInt(avgFps));
            sb.Append(" frameMs="); sb.Append(frameMs.ToString("F2"));
            sb.Append(" p95Ms="); sb.Append(p95Ms.ToString("F2"));
            sb.Append(" gcBytesPerFrame="); sb.Append(Mathf.RoundToInt(gcPerFrame));
            sb.Append(" frames="); sb.Append(frameTimes.Count);

            Debug.Log(sb.ToString());
        }
    }
}
