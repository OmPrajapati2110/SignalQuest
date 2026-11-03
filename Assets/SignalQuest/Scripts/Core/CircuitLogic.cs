using System;
using System.Collections.Generic;

namespace SignalQuest
{
    public enum BinaryGate { AND, OR, XOR }
    public enum OutputStage { BUFFER, NOT }

    [Serializable]
    public sealed class MissionData
    {
        public string Id, Title, Brief, Hint;
        public int[] Target;
        public bool Cascade;
    }

    public struct TruthRow
    {
        public int A, B, Expected, Actual;
        public bool Passed { get { return Expected == Actual; } }
    }

    public sealed class DiagnosticResult
    {
        public readonly List<TruthRow> Rows = new List<TruthRow>();
        public int Correct;
        public bool Passed { get { return Correct == 4; } }
    }

    // Pure C#: intentionally independent of Unity's scene, UI and save systems.
    public static class CircuitLogic
    {
        public static int Bit(int value)
        {
            if (value != 0 && value != 1) throw new ArgumentOutOfRangeException(nameof(value), "Signals must be 0 or 1.");
            return value;
        }

        public static int Gate(BinaryGate gate, int a, int b)
        {
            Bit(a); Bit(b);
            switch (gate)
            {
                case BinaryGate.AND: return a & b;
                case BinaryGate.OR: return a | b;
                case BinaryGate.XOR: return a ^ b;
                default: throw new ArgumentOutOfRangeException(nameof(gate));
            }
        }

        public static int Stage(OutputStage stage, int input)
        {
            Bit(input);
            switch (stage)
            {
                case OutputStage.BUFFER: return input;
                case OutputStage.NOT: return 1 - input;
                default: throw new ArgumentOutOfRangeException(nameof(stage));
            }
        }

        public static int Evaluate(bool cascade, BinaryGate gate, OutputStage stage, int a, int b)
        {
            int first = Gate(gate, a, b);
            return cascade ? Stage(stage, first) : first;
        }

        public static DiagnosticResult Diagnose(MissionData mission, BinaryGate gate, OutputStage stage)
        {
            ValidateShape(mission);
            var result = new DiagnosticResult();
            for (int i = 0; i < 4; i++)
            {
                var row = new TruthRow { A = i / 2, B = i % 2, Expected = mission.Target[i] };
                row.Actual = Evaluate(mission.Cascade, gate, stage, row.A, row.B);
                if (row.Passed) result.Correct++;
                result.Rows.Add(row);
            }
            return result;
        }

        public static int CountSolutions(MissionData mission)
        {
            ValidateShape(mission);
            int count = 0;
            foreach (BinaryGate gate in Enum.GetValues(typeof(BinaryGate)))
                for (int stage = 0; stage < (mission.Cascade ? 2 : 1); stage++)
                    if (Diagnose(mission, gate, (OutputStage)stage).Passed) count++;
            return count;
        }

        public static void Validate(MissionData mission)
        {
            ValidateShape(mission);
            if (CountSolutions(mission) == 0) throw new ArgumentException("Mission target cannot be solved with the available gates.");
        }

        private static void ValidateShape(MissionData mission)
        {
            if (mission == null) throw new ArgumentNullException(nameof(mission));
            if (string.IsNullOrWhiteSpace(mission.Id) || string.IsNullOrWhiteSpace(mission.Title)
                || string.IsNullOrWhiteSpace(mission.Brief) || string.IsNullOrWhiteSpace(mission.Hint))
                throw new ArgumentException("Mission ID, title, briefing and hint must be filled in.");
            if (mission.Target == null || mission.Target.Length != 4)
                throw new ArgumentException("Target requires four bits in input order 00, 01, 10, 11.");
            foreach (int value in mission.Target) Bit(value);
        }

        public static MissionData[] Builtins()
        {
            return new[]
            {
                new MissionData { Id = "airlock", Title = "Airlock interlock", Cascade = false,
                    Brief = "A and B are door-lock sensors. Enable relay Y only when both doors are locked.",
                    Hint = "Which gate requires BOTH inputs to be active?", Target = new[] { 0, 0, 0, 1 } },
                new MissionData { Id = "beacon", Title = "Distress beacon", Cascade = false,
                    Brief = "A and B are distress switches. Enable Y if either or both switches are pressed.",
                    Hint = "Include the case where both switches are pressed.", Target = new[] { 0, 1, 1, 1 } },
                new MissionData { Id = "router", Title = "Power route selector", Cascade = false,
                    Brief = "A and B request power routes. Enable Y only when exactly one route is requested.",
                    Hint = "The inputs must be different.", Target = new[] { 0, 1, 1, 0 } },
                new MissionData { Id = "shield", Title = "Radiation shield", Cascade = true,
                    Brief = "A and B are radiation alarms. The safe-to-open relay Y must be 1 only when neither alarm is active.",
                    Hint = "Detect ANY alarm first, then invert the result.", Target = new[] { 1, 0, 0, 0 } }
            };
        }
    }
}
