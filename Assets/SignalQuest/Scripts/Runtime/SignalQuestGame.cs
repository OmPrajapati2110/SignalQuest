using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SignalQuest
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SignalQuestGame : MonoBehaviour
    {
        public MissionDefinition[] missions;
        public StationView station;
        private VisualElement root, navigation, table;
        private Label title, brief, feedback, progress, notes;
        private DropdownField gateMenu, stageMenu;
        private Button hintButton, nextButton, resetButton;
        private Toggle motionToggle;
        private CircuitBoard board;
        private MissionData[] data;
        private int[] ratings;
        private int active, a, b, attempts;
        private bool usedHint, resetArmed;
        private BinaryGate gate = BinaryGate.OR;
        private OutputStage stage = OutputStage.BUFFER;

        private T Query<T>(string name) where T : VisualElement
        {
            T element = root.Q<T>(name);
            if (element == null) throw new InvalidOperationException("Missing UI element: " + name);
            return element;
        }

        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            try
            {
                if (missions == null || missions.Length == 0) throw new InvalidOperationException("Assign mission assets with Signal Quest > Create Demo Scene.");
                data = new MissionData[missions.Length]; ratings = new int[missions.Length];
                var ids = new HashSet<string>();
                for (int i = 0; i < missions.Length; i++)
                {
                    if (missions[i] == null) throw new InvalidOperationException("A mission reference is missing.");
                    data[i] = missions[i].ToData(); CircuitLogic.Validate(data[i]);
                    if (!ids.Add(data[i].Id)) throw new InvalidOperationException("Mission IDs must be unique.");
                    ratings[i] = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey(i), 0), 0, 3);
                }
                navigation = Query<VisualElement>("missions"); table = Query<VisualElement>("truth-table");
                title = Query<Label>("mission-title"); brief = Query<Label>("brief");
                feedback = Query<Label>("feedback"); progress = Query<Label>("progress"); notes = Query<Label>("notes");
                gateMenu = Query<DropdownField>("gate-choice"); stageMenu = Query<DropdownField>("stage-choice");
                gateMenu.choices = new List<string> { "AND", "OR", "XOR" };
                stageMenu.choices = new List<string> { "BUFFER", "NOT" };
                hintButton = Query<Button>("hint"); nextButton = Query<Button>("next"); resetButton = Query<Button>("reset");
                motionToggle = Query<Toggle>("reduce-motion");
                var workspace = Query<VisualElement>("workspace");
                var leftPanel = Query<VisualElement>("left-panel");
                var rightPanel = Query<VisualElement>("right-panel");
                workspace.RegisterCallback<GeometryChangedEvent>(evt => {
                    bool narrow = evt.newRect.width < 1000f;
                    workspace.style.flexDirection = narrow ? FlexDirection.Column : FlexDirection.Row;
                    leftPanel.style.minWidth = 0; leftPanel.style.marginRight = narrow ? 0 : 16;
                    rightPanel.style.width = narrow ? new StyleLength(Length.Percent(100)) : new StyleLength(310f);
                    rightPanel.style.marginTop = narrow ? 16 : 0;
                });
                motionToggle.SetValueWithoutNotify(PlayerPrefs.GetInt("signalquest.unity.motion", 0) == 1);
                board = new CircuitBoard(() => { a = 1-a; Render(); }, () => { b = 1-b; Render(); });
                Query<VisualElement>("board-host").Add(board);
                gateMenu.RegisterValueChangedCallback(_ => { gate = (BinaryGate)gateMenu.index; Changed(); });
                stageMenu.RegisterValueChangedCallback(_ => { stage = (OutputStage)stageMenu.index; Changed(); });
                Query<Button>("diagnose").clicked += Diagnose;
                hintButton.clicked += () => { usedHint = true; hintButton.SetEnabled(false); Message(data[active].Hint + " This run will earn one star."); };
                nextButton.clicked += () => { if (active+1 < data.Length) Load(active+1); };
                resetButton.clicked += ResetProgress;
                motionToggle.RegisterValueChangedCallback(evt => {
                    PlayerPrefs.SetInt("signalquest.unity.motion", evt.newValue ? 1 : 0); PlayerPrefs.Save(); UpdateProgress();
                });
                if (station != null) station.Bind(Query<VisualElement>("station-window"), root);
                Load(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                root.Clear(); var label = new Label("Signal Quest setup error: " + error.Message);
                label.style.whiteSpace = WhiteSpace.Normal; label.style.color = Color.white; root.Add(label);
            }
        }

        private string SaveKey(int index)
        {
            // A changed specification gets a fresh rating instead of inheriting an old score.
            var mission = data[index];
            return "signalquest.unity.rating." + mission.Id + "." + string.Join("", mission.Target) + "." + mission.Cascade;
        }

        private void Load(int index)
        {
            active = index; a = b = attempts = 0; usedHint = false; gate = BinaryGate.OR; stage = OutputStage.BUFFER;
            resetArmed = false; resetButton.text = "Reset ratings";
            title.text = string.Format("{0:00} / {1:00}  {2}", active+1, data.Length, data[active].Title);
            brief.text = data[active].Brief;
            gateMenu.SetValueWithoutNotify(gate.ToString()); stageMenu.SetValueWithoutNotify(stage.ToString());
            stageMenu.style.display = data[active].Cascade ? DisplayStyle.Flex : DisplayStyle.None;
            hintButton.SetEnabled(true); nextButton.style.display = DisplayStyle.None;
            Render(); UpdateProgress(); Message("Choose gates, inspect A/B, then verify every truth-table row.");
        }

        private void Changed()
        {
            nextButton.style.display = DisplayStyle.None; Render();
            Message("Circuit changed. Run diagnostics to check all four input combinations.");
        }

        private void Render()
        {
            board.Show(gate, stage, data[active].Cascade, a, b);
            var result = CircuitLogic.Diagnose(data[active], gate, stage);
            table.Clear(); AddRow("A", "B", "Target", "Live", "Check", "table-header");
            foreach (var row in result.Rows)
                AddRow(row.A.ToString(), row.B.ToString(), row.Expected.ToString(), row.Actual.ToString(),
                    row.Passed ? "PASS" : "FAIL", row.A == a && row.B == b ? "current-row" : "");
            notes.text = "AND: both inputs. OR: either or both. XOR: inputs differ. NOT: invert. BUFFER: unchanged.";
        }

        private void AddRow(string aText, string bText, string expected, string actual, string status, string css)
        {
            var row = new VisualElement(); row.AddToClassList("truth-row");
            if (!string.IsNullOrEmpty(css)) row.AddToClassList(css);
            foreach (string value in new[] { aText, bText, expected, actual, status })
            {
                var label = new Label(value); label.AddToClassList("truth-cell"); row.Add(label);
            }
            table.Add(row);
        }

        private void Diagnose()
        {
            attempts++;
            var result = CircuitLogic.Diagnose(data[active], gate, stage);
            if (!result.Passed)
            {
                TruthRow failed = result.Rows.Find(row => !row.Passed);
                a = failed.A; b = failed.B; Render();
                Message(string.Format("{0}/4 cases passed. A={1}, B={2}: target {3}, your output {4}. Adjust the circuit.", result.Correct, a, b, failed.Expected, failed.Actual));
                return;
            }
            int stars = usedHint ? 1 : attempts == 1 ? 3 : 2;
            ratings[active] = Math.Max(ratings[active], stars);
            PlayerPrefs.SetInt(SaveKey(active), ratings[active]); PlayerPrefs.Save();
            UpdateProgress(); Message("SYSTEM RESTORED · 4/4 cases passed · " + stars + " stars this run.", true);
            nextButton.style.display = active+1 < data.Length ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateProgress()
        {
            int complete = 0; var states = new bool[ratings.Length]; navigation.Clear();
            for (int i = 0; i < ratings.Length; i++)
            {
                states[i] = ratings[i] > 0; if (states[i]) complete++;
                int selected = i;
                var button = new Button(() => Load(selected)) { text = (i+1) + "  " + data[i].Title + (states[i] ? "  [" + ratings[i] + "/3]" : "") };
                button.AddToClassList("mission-button"); button.EnableInClassList("selected", i == active); navigation.Add(button);
            }
            progress.text = complete == ratings.Length ? "STATION ONLINE · all systems restored" : complete + " / " + ratings.Length + " systems restored";
            if (station != null) station.SetProgress(states, motionToggle.value);
        }

        private void ResetProgress()
        {
            if (!resetArmed) { resetArmed = true; resetButton.text = "Confirm reset"; Message("Press Confirm reset to clear ratings, or change mission to cancel."); return; }
            for (int i = 0; i < data.Length; i++) { PlayerPrefs.DeleteKey(SaveKey(i)); ratings[i] = 0; }
            PlayerPrefs.Save(); Load(0);
        }

        private void Message(string text, bool success = false)
        {
            feedback.text = text; feedback.EnableInClassList("success", success);
        }
    }
}
