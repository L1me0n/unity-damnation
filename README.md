# Damnation

> **Genre:** Hidden-information tactical strategy / board-game battle royale  
> **Platform:** Android-first mobile  
> **Engine:** Unity 2D  
> **Orientation:** Landscape  
> **Development cadence:** Small-scope Sunday project  
> **Status:** Active production; the next development hand will focus on combat encounters

## Overview

Damnation is a four-nation tactical strategy game played across a fixed **8×8 battlefield of 64 building nodes**.

Each nation fields a persistent team of **six units**. The player controls the team of a **Classical Republic** against three AI-controlled nations. A match represents one year in which the four nations collide. The winner earns progression, while every losing nation must sacrifice a member of its roster.

Within a match, teams deploy from visible starting buildings and then issue hidden movement orders between adjacent nodes. Units spread out to claim territory, regroup for combat, recover while stationary, and rotate away from a shrinking lethal zone. When hostile units meet, combat becomes a weapon-specific turn-based encounter governed by Agility, targeting, prediction, armor, and retreat.

The strategic heart of the game is:

> **Spread to claim territory, group to survive combat, and rotate before the zone destroys you.**

The long-term heart is:

> **Build a nation of remembered warriors, knowing that every lost year may demand one of them as a sacrifice.**

## Core Match Structure

- Four nations participate in every match:
  - one player-controlled Classical Republic;
  - three AI-controlled nations.
- Each nation fields six persistent units.
- The battlefield contains 64 mechanically identical buildings arranged in an 8×8 grid.
- Buildings connect orthogonally; units move only up, down, left, or right to adjacent buildings.
- Co-located friendly units are treated as a squad for convenience but remain separately commandable.
- Movement orders are selected during planning and resolved together through the central turn flow.
- A match represents one year in the persistent nation layer.

## Deployment

- Turn 0 is deployment-only. Normal commands begin on Turn 1.
- Two of the board's 16 lines—eight rows and eight columns—are rolled as unavailable deployment lines at the start of each match.
- The same line may be rolled twice.
- Deployment blocking affects only the initial drop; those buildings become usable after deployment.
- Each nation chooses one valid starting building and deploys all six units there.
- Starting locations are revealed to every nation.
- Different nations may choose the same starting building.
- A shared landing creates an immediate two-, three-, or four-way hot-drop encounter.

## Movement and Hidden Information

- After deployment, enemy movement remains hidden unless detected legitimately.
- Friendly units on one building can be selected as a group or individually.
- A movement command may target only an adjacent building.
- Commands can be previewed, edited, cancelled, and confirmed before resolution.
- Teams may split to claim territory and later regroup.
- AI nations must obey the same visibility restrictions as the player.

## Territory

A nation claims a building when one or more of its units enter it uncontested or eliminate all enemies occupying it.

- A claimed building remains owned after the nation leaves it.
- Ownership changes when another nation successfully occupies it.
- Territory is scored only when:
  - the player is defeated; or
  - the player wins the match.
- Each owned building is worth one territory point.

Territory rewards splitting and map coverage, while combat rewards concentration.

## Unit Identity and Stats

Every active and historical warrior belongs to a persistent nation record. A unit has four primary stats.

### Resistance

Resistance is the unit's health.

- Damage reduces current Resistance.
- A unit is defeated when current Resistance reaches zero.
- Damage persists between encounters during the match.
- Units enter later encounters with their remaining Resistance.

### Strength

Strength determines the unit's direct combat damage.

### Intelligence

Intelligence allows a unit to detect nearby enemies.

- Each Intelligence point gives a **5% detection chance**.
- Detection checks apply to enemy units in adjacent buildings.
- Successful detection reveals a signal rather than complete information.
- Signals may indicate:
  - movement detected;
  - small force;
  - squad detected;
  - large force.
- Exact enemy stats and current Resistance remain hidden.

### Agility

Agility determines the order in which all participating units act during combat encounters.

- Units from every participating nation share one encounter order.
- The exact Agility formula is not yet locked.
- Whether Agility only sorts one action per round or can also create extra actions remains a balance decision.

## Armor

Armor is a persistent defensive resource separate from Resistance.

- Armor reduces incoming damage.
- The strength of its reduction depends on its remaining integrity.
- Armor integrity changes only in quarter increments:
  - `0.00`;
  - `0.25`;
  - `0.50`;
  - `0.75`;
  - `1.00`.
