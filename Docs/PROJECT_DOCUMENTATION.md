# Signal Quest Project Documentation

This guide explains the current Unity game, its intended educational use, the code and asset structure, setup, verification and extension points. The first practical goal is a verified four-mission playthrough. The game is an early educational prototype whose learning benefits still need evaluation.

## Purpose and users

Signal Quest targets learners beginning Boolean logic and truth tables. An instructor or tutor could use the puzzles to discuss specifications and debugging. The player repairs a station by choosing gates that produce the required behavior for every possible input pair. The setting gives the exercises a progression goal; the technical core is circuit evaluation and diagnostic feedback.

The useful learning task is to connect a verbal requirement, a Boolean function and a complete truth table. A learner should be able to explain why a selected gate works, identify a counterexample to an incorrect choice, and combine two stages for an inverted function. These are intended outcomes, not measured results.

## Current evidence and scope

Screenshots from the Mac setup show Unity 6.6 version 6000.6.7f1, the imported scripts, the custom Signal Quest menu and a generated scene. The source kit originally targeted Unity 6.3 LTS. The latest setup screenshot showed UIDocument Panel Settings set to None. Manual assignment was recommended, but a corrected running screen has not been confirmed.

Scene generation is observed. A full Play mode pass, actual Editor logic-check output, keyboard/accessibility evaluation, learning study and native Mac build remain unverified. The previous JavaScript game's test results must not be presented as Unity test results.

## Prerequisites and first run

Basic concepts are C# variables, arrays, enums, functions, exceptions and events; Unity GameObjects, components, scenes, prefabs and the Inspector; and Boolean gates. The game uses UI Toolkit and Unity's local PlayerPrefs storage. No external AI service, API key or npm server is required.

For the existing project, open the generated scene at Assets/SignalQuest/Generated/SignalQuest.unity. Select Signal Quest UI in Hierarchy and check UIDocument Source Asset and Panel Settings. If Panel Settings is None, use its object-picker circle to choose the generated PanelSettings asset, then save. Open Game view and press Play.

For a fresh setup, create a Universal 2D project, copy the source kit's Assets/SignalQuest folder into Assets, wait for compilation, and select Signal Quest then Create Demo Scene from the Mac's top application menu bar. The setup script preserves existing mission assets but recreates its generated demo scene. Save custom scene layouts separately before regenerating.

## Gameplay and progression

The player reads a mission, chooses Gate 01, toggles A and B to inspect individual cases, then runs diagnostics. All four input pairs must pass. On failure, the game selects the first failing pair in order 00, 01, 10, 11 and states expected and actual outputs. On success, it awards a rating and marks the station module as restored.

The mission light represents completion of the entire specification. It is separate from the live relay value for the currently selected A/B pair. A repaired system can remain marked complete while its live output is 0 for a valid input case. Mission navigation permits replay; missions are not locked into a mandatory sequence.

| Mission | Signal meaning and requirement | Function |
| --- | --- | --- |
| Airlock | A and B are door-lock sensors; enable only when both are active | A AND B |
| Beacon | A and B are distress switches; either or both should trigger the relay | A OR B |
| Router | A and B request routes; enable for exactly one request | A XOR B |
| Shield | A and B are alarms; safe-to-open relay is active only when neither is active | NOT of A OR B |

## Boolean specification

Inputs and outputs are numeric bits. The simulator rejects values outside 0 and 1. The first stage offers AND, OR and XOR. The optional second stage is BUFFER or NOT. BUFFER preserves its single input; NOT flips it.

| A | B | Airlock AND | Beacon OR | Router XOR | Shield NOR |
| --- | --- | --- | --- | --- | --- |
| 0 | 0 | 0 | 0 | 0 | 1 |
| 0 | 1 | 0 | 1 | 1 | 0 |
| 1 | 0 | 0 | 1 | 1 | 0 |
| 1 | 1 | 1 | 1 | 0 | 0 |

The airlock illustrates the difference between inspecting one case and proving the whole specification. OR and AND both produce 0 for 00 and 1 for 11, but OR fails the airlock requirement for 01 and 10. Exhaustive diagnostics expose those counterexamples.

## Code and asset architecture

CircuitLogic is pure C# and has no UnityEngine dependency. MissionDefinition stores editable specifications in ScriptableObject assets. SignalQuestGame coordinates data, user events, UI rendering and saved ratings. CircuitBoard draws the current circuit with UI Toolkit vector paths and native buttons. StationView and StationModule display completed-system feedback through prefab instances.

| Source file | Responsibility |
| --- | --- |
| CircuitLogic.cs | Enums, MissionData, TruthRow, DiagnosticResult, gates, evaluation, diagnostics, shape and solvability validation |
| MissionDefinition.cs | Serialized Inspector fields and copying between an asset and runtime data |
| SignalQuestGame.cs | Startup validation, mission state, callbacks, scoring, progress, reset and PlayerPrefs |
| CircuitBoard.cs | A/B buttons, intermediate/output labels, node placement and Painter2D wires |
| StationModule.cs | Sets a module's status-light color and pulsing alpha |
| StationView.cs | Fits the station camera to its UI window and updates module feedback in LateUpdate |
| CreateDemoScene.cs | Generates assets, prefab instances, camera, UI references and demo scene |
| LogicChecks.cs | Menu-driven checks that call the actual C# logic |
| UXML USS and TSS files | UI hierarchy, styling and default runtime theme |

The scene contains Station Camera, Relay Station and Signal Quest UI. Generated assets include four mission assets, PanelSettings.asset, StationUnlit.mat, StationPixel.png, StationModule.prefab and SignalQuest.unity. Unity creates the associated .meta files on import; preserve them in Git.

