# Damnation

> **Title:** Damnation  
> **Genre:** Hidden-information tactical strategy / board-game battle royale  
> **Platform:** Mobile  
> **Planned engine:** Unity 2D  
> **Development cadence:** Small-scope Sunday project

## Overview

Damnation is a four-team tactical strategy game played across a network of **64 buildings**.

The player controls one team of **six persistent units** against three AI-controlled teams. Every team chooses an initial landing building, then secretly moves units between adjacent buildings. Enemy landing locations are visible, but later movement remains hidden unless detected through Intelligence.

The game combines:

- hidden movement;
- territorial expansion;
- squad splitting and regrouping;
- simultaneous turn resolution;
- persistent unit damage;
- short turn-based combat encounters;
- permanent unit progression;
- a shrinking, increasingly lethal safe zone.

The strategic heart of the game is a constant trade-off:

> **Spread to claim territory, group to survive combat, and rotate before the zone destroys you.**

## Core Match Structure

- Four teams participate in every match:
  - one player;
  - three AI teams.
- Each team begins with six units.
- The board contains 64 connected building nodes.
- Buildings are mechanically identical.
- Every team chooses one starting building and deploys all six units there.
- All starting buildings are revealed before movement begins.
- Later enemy movement is hidden unless detected.
- Units may move only to adjacent buildings.
- Units on the same building are treated as a squad for convenience, but may still receive separate commands.
- Orders are chosen during a planning phase and resolved simultaneously.

## Territory

A team claims a building when one or more of its units enter it uncontested or eliminate all enemies occupying it.

- A claimed building remains owned after the team leaves it.
- Ownership changes when another team successfully occupies it.
- Territory is scored only when:
  - the player is defeated; or
  - the player wins the match.
- Each owned building is worth one territory point.

Territory rewards splitting and map coverage, while combat rewards concentration.

## Combat

Combat begins when units from different teams occupy the same building after movement resolution.

Each combat round:

1. Every surviving unit receives a target.
2. Player units attack the selected targets.
3. AI units select targets using simple combat logic.
4. All attacks resolve simultaneously.
5. Defeated units are removed from the match.
6. The player may continue fighting or retreat.

Rules:

- Retreat is unavailable before the first combat round resolves.
- Retreat returns surviving units to their previous building.
- Retreat is allowed only if the destination does not contain enemy units.
- Combat ends when:
  - one side is eliminated;
  - the player squad is eliminated;
  - the player retreats;
  - only one team remains in a multi-team encounter.
- Damage persists throughout the match.
- Units enter later battles with their remaining Resistance.

## Unit Stats

Every unit has three stats.

### Resistance

Resistance is the unit's health.

- Damage reduces current Resistance.
- A unit is defeated when Resistance reaches zero.
- Damage persists throughout the match.
- A unit can regenerate only by remaining safely in the same building for multiple turns.

### Strength

Strength determines damage dealt by the unit during combat.

### Intelligence

Intelligence allows a unit to detect nearby enemies.

- Each Intelligence point gives a **5% detection chance**.
- Detection checks apply to enemy units in adjacent buildings.
- Successful detection reveals a signal rather than full information.
- Signals may indicate:
  - movement detected;
  - small force;
  - squad detected;
  - large force.
- Exact enemy stats and health remain hidden.

## Regeneration

Regeneration rewards stopping and recovering instead of rotating continuously.

Current rule:

- first consecutive idle turn: no healing;
- second consecutive idle turn: healing begins;
- each later uninterrupted idle turn restores health;
- moving, retreating, or entering combat resets the rest counter;
- healing cannot exceed maximum Resistance.

The exact amount restored per turn will be balanced during development.

## Shrinking Zone

The safe zone is the main anti-stalling and match-compression system.

- The full board begins with 64 buildings.
- The first major phase shrinks the playable safe area to approximately 32 buildings.
- Later phases continue reducing the safe region.
- Each phase selects and reveals a focus building.
- The zone is calculated through graph distance from that focus building.
- Buildings outside the current safe zone deal damage to every unit occupying them.
- Zone damage increases with each phase.
- The next focus must remain inside, or substantially overlap, the current safe region.
- Zone damage is applied after movement and combat resolution.

Approximate progression:

| Stage | Approximate Safe Buildings |
|---|---:|
| Initial board | 64 |
| Phase 1 | 32 |
| Phase 2 | 18–20 |
| Phase 3 | 10–12 |
| Phase 4 | 5–6 |
| Final zone | 1–3 |

The exact timing and damage values remain balancing variables.

## Match Results

Placement rewards:

| Placement | Placement Points |
|---|---:|
| 1st | 5 |
| 2nd | 2 |
| 3rd | 1 |
| 4th | 0 |

When the player is defeated:

- territory is scored using the board state at that moment;
- the player's placement is locked;
- surviving AI teams are ranked by surviving unit count;
- equal surviving-unit counts are resolved randomly;
- the match ends immediately.

When the player wins:

- final territory ownership is scored;
- first-place points are awarded;
- XP is calculated.

## Progression

The player gains XP after every match.

- Wins grant significantly more XP than losses.
- XP increases the player's account level.
- Each level grants one training point.
- One training point upgrades one stat on one specific unit.
- Each unit may receive a maximum of 20 total training points.
- The player decides how to distribute those points between Resistance, Strength, and Intelligence.

This allows units to specialize as:

- frontline fighters;
- durable defenders;
- scouts;
- balanced all-rounders;
- territorial runners;
- ambushers.

Enemy teams use the same stat system. Campaign difficulty increases by giving enemy units larger training budgets and stronger distributions, not by changing the core rules.

## AI Philosophy

The AI should be simple, readable, and competitive without cheating.

AI teams should:

- obey the same visibility rules as the player;
- capture empty buildings;
- group when threatened;
- split when territory is valuable;
- rotate toward the safe zone;
- avoid clearly stronger enemies;
- attack weaker visible forces;
- preserve damaged units when reasonable;
- pursue placement and victory.

The AI should not have access to hidden player positions unless those positions are detected legitimately.

## Mobile Interaction Goals

The game should remain comfortable to control with a small number of taps.

Planned interaction flow:

1. Tap a building containing friendly units.
2. All units on that building are selected by default.
3. Tap unit portraits to include or exclude individual units.
4. Tap an adjacent building to assign the movement order.
5. Confirm the turn when all desired orders are prepared.

The interface should make grouped movement fast while preserving individual control.

## Design Pillars

1. **Information matters**  
   Enemy movement is hidden, so prediction and Intelligence create meaningful uncertainty.

2. **Every formation has a cost**  
   Grouping protects units but sacrifices territory. Splitting captures ground but creates vulnerable targets.

3. **The zone authors the match**  
   Every zone forces new routes, confrontations, sacrifices, and rotations.

4. **Combat is quick**  
   Battles support the board strategy instead of replacing it.

5. **Progression creates identity**  
   The six units become a personalized roster rather than disposable pieces.

6. **The scope stays controlled**  
   Buildings share one ruleset, AI uses understandable heuristics, and the campaign reuses the same core systems.

## Current Status

The project is currently in pre-production and rules design.

The next step is to build the smallest playable version:

- a connected board;
- six controllable units;
- movement between adjacent buildings;
- ownership;
- one AI opponent;
- simultaneous turn resolution.

## Inspiration

The project is inspired by the macro-strategy of battle royale esports:

- landing decisions;
- map control;
- rotations;
- scouting;
- team splits;
- gatekeeping;
- zone pressure;
- placement value;
- compressed endgames.

The goal is not to reproduce a shooter. The goal is to translate those strategic pressures into a compact mobile board game.
