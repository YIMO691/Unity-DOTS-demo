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
                Subtitle = "1,000 - 50,000 entities",
                Description = "IJobEntity + Burst + ECB\nParallel entity movement\nBoundary wrap"
            },
            new DemoEntry
            {
                SceneName = "Demo02_BouncingBalls",
                Title = "Bouncing Balls",
                Subtitle = "100 - 1,000 physics entities",
                Description = "Unity Physics integration\nPhysicsCollider + PhysicsVelocity\nWall collision and reset volume"
            },
            new DemoEntry
            {
                SceneName = "Demo03_FlockingAgents",
                Title = "Flocking Agents",
                Subtitle = "500 - 10,000 agents",
                Description = "Boids algorithm\nSeparation + Alignment + Cohesion\nSpatial hash optimization"
            },
            new DemoEntry
            {
                SceneName = "Demo04_TowerDefense",
                Title = "Tower Defense",
                Subtitle = "ECS gameplay loop",
                Description = "Wave spawning + tower targeting\nProjectile homing + damage\nRuntime HUD and health bars"
            }
        };

        private const string HubTitle = "Unity DOTS Demo Hub";
        private const string HubSubtitle = "Data-Oriented Technology Stack | Entities 1.3 | Unity 2022.3 LTS";

        private int _selectedIndex = -1;

        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _selectedButtonStyle;
        private GUIStyle _demoTitleStyle;
        private GUIStyle _bodyStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _selectedIndex = -1;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float titleY = screenHeight * 0.12f;

            GUI.Label(new Rect(0f, titleY, screenWidth, 48f), HubTitle, _titleStyle);
            GUI.Label(new Rect(0f, titleY + 48f, screenWidth, 26f), HubSubtitle, _subtitleStyle);

            float panelWidth = Mathf.Min(520f, screenWidth - 40f);
            float panelX = (screenWidth - panelWidth) * 0.5f;
            float panelY = titleY + 98f;
            float buttonHeight = 62f;
            float gap = 8f;
            float panelHeight = Demos.Length * (buttonHeight + gap) - gap + 24f;

            Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            GUI.Box(panelRect, GUIContent.none, _panelStyle);

            float buttonX = panelX + 12f;
            float buttonY = panelY + 12f;
            float buttonWidth = panelWidth - 24f;

            for (int i = 0; i < Demos.Length; i++)
            {
                DemoEntry demo = Demos[i];
                Rect buttonRect = new Rect(buttonX, buttonY, buttonWidth, buttonHeight);
                bool selected = _selectedIndex == i;

                if (GUI.Button(buttonRect, GUIContent.none, selected ? _selectedButtonStyle : _buttonStyle))
                {
                    if (selected)
                    {
                        SceneManager.LoadScene(demo.SceneName);
                    }
                    else
                    {
                        _selectedIndex = i;
                    }
                }

                GUI.Label(
                    new Rect(buttonRect.x + 14f, buttonRect.y + 8f, buttonRect.width - 28f, 24f),
                    demo.Title,
                    _demoTitleStyle);
                GUI.Label(
                    new Rect(buttonRect.x + 14f, buttonRect.y + 32f, buttonRect.width - 28f, 20f),
                    demo.Subtitle,
                    _bodyStyle);

                buttonY += buttonHeight + gap;
            }

            DrawSelectionDetails(panelY + panelHeight + 16f, screenWidth);
        }

        private void DrawSelectionDetails(float top, float screenWidth)
        {
            if (_selectedIndex < 0 || _selectedIndex >= Demos.Length)
            {
                GUI.Label(new Rect(0f, top + 4f, screenWidth, 24f), "Select a demo to begin", _subtitleStyle);
                return;
            }

            DemoEntry entry = Demos[_selectedIndex];
            float detailWidth = Mathf.Min(460f, screenWidth - 40f);
            float detailX = (screenWidth - detailWidth) * 0.5f;
            Rect detailRect = new Rect(detailX, top, detailWidth, 92f);

            GUI.Box(detailRect, GUIContent.none, _panelStyle);
            GUI.Label(
                new Rect(detailRect.x + 14f, detailRect.y + 10f, detailRect.width - 28f, detailRect.height - 20f),
                entry.Description,
                _bodyStyle);
            GUI.Label(
                new Rect(0f, detailRect.yMax + 10f, screenWidth, 24f),
                "Click again to launch | Esc to deselect",
                _subtitleStyle);
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelStyle = GUIStyleHelper.DarkPanel();

            _titleStyle = GUIStyleHelper.TitleLabel(34);
            _titleStyle.alignment = TextAnchor.MiddleCenter;

            _subtitleStyle = GUIStyleHelper.HintLabel(14);
            _subtitleStyle.alignment = TextAnchor.MiddleCenter;

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 14, 8, 8)
            };

            _selectedButtonStyle = new GUIStyle(_buttonStyle)
            {
                normal =
                {
                    background = GUIStyleHelper.MakeTexture(2, 2, new Color(0.25f, 0.45f, 0.75f, 0.85f)),
                    textColor = Color.white
                },
                hover =
                {
                    background = GUIStyleHelper.MakeTexture(2, 2, new Color(0.3f, 0.55f, 0.85f, 0.95f)),
                    textColor = Color.white
                }
            };

            _demoTitleStyle = GUIStyleHelper.TitleLabel(15);
            _demoTitleStyle.alignment = TextAnchor.MiddleLeft;

            _bodyStyle = GUIStyleHelper.BodyLabel(12);
            _bodyStyle.alignment = TextAnchor.MiddleLeft;
        }
    }
}