## Runtime data flow

1. Start reads the assigned mission assets, copies them into MissionData, validates them and rejects duplicate IDs.
2. The game retrieves each rating and the reduced-motion preference from PlayerPrefs, queries named UI elements and connects callbacks.
3. Changing a gate or toggling an input recalculates the circuit and redraws the truth table and vector wires.
4. Run diagnostics evaluates all four input pairs and compares each output with the mission target.
5. A failed check displays a counterexample. A successful check records the best rating, updates navigation and updates completion state.
6. StationView consumes completion state and updates scene lights; it does not calculate logic correctness.

This separation lets the gate engine be checked without creating a camera, scene or UI document. Drawing code displays computed values rather than determining circuit behavior.

## Mission data and validation

| Field | Meaning |
| --- | --- |
| id | Identifier used in uniqueness checks and save keys |
| title | Mission title shown in the UI |
| brief | Verbal description of the required behavior |
| hint | Clue revealed on request |
| target | Exactly four bits, ordered 00, 01, 10, 11 |
| cascade | Whether to apply BUFFER or NOT after the first gate |

Shape validation requires nonempty text fields and four valid bits. Solvability validation enumerates the allowed configurations. A single stage has three choices; a cascade has six. At least one configuration must match every target row. Startup also checks that assigned IDs are unique. Validation cannot determine whether the briefing accurately describes the target; the author must check that semantic relationship.

CountSolutions uses at most 6 configurations times 4 input pairs, or 24 evaluations, for the current two-stage model. This is an algorithmic count, not a measured runtime benchmark. It does not describe the cost of a future arbitrary circuit editor.

## Ratings and persistence

| Successful run | Rating |
| --- | --- |
| First diagnostic attempt with no hint | Three stars |
| Later diagnostic attempt with no hint | Two stars |
| Any successful run after revealing a hint | One star |

An unsuccessful diagnostic call increments the attempt count. Revisiting a mission resets attempts, inputs and hint state. The best recorded score is retained using the maximum of the old and new ratings. Reset ratings requires a second click; changing mission cancels that pending reset.

The save key combines a Unity-specific prefix, mission ID, target bits and cascade mode. A changed target or cascade mode therefore gets a fresh rating. Changing only text does not invalidate it. The reduced-motion preference uses a separate key. Reset deletes current mission rating keys and leaves the motion preference and obsolete keys for earlier specifications untouched. PlayerPrefs is local convenience storage and does not provide secure or synchronized scoring.

## Interface and visual feedback

UXML defines the page and named controls, USS supplies the layout and colors, and TSS imports Unity's runtime theme. C# uses named queries to connect the UI to behavior. Dropdowns select gates, native buttons toggle inputs and run checks, and text communicates signal values and pass/fail results alongside color.

The workspace switches between columns and a stacked layout according to available width. StationView fits the camera viewport to the station window. The reduced-motion toggle stops the status-light pulsing; it is not a general operating-system accessibility integration. Keyboard focus and layout must still be tested in the actual Editor and player build.

## Editing and extending the game

Outside Play mode, select a generated mission asset and revise its fields. To add a mission, duplicate an asset, assign a unique ID, choose a solvable target and add it to the Signal Quest UI object's Missions array. Add a matching StationModule prefab instance and StationView reference if you want another physical light. Update the hard-coded station caption and generation script if the expanded layout should be recreated automatically.

Adding a new gate requires changing the enum, gate evaluation, UI choices, validation enumeration and independent truth-table checks together. A runtime authoring tool, drag-and-connect wiring and circuit cycle detection are future work; they are not implemented in this Unity version.

## Verification and known issues

LogicChecks.Run is an Editor menu harness, not a Unity Test Runner or NUnit suite. Its current source contains 25 assertions covering independent gate tables, output stages, built-in solvability, an incorrect airlock configuration, a cascade case and invalid inputs. Successful execution should log that count, but no actual execution result has been supplied. It does not verify input events, graphics, save persistence, arbitrary Inspector configurations or player builds.

The known setup issue is an unassigned UIDocument Panel Settings reference in the latest Unity 6.6 screenshot. Assign the generated asset and verify Game view before recording a demonstration. A Panel Renderer migration suggestion may also appear in newer Unity versions; the immediate task is to make the currently used UIDocument configuration work. Record any migration as a separate change with a new playtest.

Use the accompanying validation checklist for gameplay, Inspector edits, saved state, motion, resizing, keyboard operation and a native Mac build. Record actual pass/fail results and the exact Editor version. Keep intended behavior separate from verified behavior.

## Limitations and next milestones

The game uses Boolean values and fixed wiring. It has no analog voltage/current model, transistor simulation, propagation delay, sequential logic, arbitrary wiring, runtime JSON workshop, multiplayer or AI tutor. It is not suitable for validating a real safety system. The station is a fictional teaching context.

Finish a stable four-mission run, then choose one meaningful improvement. A useful educational evaluation could examine whether counterexample feedback helps beginners debug a circuit. Define the comparison and measures before collecting data, and report results only after a real study. A polished level-authoring workflow or a carefully scoped wiring mechanic would be alternative engineering milestones.

## References and source map

Implementation claims are grounded in the supplied Assets/SignalQuest source kit. Unity concepts can be checked in its official ScriptableObject manual, prefab manual and PlayerPrefs reference. These references explain the platform; they do not establish that this particular game has passed validation.

https://docs.unity3d.com/6000.0/Documentation/Manual/class-ScriptableObject.html

https://docs.unity3d.com/6000.0/Documentation/Manual/Prefabs.html

https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.html
