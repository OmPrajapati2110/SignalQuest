using UnityEngine;
using UnityEngine.UIElements;

namespace SignalQuest
{
    public sealed class StationView : MonoBehaviour
    {
        public Camera stationCamera;
        public StationModule[] modules;
        private bool[] powered = new bool[0];
        private bool reduceMotion;
        private VisualElement window, root;

        public void Bind(VisualElement viewport, VisualElement uiRoot)
        {
            window = viewport; root = uiRoot;
        }

        public void SetProgress(bool[] states, bool reduced)
        {
            powered = (bool[])states.Clone(); reduceMotion = reduced;
        }

        private void LateUpdate()
        {
            if (stationCamera != null && window != null && root != null)
            {
                float w = root.resolvedStyle.width, h = root.resolvedStyle.height;
                Rect bounds = window.worldBound;
                if (w > 0 && h > 0 && bounds.width > 0 && bounds.height > 0)
                {
                    stationCamera.rect = new Rect(bounds.x / w, 1f - bounds.yMax / h, bounds.width / w, bounds.height / h);
                    float aspect = (Screen.width * bounds.width / w) / (Screen.height * bounds.height / h);
                    stationCamera.orthographicSize = Mathf.Max(1.2f, 6.8f / aspect);
                }
            }
            if (modules == null) return;
            float pulse = reduceMotion ? 1f : 0.82f + 0.18f * Mathf.Sin(Time.unscaledTime * 2f);
            for (int i = 0; i < modules.Length; i++)
                if (modules[i] != null) modules[i].ShowState(i < powered.Length && powered[i], pulse);
        }
    }
}