- Armor persists between encounters.
- A Hammer hit removes one armor pip (`−0.25`).
- A stationary recovery turn can restore one armor pip (`+0.25`) when armor repair is selected.
- The exact damage-reduction curve remains to be balanced.

## Recovery

Remaining safely on one building allows a unit to recover instead of rotating.

- The first consecutive idle turn does not restore Resistance.
- Recovery begins after the unit has remained stationary for multiple consecutive turns.
- Moving, retreating, or entering combat resets the rest counter.
- Recovery cannot exceed maximum Resistance or full armor integrity.
- The player can set a stationary unit to prioritize either:
  - healing Resistance; or
  - repairing armor.
- Healing is the default when no choice is made.
- If Resistance is already full, armor repair is selected automatically.
- Armor repair restores one pip per eligible stationary recovery turn.
- The exact healing amount remains a balance variable.

## Combat Encounters

Combat begins when units from different nations occupy the same building after movement resolution or share a deployment building during a hot drop.

- Encounters may involve two, three, or all four nations.
- Each surviving unit receives its own combat turns.
- The order of those turns is determined by Agility.
- When attacking, the acting unit first selects an enemy nation and then a target unit within that nation.
- The attacker then chooses one of four weapon types:
  - Sword;
  - Hammer;
  - Flail;
  - Bow.
- The defending nation is shown which weapon is being used before choosing the corresponding defensive response.
- Hidden attack choices are committed before the defense is resolved.
- A unit uses only one weapon type during one combat turn and cannot switch weapons during a Sword critical chain.

### Sword

The Sword is the baseline high-damage duelling weapon.

The attacker secretly chooses one direction:

- Up;
- Down;
- Left;
- Right.

The defender chooses:

- Center/Guard; or
- Up;
- Down;
- Left;
- Right.

| Relationship Between Choices | Outcome |
|---|---|
| Same direction | Critical hit with slightly increased damage; attacker may strike again |
| Opposite direction (`Up–Down`, `Left–Right`) | Complete dodge; no damage |
| Perpendicular direction | Normal hit |
| Center/Guard | Normal hit with no critical |

- A critical grants another Sword exchange within the same unit turn.
- A Sword chain is limited to **three total hits in one turn**.
- The weapon cannot change during the chain.
- The exact critical damage bonus remains to be balanced.

### Hammer

The Hammer is a low-damage armor-breaking weapon.

- The attacker secretly chooses Left, Center, or Right.
- The defender chooses Left, Center, or Right.
- Matching choices produce a hit.
- Different choices produce a miss.
- There are no critical outcomes.
- The current working damage is **25% of normal Sword damage**.
- Every successful hit removes one armor pip (`−0.25`).

### Flail

The Flail applies low area damage across an enemy nation rather than targeting one unit.

- The attack randomly selects approximately half of the enemy team's surviving units.
- For odd team sizes greater than one, the number selected rounds up.
- Against one remaining unit, the Flail has a 50% chance to miss.
- Before the random targets are revealed, the defender chooses approximately half of the team to shield.
- The number of shielded units also rounds up.
- Randomly selected units that are not protected receive the Flail's low direct damage.
- The exact per-unit damage remains to be balanced.
- The interaction between the one-unit 50% attack rule and rounded-up shielding still requires a final rule.

### Bow

The Bow is a delayed, guaranteed heavy attack.

- A Bow attack takes two turns from the archer:
  1. **Aim:** select and lock a target unit.
  2. **Fire:** release the prepared shot.
- A fired shot has a 100% hit chance.
- A shot deals **1.75× normal Sword damage**.
- Bow attacks cannot critically hit.
- Every hit has a **50% chance to apply one Bleeding stack**.
- Bleeding stacks accumulate.
- Each stack removes **1% of the target's maximum Resistance per combat turn**.
- Bleeding lasts until the end of the encounter.

Bow information is asymmetric:

- The aiming nation can see exactly which unit each of its archers is targeting.
- Other nations can see which units are preparing Bow shots.
- Other nations cannot see the targets of those prepared shots.
- Targets are revealed when the arrows are fired.

Prepared arrows also punish retreat:

