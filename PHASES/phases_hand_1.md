# Damnation — Development Phases Hand 1

> **Coverage:** Phases 1–5  
> **Status:** APPROVED  
> **Platform direction:** Mobile, Android-first, landscape  
> **Engine direction:** Unity 2D

This file is the authoritative production plan for the first five development phases of Damnation. It defines what each phase must accomplish, what it must not expand into, and the conditions required before the phase may be locked and committed.

Later phases are intentionally not defined here. Phases 6–10 will be planned in `phases_hand_2.md` after the first five phases are complete or close enough that their results can guide the next batch.

## Working Rule

Every phase follows the same process:

1. The developer describes the intended implementation approach.
2. The approach is reviewed and corrected where necessary.
3. The developer approves the final approach.
4. The phase is implemented.
5. The implementation is reviewed against its exit conditions.
6. Required corrections are completed.
7. Relevant documentation is updated.
8. The phase is locked with one focused Git commit.

## Pre-Phase Setup

This setup is required before Phase 1 but is not counted as one of the five gameplay phases.

### Goal

Create the clean mobile Unity project and repository in which the first phase hand will be developed.

### Required Work

- Create the Unity 2D project.
- Set Android as the first mobile build target.
- Set landscape as the initial screen orientation.
- Configure an input foundation that supports touch on mobile and mouse equivalents in the Unity editor.
- Create the initial scene and project folder structure.
- Create the Git repository and Unity `.gitignore`.
- Add the current project documentation.
- Make the initial project-foundation commit.

### Exit Conditions

- The project opens without errors.
- A clean Android development build can be produced.
- Generated Unity files are excluded from Git.
- The initial scene and repository structure are present.
- The project foundation is committed.

---

# Phase 1 — Battlefield Foundation

## Goal

Create the battlefield model, its prototype visual representation, and the mobile camera used to inspect and interact with it.

This phase establishes both the board backend and its temporary presentation. The visual tiles must not become the authoritative source of board state.

## Required Work

### Board Model

- Create an 8×8 battlefield containing exactly 64 building-node tiles.
- Give every building a stable unique ID.
- Store or derive each building's row and column.
- Establish orthogonal adjacency between neighbouring buildings.
- Ensure edge and corner buildings have the correct neighbour sets.
- Provide a way to retrieve a building by ID or grid position.
- Validate that adjacency is legal and reciprocal.

### Prototype Presentation

- Represent every building with a temporary square top-down tile.
- Keep the entire board readable at a full-board camera view.
- Give tiles enough visual states to support later selection, deployment, movement, and zone overlays.
- Keep prototype presentation replaceable; this is not the final visual-identity pass.

### Mobile Camera and Input

- Use an orthographic top-down camera.
- Support one-finger drag for camera movement.
- Support pinch gestures for zooming in and out.
- Provide mouse equivalents for editor testing.
- Add minimum and maximum zoom limits.
- Clamp camera movement around the battlefield.
- Distinguish a deliberate tile tap from a camera drag through an input movement threshold.
- Prevent touches over UI from also interacting with the battlefield.
- Test the board at the target landscape mobile resolution and on an Android development build.

## Out of Scope

- Final building art.
- Building ownership.
- Unit spawning or movement.
- Deployment-line disabling.
- Zone calculations.
- Final UI styling.

## Exit Conditions

- All 64 buildings are visible and correctly arranged.
- Every building has a stable identity, grid position, and correct orthogonal neighbours.
- Invalid or non-reciprocal adjacency can be detected.
- The player can tap a tile without accidental selection after a camera drag.
- Camera pan, pinch zoom, zoom limits, and board clamping work on mobile.
- The board remains readable at supported landscape aspect ratios.

---

# Phase 2 — Deployment Selection

## Goal

Implement the initial player drop-selection rules: two disabled deployment lines and the selection of one valid starting building.

The battlefield contains 16 deployment lines: eight rows and eight columns. Two line selections are rolled independently at the start of deployment, and both rolls may select the same line.

## Required Work

### Deployment-Line Logic

- Represent all eight row lines and eight column lines.
- Perform exactly two independent random line selections for disabling.
- Allow both selections to resolve to the same row or column.
- Treat a building as unavailable for deployment when it belongs to either disabled line.
- Keep the disabled state limited to deployment; the affected buildings remain normal playable buildings once the match begins.
- Allow the deployment state to be regenerated for repeated testing.

### Deployment Presentation

