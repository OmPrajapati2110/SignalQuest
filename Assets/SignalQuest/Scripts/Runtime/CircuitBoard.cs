using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SignalQuest
{
    // A UI Toolkit element: vector wires plus native focusable input buttons.
    public sealed class CircuitBoard : VisualElement
    {
        private readonly Button inputA, inputB;
        private readonly Label first, second, output;
        private int a, b, middle, y;
        private bool cascade;
        private readonly Color high = new Color(0.55f, 0.95f, 0.80f);
        private readonly Color low = new Color(0.40f, 0.49f, 0.60f);

        public CircuitBoard(Action toggleA, Action toggleB)
        {
            AddToClassList("circuit-board");
            inputA = new Button(toggleA); inputB = new Button(toggleB);
            first = new Label(); second = new Label(); output = new Label();
            Add(inputA); Add(inputB); Add(first); Add(second); Add(output);
            foreach (var child in Children()) child.AddToClassList("circuit-node");
            inputA.AddToClassList("input-node"); inputB.AddToClassList("input-node");
            output.AddToClassList("output-node");
            RegisterCallback<GeometryChangedEvent>(_ => PositionNodes());
            generateVisualContent += Draw;
        }

        public void Show(BinaryGate gate, OutputStage stage, bool twoStages, int input0, int input1)
        {
            cascade = twoStages; a = input0; b = input1;
            middle = CircuitLogic.Gate(gate, a, b);
            y = cascade ? CircuitLogic.Stage(stage, middle) : middle;
            inputA.text = "A = " + a; inputB.text = "B = " + b;
            first.text = gate + "\nOUT " + middle; second.text = stage + "\nOUT " + y;
            output.text = "RELAY Y\n" + y;
            second.style.display = cascade ? DisplayStyle.Flex : DisplayStyle.None;
            inputA.EnableInClassList("active-signal", a == 1);
            inputB.EnableInClassList("active-signal", b == 1);
            output.EnableInClassList("active-signal", y == 1);
            PositionNodes(); MarkDirtyRepaint();
        }

        private void PositionNodes()
        {
            float w = contentRect.width, h = contentRect.height;
            Place(inputA, .12f * w, .27f * h, 72, 42);
            Place(inputB, .12f * w, .73f * h, 72, 42);
            Place(first, (cascade ? .35f : .48f) * w, .5f * h, 92, 64);
            Place(second, .65f * w, .5f * h, 92, 64);
            Place(output, .87f * w, .5f * h, 86, 64);
        }

        private static void Place(VisualElement node, float x, float y, float width, float height)
        {
            node.style.position = Position.Absolute;
            node.style.left = x - width / 2; node.style.top = y - height / 2;
            node.style.width = width; node.style.height = height;
        }

        private void Draw(MeshGenerationContext context)
        {
            float w = contentRect.width, h = contentRect.height;
            if (w <= 0 || h <= 0) return;
            float gx = (cascade ? .35f : .48f) * w;
            var painter = context.painter2D;
            painter.lineWidth = 3;
            Path(painter, a, new Vector2(.12f*w+36, .27f*h), new Vector2(.23f*w,.27f*h), new Vector2(.23f*w,.5f*h-12), new Vector2(gx-46,.5f*h-12));
            Path(painter, b, new Vector2(.12f*w+36, .73f*h), new Vector2(.23f*w,.73f*h), new Vector2(.23f*w,.5f*h+12), new Vector2(gx-46,.5f*h+12));
            if (cascade)
            {
                Path(painter, middle, new Vector2(gx+46,.5f*h), new Vector2(.65f*w-46,.5f*h));
                Path(painter, y, new Vector2(.65f*w+46,.5f*h), new Vector2(.87f*w-43,.5f*h));
            }
            else Path(painter, y, new Vector2(gx+46,.5f*h), new Vector2(.87f*w-43,.5f*h));
        }

        private void Path(Painter2D painter, int value, params Vector2[] points)
        {
            painter.strokeColor = value == 1 ? high : low;
            painter.BeginPath(); painter.MoveTo(points[0]);
            for (int i = 1; i < points.Length; i++) painter.LineTo(points[i]);
            painter.Stroke();
        }
    }
}
