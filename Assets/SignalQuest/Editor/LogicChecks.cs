using System;
using UnityEditor;
using UnityEngine;

namespace SignalQuest.Editor
{
    // Editor checks call the actual C# simulation; no NUnit package is required.
    public static class LogicChecks
    {
        [MenuItem("Signal Quest/Run Logic Checks")]
        public static void Run()
        {
            int assertions = 0;
            int[][] expected = { new[] {0,0,0,1}, new[] {0,1,1,1}, new[] {0,1,1,0} };
            foreach (BinaryGate gate in Enum.GetValues(typeof(BinaryGate)))
                for (int i = 0; i < 4; i++)
                { Equal(CircuitLogic.Gate(gate, i/2, i%2), expected[(int)gate][i]); assertions++; }
            for (int bit = 0; bit < 2; bit++)
            {
                Equal(CircuitLogic.Stage(OutputStage.NOT, bit), 1-bit);
                Equal(CircuitLogic.Stage(OutputStage.BUFFER, bit), bit); assertions += 2;
            }
            var defaults = CircuitLogic.Builtins();
            foreach (var mission in defaults)
            { CircuitLogic.Validate(mission); Equal(CircuitLogic.CountSolutions(mission), 1); assertions++; }
            Equal(CircuitLogic.Diagnose(defaults[0], BinaryGate.OR, OutputStage.BUFFER).Correct, 2); assertions++;
            Equal(CircuitLogic.Evaluate(true, BinaryGate.OR, OutputStage.NOT, 0, 0), 1); assertions++;
            ExpectFailure(() => CircuitLogic.Gate(BinaryGate.AND, 2, 0)); assertions++;
            ExpectFailure(() => CircuitLogic.Gate((BinaryGate)99, 0, 0)); assertions++;
            var impossible = CircuitLogic.Builtins()[0]; impossible.Target = new[] {0,0,1,1};
            ExpectFailure(() => CircuitLogic.Validate(impossible)); assertions++;
            Debug.Log("Signal Quest: " + assertions + " logic assertions passed. This does not verify rendering or input.");
        }

        private static void Equal(int actual, int expected)
        {
            if (actual != expected) throw new InvalidOperationException("Logic check failed: expected " + expected + ", got " + actual);
        }
        private static void ExpectFailure(Action action)
        {
            try { action(); }
            catch (ArgumentException) { return; }
            throw new InvalidOperationException("Expected invalid input to be rejected.");
        }
    }
}