- When a nation commits to retreat, every prepared enemy arrow currently aimed at that nation fires before the retreat completes.
- The shots deal their normal Bow damage to their locked targets.
- This is the only currently defined free attack caused by retreat.
- The interaction between a retreat volley, newly applied Bleeding, and the encounter ending remains to be finalized.

## Retreat and Chained Encounters

- A nation cannot retreat before completing at least one full combat round.
- Retreat moves all surviving members of that nation together.
- The retreat destination may be any neighboring building.
- The destination may be empty, friendly-controlled, or occupied by an enemy nation.
- Prepared Bow shots aimed at the retreating nation resolve before movement.
- Current Resistance and armor persist through the retreat.
- Retreat resets regeneration/rest progress.
- Retreating into an enemy-occupied building immediately begins another encounter.
- The nation must complete at least one full round in that new encounter before it may retreat again.
- Combat ends for a nation when:
  - all of its participating units are defeated; or
  - its surviving units successfully retreat.
- A multi-team encounter continues until only one nation remains or every other surviving nation has retreated.

## Shrinking Zone

The safe zone prevents stalling and compresses the match.

- The match begins with all 64 buildings available.
- The safe region shrinks through five turn-driven stages.
- The implemented shrink windows are:
  - Turns 5–8;
  - Turns 12–15;
  - Turns 18–21;
  - Turns 24–27;
  - Turns 30–33.
- The current stage is shown in the HUD.
- The current safe-region border is visible.
- Unsafe buildings are visually marked as dangerous.
- Stage changes use queued, pooled, tap-to-dismiss transition messages.
- Units outside the safe region take zone damage.
- Zone damage increases as stages advance.
- Zone damage resolves after movement and combat.
- Exact safe-region sizes and zone-damage values remain balance variables.

## Match Results and Scoring

Placement rewards:

| Placement | Placement Points |
|---|---:|
| 1st | 5 |
| 2nd | 2 |
| 3rd | 1 |
| 4th | 0 |

- Every owned building is worth one territory point.

When the player is defeated:

- territory is scored using the board state at that moment;
- the player's placement is locked;
- surviving AI nations are ranked by surviving unit count;
- equal surviving-unit counts are resolved randomly;
- the match ends immediately.

When the player wins:

- final territory ownership is scored;
- first-place points are awarded;
- the year is recorded as a national victory;
- the nation receives its victory progression.

## Years and Nation Progression

Every match represents one year of competition between the same four persistent nations.

- The winner receives at least one unit-training point; the final award formula remains open.
- Every losing nation must sacrifice one active warrior.
- The sacrificed warrior is permanently removed from the active roster.
- A completely blank replacement unit enters that nation.
- A blank unit begins with base stats, no allocated training points, and no accumulated history.
- Player and AI nations all progress under the same underlying rules.
- A unit may receive a maximum of **20 total training points** across its stats.

The player record tracks at least:

- the current number of consecutive victorious years;
- the total number of victories.

### Classical Republic Sacrifice

The player's nation uses a republican lottery followed by a final player decision.

1. After a lost year, three of the six active warriors are selected uniformly at random as sacrifice contenders.
2. The player chooses one of those three contenders.
3. The chosen warrior is permanently sacrificed.
4. A blank unit fills the vacant roster position.

All six active roster members are part of the lottery, including blank replacements. Therefore, a blank may become a future contender.

AI nations also progress, lose warriors, and receive replacements. Their exact sacrifice decision logic and the separate difficulty-scaling system remain to be designed.

## Nation History

Every nation retains a persistent historical record rather than deleting departed units from memory.

Nation History records:

- each year's winner;
- victory totals and current streaks;
- sacrifice nominees;
- the final sacrificed warrior;
- incoming blank replacements;
- active warriors;
- historical warriors who are no longer on the roster.

Historical warrior records preserve at least:

- identity/name;
- nation;
- arrival and departure years;
- years served;
- allocated training points and build;
- combat record;
- sacrifice nominations;
- final fate.

## AI Philosophy and Difficulty

AI nations should remain readable and competitive without cheating.

AI nations should:

- obey the same hidden-information rules as the player;
- never access undetected player positions;
- capture empty buildings;
- group when threatened;
- split when territory is valuable;
- rotate toward the safe zone;
- avoid clearly stronger visible forces;
- attack weaker visible forces;
- preserve damaged units when reasonable;
- choose targets, weapons, defenses, retreats, upgrades, and sacrifices;
- pursue placement, yearly victory, and long-term national strength.

