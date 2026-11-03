using UnityEngine;

namespace SignalQuest
{
    public sealed class StationModule : MonoBehaviour
    {
        public SpriteRenderer indicator;
        public void ShowState(bool powered, float pulse)
        {
            if (indicator == null) return;
            indicator.color = powered ? new Color(0.55f, 0.95f, 0.80f, pulse) : new Color(0.85f, 0.43f, 0.28f, 1f);
        }
    }
}
