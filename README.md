# Signal Quest

A Unity circuit puzzle prototype that teaches Boolean reasoning through a space-station repair scenario. Players select logic gates, inspect signal values, and repair systems by satisfying every row of a truth table.

**Stack:** Unity · C# · UI Toolkit · ScriptableObjects · Prefabs · PlayerPrefs

## Project status

The prototype contains four core missions and an Editor tool that generates its assets and demo scene. Source import and scene generation have been observed on **Unity 6.6, 6000.6.7f1, on macOS**. A complete gameplay pass, successful execution of the C# logic checks, and a standalone build have **not yet been confirmed**.

The source kit was initially written for Unity 6.3 LTS. Compatibility with a particular Editor patch must be checked locally. The most recent setup screenshot showed an unassigned UIDocument Panel Settings reference; see [troubleshooting](#troubleshooting).

## Purpose

Signal Quest connects a Boolean specification to an interactive puzzle. A learner can select a gate, inspect individual input cases, and receive a concrete counterexample when a circuit fails. The station provides a progression goal. Educational effectiveness remains a question for future playtesting; it has not been measured.

## Features in the source

- Four missions using AND, OR, XOR, and OR followed by NOT.
- A deterministic C# Boolean simulator separated from Unity presentation.
- Live truth tables and exhaustive checks of all four two-input combinations.
- Failed-case feedback that selects the first failing input pair on the board.
- Inspector-editable ScriptableObject mission definitions, including solvability checks.
- A reusable station module prefab with independent instance status lights.
- UI Toolkit controls, vector circuit wires, hints, best ratings, and local saves.
- Reduced station pulsing, explicit signal labels, and native focusable controls.
- Editor scene-generation and logic-check menus.

## Missions

Targets are ordered by input pairs **00, 01, 10, 11**.

| Mission | Required behavior | Target | Solution |
| --- | --- | --- | --- |
| Airlock interlock | Enable only when both door-lock sensors are active | 0001 | AND |
| Distress beacon | Enable when either or both distress switches are active | 0111 | OR |
| Power route selector | Enable when exactly one route is requested | 0110 | XOR |
| Radiation shield | Enable safe-to-open relay only when neither alarm is active | 1000 | OR → NOT |

## Open and run

### In a complete Unity project

1. Open the project through Unity Hub using its recorded Editor version in `ProjectSettings/ProjectVersion.txt`.
2. Allow asset import and compilation to finish; resolve Console errors first.
3. Open `Assets/SignalQuest/Generated/SignalQuest.unity`. If it has not been generated, select **Signal Quest → Create Demo Scene** from the Editor menu.
4. Confirm the Signal Quest UI object's UIDocument has both its Source Asset and Panel Settings assigned.
5. Open **Game** view, use a landscape resolution such as 1280 × 900, and press **Play**.
6. For the first mission, select AND and run diagnostics.

### If starting from the Assets source kit

Create a new Universal 2D project, copy the kit's `Assets/SignalQuest` folder into the project's `Assets` folder, wait for compilation, and generate the demo scene through the menu above. An Assets-only kit is not a complete Hub project: commit the actual project's `Assets`, `Packages`, and `ProjectSettings` after setup.

No npm command or web API key is used by this Unity version. Initial Unity/template installation may require downloads.

## Controls and scoring

Choose Gate 01 with its dropdown. The shield also exposes Gate 02. Click A/B to toggle inputs; Run diagnostics checks every input pair. Reveal hint supplies a clue. Next mission becomes available after a successful check, and mission navigation allows replay.

First-attempt success without a hint earns three stars; later success without a hint earns two; success after a hint earns one. The best rating is retained. Revisiting a mission begins a fresh attempt sequence. Reset ratings requires a second confirmation click; changing mission cancels the pending reset.

## Architecture

| Component | Role |
| --- | --- |
| `CircuitLogic.cs` | Pure C# gates, evaluation, diagnostics, mission validation and default data |
| `MissionDefinition.cs` | ScriptableObject asset containing a mission specification |
| `SignalQuestGame.cs` | Runtime state, events, feedback, ratings, saving and progress |
| `CircuitBoard.cs` | UI Toolkit input buttons, labels and vector wires |
| `StationModule.cs` | Prefab instance status-light behavior |
| `StationView.cs` | Camera viewport, completion feedback and pulsing |
| `CreateDemoScene.cs` | Editor asset and scene generation |
| `LogicChecks.cs` | Editor checks that call the actual C# simulator |
| `SignalQuestUI.uxml` / `.uss` / `.tss` | UI structure, styling and runtime theme |

Source files live under `Assets/SignalQuest`. See [full documentation](Docs/PROJECT_DOCUMENTATION.md) for data flow, saving, extension steps and limitations.

## Verify

Outside Play mode, select **Signal Quest → Run Logic Checks**. With the current source, successful execution is expected to log **25 logic assertions passed**. This is an expected result, not a recorded result. The menu is a custom check harness, not an NUnit or Unity Test Runner suite.

These checks do not verify rendering, input delivery, save persistence or builds. Follow [the validation checklist](Docs/VALIDATION.md) and record actual results before claiming support. The earlier JavaScript prototype's tests are separate and do not establish C# correctness.

## Troubleshooting

| Symptom | Action |
| --- | --- |
| Signal Quest menu missing | Check Console compilation errors and the imported folder location |
| UIDocument Panel Settings is None | Select Signal Quest UI in Hierarchy; use the circle beside Panel Settings to assign `Assets/SignalQuest/Generated/PanelSettings.asset`; save the scene |
| No PanelSettings asset exists | Inspect Console errors from scene generation; recreate the scene in a clean project after resolving them |
| Blank Game view | Check the scene, camera, UIDocument Source Asset and Panel Settings, then inspect Console errors |
| Pink station geometry | Inspect the generated material/shader and the project's render pipeline |
| Ratings change after editing a target | Intended: the save key includes target and cascade mode |

## Scope and limitations

This is a fixed-topology Boolean puzzle, not an analog, transistor, timing or safety-system simulator. It has no arbitrary wiring, runtime level editor, JSON workshop, multiplayer, AI tutor or cloud account system. Missions are authored in the Unity Inspector. The earlier browser prototype's runtime JSON workshop is not part of this Unity implementation.

Accessibility features require evaluation; no accessibility conformance or learning improvement is claimed. PlayerPrefs ratings are local convenience data, not secure records. The generated station and its captions currently represent four core systems; adding more missions also requires deliberate visual updates.

## Repository and development notes

Keep `Assets` and its `.meta` files, `Packages`, `ProjectSettings`, README and documentation in version control. Exclude generated caches/builds using the Unity `.gitignore`. Add a real gameplay screenshot and a demo link after recording a verified run. Native builds and video demonstrations are separate deliverables; no live demo is currently linked here.

The initial implementation was generated with AI assistance. Document subsequent design, debugging, testing and feature contributions accurately. This is an independent project with no faculty supervision or university appointment implied. Choose and add a license deliberately before describing the repository as licensed for reuse.

## Documentation

- [Project documentation](Docs/PROJECT_DOCUMENTATION.md)
- [Validation checklist](Docs/VALIDATION.md)

## References

- [Unity ScriptableObject documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/class-ScriptableObject.html)
- [Unity prefab documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/Prefabs.html)
- [Unity PlayerPrefs reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.html)
