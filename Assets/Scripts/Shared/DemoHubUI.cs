using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DOTSDemo.Shared
{
    public sealed class DemoHubUI : MonoBehaviour
    {
        private struct DemoEntry
        {
            public string SceneName;
            public string Title;
            public string Subtitle;
            public string Description;
        }

        private static readonly DemoEntry[] Demos =
        {
            new DemoEntry
            {
                SceneName = "Demo01_MovingCubes",
                Title = "Moving Cubes",
                Subtitle = "1,000 – 50,000 entities",
                Description = "IJobEntity + Burst + ECB\nParallel entity movement\nBoundary wrap"
            },
            new DemoEntry
            {
                SceneName = "Demo02_BouncingBalls",
                Title = "Bouncing Balls",
                Subtitle = "100 – 1,000 entities",
                Description = "Unity Physics integration\nPhysicsCollider + PhysicsVelocity\nPhysicsDamping + PhysicsMass"
            },
            new DemoEntry
            {
                SceneName = "Demo03_FlockingAgents",
                Title = "Flocking Agents",
                Subtitle = "500 – 10,000 agents",
                Description = "Boids algorithm\nSeparation + Alignment + Cohesion\nSpatial hash optimization (press M to toggle)"
            },
            new DemoEntry
            {
                SceneName = "Demo04_TowerDefense",
                Title = "Tower Defense",
                Subtitle = "100 – 500 entities",
                Description = "Full ECS gameplay loop\nWave spawning + Tower targeting\nProjectile homing + Damage + Cleanup"
            }
        };

        private const string HubTitle = "Unity DOTS Demo Hub";
        private const string HubSubtitle = "Data-Oriented Technology Stack · Entities 1.3 · Unity 2022.3 LTS";

        private int _selectedIndex = -1;
        private Vector2 _scrollPosition;

        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _descTitleStyle;
        private GUIStyle _descBodyStyle;

        private void OnGUI()
        {
            EnsureStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // --- Title area ---
            float titleY = sh * 0.12f;
            Rect titleRect = new Rect(0f, titleY, sw, 80f);
            GUI.Label(titleRect, HubTitle, _titleStyle);

            Rect subRect = new Rect(0f, titleY + 55f, sw, 30f);
            GUI.Label(subRect, HubSubtitle, _subtitleStyle);

            // --- Button panel ---
            float panelW = Mathf.Min(480f, sw - 40f);
            float panelX = (sw - panelW) * 0.5f;
            float panelY = titleY + 110f;
            float buttonH = 56f;
            float gap = 8f;
            float panelH = Demos.Length * (buttonH + gap) - gap + 24f;

            Rect panelRect = new Rect(panelX, panelY, panelW, panelH);
            GUI.Box(panelRect, GUIContent.none, _panelStyle);

            float buttonX = panelX + 12f;
            float buttonW = panelW - 24f;
            float buttonY = panelY + 12f;

            for (int i = 0; i < Demos.Length; i++)
            {
                Rect btnRect = new Rect(buttonX, buttonY, buttonW, buttonH);
                bool isSelected = _selectedIndex == i;

                GUIStyle style = isSelected ? _buttonStyle : GUI.skin.button;
                if (GUI.Button(btnRect, GUIContent.none, style))
                {
                    if (_selectedIndex == i)
                    {
                        SceneManager.LoadScene(Demos[i].SceneName);
                    }
                    else
                    {
                        _selectedIndex = i;
                    }
                }

                // Draw button label manually for two-line layout
                Rect labelRect = new Rect(btnRect.x + 14f, btnRect.y + 8f, btnRect.width - 28f, 22f);
                GUI.Label(labelRect, Demos[i].Title, _descTitleStyle);

                Rect subLabelRect = new Rect(btnRect.x + 14f, btnRect.y + 28f, btnRect.width - 28f, 20f);
                GUI.Label(subLabelRect, Demos[i].Subtitle, _descBodyStyle);

                buttonY += buttonH + gap;
            }

            // --- Description panel ---
            if (_selectedIndex >= 0 && _selectedIndex < Demos.Length)
            {
                DemoEntry entry = Demos[_selectedIndex];

                float descW = Mathf.Min(420f, sw - 40f);
                float descX = (sw - descW) * 0.5f;
                float descY = panelY + panelH + 16f;
                float descH = 80f;

                Rect descPanelRect = new Rect(descX, descY, descW, descH);
                GUI.Box(descPanelRect, GUIContent.none, _panelStyle);

                Rect descTextRect = new Rect(descX + 14f, descY + 10f, descW - 28f, descH - 20f);
                GUI.Label(descTextRect, entry.Description, _descBodyStyle);

                Rect hintRect = new Rect(0f, descY + descH + 12f, sw, 24f);
                GUI.Label(hintRect, "Click again to launch · Esc to deselect", _subtitleStyle);
            }
            else
            {
                Rect hintRect = new Rect(0f, panelY + panelH + 20f, sw, 24f);
                GUI.Label(hintRect, "Select a demo to begin", _subtitleStyle);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _selectedIndex = -1;
            }
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null) return;

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.85f)) },
                padding = new RectOffset(12, 12, 10, 10)
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                normal = { textColor = Color.white }
            };

            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                normal = { textColor = new Color(0.65f, 0.65f, 0.7f) }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { background = MakeTexture(2, 2, new Color(0.25f, 0.45f, 0.75f, 0.8f)),
                           textColor = Color.white },
                hover = { background = MakeTexture(2, 2, new Color(0.3f, 0.55f, 0.85f, 0.9f)),
                          textColor = Color.white },
                padding = new RectOffset(14, 14, 8, 8)
            };

            _descTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            _descBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                normal = { textColor = new Color(0.7f, 0.7f, 0.75f) }
            };
        }

        private static Texture2D MakeTexture(int w, int h, Color color)
        {
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            Texture2D tex = new Texture2D(w, h);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