- Clearly show both disabled lines.
- Clearly distinguish valid and invalid starting buildings.
- Allow the player to tap a valid building to select it.
- Visually show the currently selected building.
- Allow the selection to be changed before confirmation.
- Prevent confirmation when no valid building is selected.

### Confirmation and Stored State

- Confirm one valid player starting building.
- Store the confirmed building through its board identity rather than only through its visual object.
- Clear or deactivate deployment-only overlays when deployment ends.
- Do not impose a uniqueness rule that would prevent multiple teams from choosing the same starting building in later phases.

## Preserved Future Rule

If two, three, or four teams eventually choose the same starting building, a hot-drop battle will begin before Turn 1. Phase 2 must preserve this possibility, but it does not implement the battle.

## Out of Scope

- Player unit spawning.
- AI starting-building selection.
- Revealing all four confirmed team landings.
- Hot-drop detection and combat.
- Normal movement turns.

## Exit Conditions

- Every deployment performs exactly two valid line selections from the set of 16, with duplicate results allowed.
- Buildings belonging to either disabled line cannot be selected for deployment.
- Valid buildings remain selectable.
- The player can select, change, and confirm one valid starting building.
- The confirmed building remains available to the next system through stable board data.
- Deployment restrictions disappear without disabling those buildings during normal play.

---

# Phase 3 — Unit Foundation and Movement Orders

## Goal

Create the six player units, separate their runtime data from their visual representation, and implement the first player-only simultaneous movement turn.

## Required Work

### Unit Runtime Data

Create six distinct player units. Each unit must track at least:

- unit identity;
- team ownership;
- current building;
- previous building;
- planned destination;
- current Resistance;
- maximum Resistance;
- Strength;
- Intelligence;
- alive or eliminated state.

Unit data must support controlled operations for:

- receiving damage;
- healing without exceeding maximum Resistance;
- changing supported stats without creating invalid values;
- assigning, replacing, and clearing movement orders;
- updating current and previous buildings during resolution.

Permanent roster progression and saved upgrades are not part of this phase.

### Unit Presentation

- Create a replaceable visual prefab or view for player units.
- Spawn all six player units at the starting building confirmed in Phase 2.
- Visually communicate the number of friendly units occupying a building.
- Visually communicate which units or group are currently selected.
- Keep the visual representation synchronized with runtime unit data without making the visual prefab the authoritative data source.

### Mobile Selection and Order Assignment

- Tap a friendly occupied building to begin unit selection.
- Select all friendly units on that building by default.
- Allow individual units to be included or excluded from the selected group.
- Highlight legal orthogonally adjacent destinations.
- Assign the selected units a planned destination by tapping a legal neighbouring building.
- Show the planned order visually.
- Allow orders to be changed or cancelled before confirmation.
- Make units without movement orders remain in place.
- Treat destination selection as an order; do not immediately change the unit's actual building.
- Avoid literal unit dragging because it conflicts with mobile camera dragging and grouped-unit control.

### Minimal Turn Spine

- Create the initial central match-flow controller.
- Support a planning state.
- Support order confirmation.
- Lock command input during resolution.
- Resolve all confirmed player movement orders together.
- Merge friendly units that arrive at the same building.
- Update previous-building and current-building state correctly.
- Clear resolved movement orders.
- Advance the turn counter and return to planning.

This controller is the foundation into which AI movement, detection, encounters, combat, capture, regeneration, and zone damage will later be inserted in the correct order.

## Out of Scope

- AI teams and AI orders.
- Hidden enemy information.
- Building ownership and capture.
- Combat and hot-drops.
- Retreat.
- Regeneration.
- Permanent training or save data.

## Exit Conditions

- Six distinct player units spawn at the confirmed starting building.
- Their location and canonical stats—Resistance, Strength, and Intelligence—are tracked correctly.
- All six units can be ordered together.
- Any valid subset can receive a separate adjacent movement order.
- Illegal destinations cannot receive orders.
- Orders may be edited or cancelled before confirmation.
- Unit locations do not change during planning.
- Confirmed movement resolves simultaneously and returns the match to planning for the next turn.

---

# Phase 4 — Zone System Backend

## Goal

Implement the complete backend state and progression logic for the shrinking damaging zone without depending on final battlefield visuals or UI.

The zone is turn-driven. Any zone countdown represents turns, not real-time seconds, and the system must not progress independently through `Update()`.

## Required Work

### Zone State

Track at least:

- current zone phase;
- current focus building;
- future focus building when available;
- current safe-building set;
- future safe-building set;
- turns until the next shrink step;
- current shrink progress;
- damage value for the current phase;
- whether the zone has reached its final state.

