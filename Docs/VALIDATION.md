# Signal Quest Validation Record

Status when this documentation was prepared: source import and scene generation observed on macOS with Unity 6.6 version 6000.6.7f1. Latest screenshot shows UIDocument Panel Settings set to None. Full runtime behavior, Editor logic checks and a native build are pending confirmation.

Fill in actual results. An empty result is not a pass. The JavaScript prototype's tests do not establish Unity correctness.

| Check | Expected behavior | Actual result |
| --- | --- | --- |
| Import and compilation | Custom menu and assigned runtime scripts available | Scene generation observed; retain actual Console log |
| Panel Settings | UIDocument references generated PanelSettings asset | Pending confirmation after manual assignment |
| Logic checks | Current Editor harness logs 25 assertions passed | Not supplied |
| Airlock wrong gate | OR fails two rows and selects 01 as first counterexample | Not supplied |
| Four mission solutions | AND, OR, XOR, OR then NOT all pass their targets | Not supplied |
| Live signals | Every A/B pair matches the highlighted truth-table row | Not supplied |
| Station completion | Module lights change after a full diagnostic pass | Not supplied |
| Ratings | First no-hint pass 3; later no-hint pass 2; hinted pass 1 | Not supplied |
| Best score | Lower later score does not overwrite higher best | Not supplied |
| Persistence | Current ratings and motion preference survive restarting Play mode | Not supplied |
| Reset | Second click confirms; mission change cancels | Not supplied |
| Changed specification | A changed target or cascade gets a fresh rating | Not supplied |
| Invalid mission | Bad bits, impossible target or duplicate IDs produce a setup error | Not supplied |
| Motion | Toggle stops station light pulsing | Not supplied |
| Keyboard | Controls usable by keyboard with visible focus | Not supplied |
| Layout | Readable at intended landscape sizes and narrower workspace | Not supplied |
| Native Mac build | Loads and completes the same gameplay checks | Not supplied |
| Playtesting | Real observations recorded and one issue addressed | Not supplied |

For each run record: date, Unity patch, macOS version, chip, Editor or player build, test steps, actual outcome, error text and fix. Add evidence references only when files or recordings actually exist.

Example empty run entry:

```text
Date:
Unity version:
macOS and chip:
Test and steps:
Expected result:
Actual result:
Evidence:
Fix or follow-up:
```

Contribution log:

```text
Change:
Why it was needed:
Files or assets changed:
My contribution:
Assistance used:
Validation performed:
Result and remaining limitation:
```
