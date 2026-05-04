using UnityEngine;
using UnityEngine.SceneManagement;

namespace DOTSDemo.Shared
{
    public sealed class DemoBackButton : MonoBehaviour
    {
        private GUIStyle _buttonStyle;

        private void OnGUI()
        {
            EnsureStyles();

            float width = 150f;
            float height = 34f;
            float margin = 14f;
            Rect rect = new Rect(Screen.width - width - margin, Screen.height - height - margin, width, height);

            if (GUI.Button(rect, "Back to Hub", _buttonStyle))
            {
                SceneManager.LoadScene("DemoHub");
            }
        }

        private void EnsureStyles()
        {
            if (_buttonStyle != null)
            {
                return;
            }

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