Because AI nations now retain progression and Nation History, difficulty can no longer depend only on a fixed enemy training budget. A separate difficulty-scaling design is required and is not yet locked.

## Mobile Interaction Goals

The game should remain comfortable to control with a small number of taps.

Strategic interaction flow:

1. Tap a building containing friendly units.
2. All units on that building are selected by default.
3. Tap unit portraits to include or exclude individual units.
4. Tap an adjacent building to preview a movement order.
5. Confirm, edit, or cancel the command.
6. Resolve all confirmed orders through the turn system.

Combat interaction changes according to the selected weapon, but every weapon follows the same broad flow:

1. Select an enemy nation.
2. Select a target unit when the weapon requires one.
3. Select a weapon and commit the attack choice.
4. Show the weapon type to the defender.
5. Receive the weapon-specific defensive response.
6. Reveal and resolve the outcome.

## Design Pillars

1. **Information matters**  
   Enemy movement, attack choices, and Bow targets create meaningful uncertainty without giving AI hidden knowledge.

2. **Every formation has a cost**  
   Grouping protects units but sacrifices territory. Splitting captures ground but creates vulnerable targets.

3. **The zone authors the match**  
   Every shrink forces new routes, confrontations, sacrifices, and rotations.

4. **Combat is compact but expressive**  
   Four weapons create distinct prediction, targeting, defense, and timing problems without replacing the board strategy.

5. **Progression creates identity**  
   The six units become a personalized historical roster rather than disposable pieces.

6. **Loss has a face**  
   Defeat threatens a named warrior and changes the permanent history of every nation.

7. **The scope stays controlled**  
   Buildings share one ruleset, AI uses understandable heuristics, and the campaign reuses the same core systems.

## Current Implementation Status

Damnation is beyond pre-production and has a working Unity mobile prototype.

Implemented and tested:

- the complete 8×8 / 64-node battlefield;
- reciprocal orthogonal adjacency;
- mobile tap, pan, and pinch-zoom camera input;
- bounded camera movement and zoom;
- two randomly rolled blocked deployment lines;
- blocked-node presentation and explanatory notifications;
- starting-node selection with confirmation and cancellation;
- pooled notification UI;
- six registered player units and Unit Cards;
- group and individual unit selection;
- adjacent movement targeting;
- Command and Decision menus;
- movement ghost previews;
- command confirmation, editing, and cancellation;
- basic simultaneous player-order resolution and central turn orchestration;
- a five-stage turn-driven shrinking zone;
- zone borders and unsafe-node colouring;
- stage number and progress feedback;
- queued, pooled, tap-to-dismiss state-transition popups.

Designed but not yet implemented as complete production systems:

- the Agility-based encounter timeline;
- Sword, Hammer, Flail, and Bow combat;
- armor and weapon-specific defenses;
- retreat and chained encounters;
- AI movement and combat decision-making;
- Intelligence-based fog and detection;
- full zone damage integration;
- match scoring and victory flow;
- yearly progression, sacrifice, AI nation growth, and Nation History;
- long-term persistence and balancing.

The next development hand is reserved for combat encounters. Its exact phases will be finalized after the combat design is sufficiently settled.

## Open Rules and Balance Variables

The following are intentionally not presented as finalized facts:

- the exact Agility/initiative formula;
- whether Agility affects only order or also action frequency;
- base Resistance, Strength, Intelligence, and Agility values;
- normal Sword damage and the Sword critical multiplier;
- whether a Sword critical chain may change targets;
- the armor damage-reduction curve and Hammer damage-order details;
- Flail damage and the one-unit shielding edge case;
- the exact timing of Bleeding ticks and whether armor affects Bleeding;
- how Bleeding from a retreat volley behaves when the encounter immediately ends;
- healing amounts and the precise first eligible armor-repair turn;
- final zone damage values and safe-region sizes;
- AI retreat thresholds, upgrade allocation, sacrifice logic, and difficulty scaling;
- final progression-point award formulas.

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

Its encounter timing also draws inspiration from the visible Agility-driven flow of *Clair Obscur: Expedition 33*, while Damnation's formulas and weapon interactions will remain original.

The goal is not to reproduce a shooter or another RPG's combat. The goal is to translate large-scale strategic pressure, hidden information, and attachment to a persistent roster into a compact mobile board game.