### Safe-Region Calculation

- Select a valid random focus building.
- Calculate which buildings belong to the safe region using the zone algorithm approved during the Phase 4 implementation review.
- Support the initial full-board state.
- Support the first reduction to approximately 32 safe buildings.
- Support later reductions toward the final 1–3 safe buildings.
- Select later focus buildings according to the approved containment, overlap, connectivity, and reachability rules.
- Reject and reroll invalid focus results.

### Turn-Driven Progression

- Receive an explicit completed-turn signal from the central match-flow controller.
- Advance warning and shrinking countdowns only through turn progression.
- Support multi-step shrinking where required by the approved phase timing.
- Expose enough state for debugging before the visual phase exists.
- Keep zone progression independent from frame rate and elapsed real time.

### Zone Damage

- Identify units occupying buildings outside the current safe region.
- Apply the current phase's zone damage through the unit runtime data created in Phase 3.
- Apply zone damage at the end-of-turn zone step exposed by the match-flow controller.
- Eliminate a unit when its Resistance reaches zero.
- Keep the damage operation callable from the central match sequence rather than letting the zone choose an arbitrary execution moment.

### Final-Zone Logic

- Detect when the zone reaches its final safe-region target.
- Stop invalid additional shrinking.
- Maintain a stable final-zone state.

This phase does not determine full match victory, placement, territory results, or campaign rewards.

## Out of Scope

- Final zone visuals and HUD.
- Combat-order integration beyond preserving the future resolution slot.
- AI movement toward safety.
- Placement and victory calculation.
- Territory scoring.
- Final balancing values.

## Exit Conditions

- The system can generate a valid initial focus and safe region.
- The zone advances only when a turn-completion command is received.
- Warning, shrinking, refocusing, and later phases progress without invalid safe regions.
- The system can reach and hold a valid final region of approximately 1–3 buildings.
- Units outside the current safe region receive the correct configured damage.
- Units inside the current safe region receive no zone damage.
- Zone logic operates correctly without requiring finished visual presentation.

---

# Phase 5 — Zone System Presentation

## Goal

Connect the completed Zone System backend to the battlefield and mobile UI so the player can understand the current danger and upcoming compression at a glance.

## Required Work

### Battlefield Presentation

- Distinguish current safe buildings from unsafe buildings.
- Preview the future safe region when it has been revealed.
- Distinguish buildings that will become unsafe during the next shrink step.
- Show the current focus building.
- Show the future focus building when it has been revealed.
- Preserve tile readability alongside selection and movement-order states.

### Zone HUD

- Show the current zone phase.
- Show turns remaining before the next shrink or shrink step.
- Show the current zone damage.
- Clearly communicate warning, shrinking, and final-zone states.
- Keep text and controls readable across supported landscape aspect ratios and inside mobile safe areas.

### Damage Feedback

- Clearly communicate when a unit receives zone damage.
- Update displayed Resistance after damage.
- Clearly communicate when zone damage eliminates a unit.
- Avoid presentation timing that unnecessarily delays the next turn.

### Architecture Boundary

- Keep zone calculations and authoritative state inside the Zone System backend.
- Make the battlefield and HUD observe or receive zone-state changes.
- Do not make the backend responsible for directly recolouring tiles, controlling animations, or writing UI text.

## Out of Scope

- Final art direction.
- Final visual effects.
- Audio and music.
- Full accessibility pass.
- Final animation polish.
- AI zone behaviour.
- Full match results.

## Exit Conditions

- The current and future safe regions are immediately distinguishable.
- The player can identify which buildings are becoming unsafe next.
- Focus buildings, phase number, turn countdown, and damage are correctly displayed.
- Visual state remains synchronized with the Zone System throughout every phase.
- Selection, movement orders, and zone overlays remain readable together.
- Zone damage and zone-caused elimination are clearly communicated.
- The complete zone flow is understandable and usable on an Android landscape build.

---

# Phase Hand 1 Completion Condition

`phases_hand_1.md` is complete when all five phases are individually reviewed, locked, and committed.

At that point, the project must contain:

- a functional 64-building mobile battlefield;
- mobile camera and touch controls;
- deployment-line disabling and player start selection;
- six player units with runtime stats;
- grouped and individual movement orders;
- a minimal simultaneous turn loop;
- a complete turn-driven shrinking-zone backend;
- readable zone presentation and UI.

Only then will Phases 6–10 be organized in `phases_hand_2.md`.
