using UnityEngine;

namespace SignalQuest
{
    [CreateAssetMenu(fileName = "Mission", menuName = "Signal Quest/Mission")]
    public sealed class MissionDefinition : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea(2, 5)] public string brief;
        [TextArea(2, 4)] public string hint;
        public bool cascade;
        [Tooltip("Four target bits for input pairs 00, 01, 10, 11.")]
        public int[] target = new int[4];

        public MissionData ToData()
        {
            return new MissionData { Id = id, Title = title, Brief = brief, Hint = hint,
                Cascade = cascade, Target = target == null ? null : (int[])target.Clone() };
        }

        public void SetData(MissionData data)
        {
            CircuitLogic.Validate(data);
            id = data.Id; title = data.Title; brief = data.Brief; hint = data.Hint;
            cascade = data.Cascade; target = (int[])data.Target.Clone();
        }
    }
}
