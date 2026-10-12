# Damnation — Development Phases Hand 2

> **Coverage:** Phases 6–10  
> **Status:** APPROVED  
> **Focus:** AI nation foundation and strategic movement decisions  
> **Platform direction:** Mobile, Android-first, landscape  
> **Engine direction:** Unity 2D

This file is the authoritative production plan for the second five development phases of Damnation. It begins from the completed Hand 1 battlefield, player deployment, unit movement, central turn flow, and shrinking-zone systems.

Hand 2 introduces the three AI-controlled nations, the territory and match-result rules that give them objectives, the information each AI is legitimately allowed to use, and progressively more capable strategic movement decisions. It ends with AI nations that can deploy, move, split, spread, and regroup through the same simultaneous turn system as the player.

Combat encounters are intentionally not implemented in this hand. Hand 2 may detect and register contested buildings or hot drops, but it must not invent temporary combat outcomes that conflict with the future encounter system.

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

## Shared Hand 2 Rules

### Equal-Stat Decision Scope

All units begin the first match with the same stats. During Hand 2, AI strategic decisions therefore use unit count, position, territory, placement, zone state, and legitimately known enemy information. They do not compare individual unit stats when choosing movement or separation plans.

The runtime unit model must continue to preserve each unit's actual stats, and later systems must be able to add stat-aware decisions without replacing the Hand 2 architecture. Stat-aware combat, movement, retreat, and preservation logic remain outside this hand.

### Fair Information

Every AI nation makes decisions from its own information state rather than from unrestricted access to the authoritative board.

- An AI always knows the exact state and location of its own units.
- Public match information may be shared with every nation.
- An AI may remember previously revealed enemy information.
- Hidden enemy movement must not silently update an AI's knowledge.
- One AI must not receive another nation's private information or unconfirmed movement order.

The authoritative match state may be used to resolve actions after every nation has committed its orders. It must not be used as secret decision input.

### Probabilistic but Reproducible Decisions

AI choices should not always select one mathematically highest-scoring option. Stronger options should receive greater probability while still allowing controlled variation.

Random selection must support reproducible testing through an inspectable or injectable seed. Debug information should make it possible to see which legal options were evaluated, how they were weighted, and which option was selected.

---

# Phase 6 — AI Teams Initialization

## Goal

Expand the player-only unit foundation into a four-nation match containing the player-controlled nation and three AI-controlled nations, each with six valid runtime units.

This phase creates the teams and units that later AI systems will control. It does not choose deployment buildings or issue AI orders.

## Required Work

### Nation Runtime Model

- Represent exactly four nations in the match:
  - one player-controlled nation;
  - three AI-controlled nations.
- Give every nation a stable identity independent from its display name, colour, or visual objects.
- Track whether a nation is player-controlled or AI-controlled.
- Maintain each nation's six-unit roster.
- Expose controlled queries for:
  - all units belonging to a nation;
  - living units belonging to a nation;
  - units occupying a specified building;
  - the number of surviving units;
  - whether the nation has any surviving units.
- Avoid separate hard-coded logic for AI Nation 1, AI Nation 2, and AI Nation 3.

### AI Unit Initialization

- Create exactly 18 AI units: six for each AI nation.
- Give every unit a stable identity and correct nation ownership.
- Use the same runtime unit model, validation rules, damage rules, and movement-order fields used by player units.
- Initialize all first-match units with the same approved base stats.
- Keep units unplaced until the deployment system assigns their starting building in Phase 8.
- Ensure an unplaced unit cannot receive zone damage or participate in a normal movement turn.

### Existing-System Generalization

- Remove player-only assumptions from unit registration and relevant runtime queries.
- Make the zone backend capable of identifying and damaging units from any nation.
- Make unit elimination update the correct nation's surviving-unit count.
- Preserve all existing player selection, movement, damage, and presentation behavior.
- Keep authoritative nation and unit state outside replaceable visual prefabs.

### Presentation and Debugging Foundation

- Give every nation a distinct temporary presentation identity suitable for testing.
- Allow developers to inspect all four rosters and their ownership assignments.
- Do not make debug visibility equivalent to player-visible enemy information.
- Preserve a future visibility boundary so enemy unit views can be shown or hidden independently from their runtime existence.

## Out of Scope

- AI deployment choices.
- AI movement orders.
- Territory ownership and scoring.
- Placement and match-end resolution.
- AI information memory.
- Combat and hot drops.
- Permanent progression, names, Nation History, or sacrifices.
- Stat-aware AI decisions.

## Exit Conditions

- The match initializes exactly four stable nation records.
- The player nation retains its six functioning units.
- Each AI nation owns exactly six distinct valid units.
- All 24 units use the same runtime rules without duplicated nation-specific implementations.
- AI units exist safely before deployment without occupying arbitrary buildings or receiving invalid zone damage.
- Unit queries and surviving-unit counts return correct results for every nation.
- Existing player deployment and movement behavior remains functional.

---

# Phase 7 — Win Conditions and Points System

## Goal

Implement building ownership, territory counting, nation elimination, placement assignment, match-end conditions, and final scoring so AI decisions can pursue meaningful match objectives.

Placement determines who won the year. Final points combine placement reward with territory held when the match ends.

## Required Work

### Building Ownership

- Give every building an authoritative ownership state that is either unclaimed or assigned to one nation.
- Claim an unclaimed or enemy-owned building when a nation occupies it without hostile units present.
- Claim an uncontested deployment building after all starting locations have been revealed.
- Keep a building owned after its occupying units leave.
- Transfer ownership when another nation later occupies the empty or otherwise uncontested building.
- Do not transfer a contested building merely because hostile units arrived there.
- Preserve the current owner until a future encounter result identifies the occupying winner.
- Provide a future combat-facing operation that can award a contested building to the victorious nation.
- Keep ownership state in the board model rather than only in tile colours or labels.

### Territory State

- Count the buildings currently owned by each nation.
- Treat each owned building as one potential territory point.
- Update territory counts when ownership changes.
- Do not permanently add one point every turn; territory is scored from the ownership snapshot taken when the match ends.
- Expose ownership and territory changes to presentation, match-result, and AI-information systems without making those consumers authoritative.

### Nation Elimination and Placement

- Mark a nation eliminated when it has no living units.
- Prevent eliminated nations from deploying, planning, moving, capturing buildings, or receiving later turns.
- Record the order in which nations are eliminated.
- Assign placements from fourth toward first as the match narrows.
- If the player is eliminated, lock the player's placement and end active match play immediately.
- When the player is eliminated, rank every still-surviving AI nation by surviving-unit count.
- Resolve equal surviving-unit counts randomly using the match random source.
- If all three AI nations are eliminated while the player survives, declare the player first and end the match as a victory.
- Finalize the handling of simultaneous elimination edge cases during the Phase 7 implementation review before the phase is locked.

### Placement and Final Points

Award placement points according to the established table:

| Placement | Placement Points |
|---|---:|
| 1st | 5 |
| 2nd | 2 |
| 3rd | 1 |
| 4th | 0 |

- Snapshot final territory ownership when the match ends.
- Award one territory point per owned building.
- Calculate each nation's final match score as placement points plus territory points.
- Preserve placement points, territory points, total points, surviving-unit count, and final placement as separate result values.
- Do not treat the highest total point score as a replacement for first-place survival; the first-place nation is the winner of the year.
- Produce one complete immutable match-result record after finalization.

### Minimal Presentation and Verification

- Give owned buildings a readable temporary nation indicator that remains compatible with selection and zone overlays.
- Show or expose each nation's current territory count for testing.
- Provide a clear basic match-result summary containing placement, territory, placement points, and total points.
- Prevent match results from being calculated or awarded more than once.

## Out of Scope

- Permanent victory rewards and unit-training points.
- Sacrifice selection and blank replacements.
- Nation History and multi-year persistence.
- Full final-results visual polish.
- Combat resolution.
- AI use of the scoring system.
- Final tie balancing beyond the approved match rules.

## Exit Conditions

- Buildings begin unclaimed and change ownership only through valid capture operations.
- Leaving a building does not remove its owner.
- Contested occupation does not incorrectly transfer ownership before an encounter result exists.
- Territory counts always match the authoritative board ownership state.
- A nation with no surviving units is eliminated and cannot act again.
- Player defeat and player victory both end the active match exactly once.
- All four placements can be assigned under the established end conditions.
- Final placement, territory, and total points are calculated correctly and stored separately.
- Repeated finalization calls cannot duplicate rewards or alter a completed result.

---

# Phase 8 — AI Deployment and Decision Information Backend

## Goal

Deploy all three AI nations through the established legal deployment rules and create the fair, nation-specific information backend from which later AI decisions will operate.

AI deployment is intentionally random. Strategic weighting begins with normal turn decisions, not with starting-building selection.

## Required Work

### AI Deployment Selection

- Give each AI nation one independently selected random starting building.
- Select only from buildings that are valid under the two disabled deployment-line rolls.
- Use uniform random selection among valid buildings.
- Do not reserve a selected building or remove it from the choices available to later nations.
- Allow two, three, or all four nations to choose the same starting building.
- Keep AI selections hidden until the player has confirmed a starting building.
- Reveal all four confirmed starting buildings together after every nation has committed.
- Place all six units of each nation on its confirmed starting building.
- Claim uncontested starting buildings through the Phase 7 ownership system.

### Hot-Drop Registration

- Check the four confirmed starting buildings for multi-nation occupation.
- Register every shared starting building as a pending hot-drop encounter.
- Identify all participating nations and units without attempting to choose a winner.
- Prevent an unresolved hot drop from being mistaken for uncontested ownership.
- Prevent Turn 1 from beginning while a pending hot drop remains unresolved.
- Preserve the future hook through which the combat system will resolve the encounter before Turn 1.
- Do not create a temporary auto-win, random elimination, or forced relocation rule for this hand.

### Nation-Specific Knowledge State

Create a separate information state for each AI nation containing at least:

- exact locations and current runtime condition of its own units;
- its own current territory and placement state;
- public zone state, including current and revealed future safety information;
- publicly visible building ownership and territory standings;
- all publicly revealed deployment locations;
- remembered enemy observations;
- the turn on which each remembered observation was obtained;
- the source or type of each observation where relevant.

Enemy observations must support last-known information without treating it as live truth. A deployment location known on Turn 0 remains a historical observation after that enemy moves unless a legitimate later event reveals new information.

### Decision Context and Relative Information

- Build a read-only decision context from the AI nation's own knowledge state.
- Derive useful relative information without exposing hidden authoritative state, including:
  - whether a candidate building is owned by itself, an opponent, or nobody;
  - whether a candidate is currently safe, becoming unsafe, or inside the revealed future region;
  - grid distance toward the current or future safe region;
  - distance to remembered enemy observations;
  - nearby friendly support visible to that AI;
  - potential territory change from uncontested occupation.
- Keep the knowledge state separate from the decision logic that will consume it.
- Allow future Intelligence detection, encounters, public events, and scouting systems to submit new observations without redesigning the knowledge model.
- Do not supply exact hidden enemy locations, planned orders, or private unit stats.

### Inspection and Fairness Verification

- Provide a development view or structured log for inspecting one AI nation's current knowledge and derived decision context.
- Make it possible to compare knowledge against authoritative match state during testing without merging the two.
- Verify that moving a hidden enemy does not automatically move that enemy inside another nation's knowledge state.
- Verify that the three AI nations can hold different knowledge about the same enemy at the same time.

## Out of Scope

- Intelligence-based detection rolls.
- Player-facing fog-of-war presentation.
- Strategic deployment weighting.
- AI movement selection.
- Team separation.
- Hot-drop combat resolution.
- Exact enemy-stat knowledge.
- Stat-aware decisions.

## Exit Conditions

- Every AI nation selects exactly one valid starting building uniformly and independently.
- AI deployment never uses a blocked building.
- Duplicate AI or player starting buildings remain possible.
- All 24 units occupy their nation's confirmed starting building after a non-contested deployment.
- All starting locations are revealed only after every nation has committed.
- Uncontested deployment buildings receive the correct owner.
- Shared deployment buildings create accurate pending hot-drop records and are not falsely resolved.
- An unresolved hot drop stops match progression at the future encounter-resolution boundary.
- Every AI receives its own valid information state and read-only decision context.
- Historical enemy observations do not update from hidden authoritative movement.
- No AI context exposes another nation's unconfirmed order or undetected live position.

---

# Phase 9 — AI Neighbouring-Node Selection

## Goal

Allow each AI nation to evaluate staying or moving to one legal neighbouring building, choose probabilistically from those options, and submit one whole-squad movement order through the simultaneous turn system.

During this phase, every AI nation keeps all of its surviving units together. Choosing how to divide the team is deferred to Phase 10.

## Required Work

### Legal Action Generation

- Generate one hold option for the AI squad's current building.
- Generate one movement option for every orthogonally adjacent building.
- Treat unsafe buildings as legal but undesirable rather than silently changing the player's movement rules for AI nations.
- Exclude nonexistent, non-adjacent, or otherwise illegal destinations.
- Assign the selected destination to every living unit in the AI nation.
- Represent holding through no movement order or the established explicit hold behavior without moving the units away and back.

### Candidate Evaluation

Evaluate every candidate using only the Phase 8 decision context. The initial utility model must account for at least:

- immediate zone safety;
- movement toward the current or revealed future safe region;
- potential capture of an unclaimed or enemy-owned building;
- preservation of currently owned territory when relevant;
- proximity to remembered enemy forces;
- the age and reliability of enemy observations;
- current territory and placement pressure;
- whether the move leaves useful legal options for the following turn.

The exact numerical weights are balancing variables to be approved during implementation review. Zone survival must be strong enough that an AI does not behave as though territory is valuable after death.

Unit count may be used as the current estimate of friendly force size. Individual Strength, Resistance, Intelligence, and future Agility differences must not influence Hand 2 movement utility.

### Probabilistic Selection

- Convert valid candidate utilities into weighted selection probabilities.
- Give strategically stronger candidates greater probability.
- Prevent illegal candidates from receiving any probability.
- Avoid a system in which every legal option is effectively equally random.
- Avoid a system that always makes the same highest-scoring choice when several choices are credible.
- Support deterministic replay with a controlled random seed.
- Record evaluated candidates, utility components, final weights, and the selected action for development testing.

### Simultaneous Turn Integration

- Capture or preserve the fair decision snapshot before movement resolution.
- Do not expose the player's planned destination to AI decision contexts.
- Have all three AI nations commit orders before any nation's movement changes authoritative positions.
- Prevent an AI processed later in the planning loop from observing an earlier AI's unconfirmed order.
- Resolve player and AI movement through the same central movement step.
- Update previous and current buildings through the existing unit runtime operations.
- Detect multi-nation destinations as pending encounters.
- Apply uncontested building capture after movement through the Phase 7 ownership system.
- If any pending encounter exists, pause the central sequence at the future encounter-resolution step rather than applying zone damage or completing the turn.
- If no pending encounter exists, preserve the established end-of-turn zone step and turn advancement.
- Skip eliminated nations safely.

### Behaviour Verification Scenarios

Test at least the following controlled situations across repeatable seeds:

- a safe unclaimed neighbour compared with an otherwise equivalent owned neighbour;
- a neighbouring building that will remain safe compared with one becoming unsafe;
- an AI outside the safe region with several possible routes inward;
- a recent enemy observation compared with an old observation;
- a corner, edge, and interior starting position;
- an eliminated AI nation that must produce no order.

These scenarios verify relative weighting and legality. They do not establish final game balance.

## Out of Scope

- Splitting any AI squad.
- Coordinating several AI groups from the same nation.
- Selecting individual units for different destinations.
- Combat attacks, defenses, targets, weapons, or retreat.
- New enemy-detection rules.
- Stat-aware utility.
- AI personality and difficulty variants.

## Exit Conditions

- Every active AI nation generates only hold or legal adjacent movement candidates.
- Every living unit in one AI nation receives the same selected movement result.
- Candidate evaluation reads only that nation's decision context.
- Hidden enemy movement and player orders cannot influence AI selection illegitimately.
- Strategic candidate weights respond correctly to territory, zone, and remembered-enemy test scenarios.
- Repeated runs with one seed reproduce the same choices, while different seeds can produce controlled variation.
- All three AIs commit before simultaneous movement begins.
- AI movement, uncontested capture, pending-encounter detection, encounter gating, zone damage, and turn advancement execute in the correct central sequence.

---

# Phase 10 — AI Team Separation and Coordination

## Goal

Extend AI strategic movement from one indivisible squad into whole-team planning that can divide units across destinations, maintain several groups, spread for territory, and regroup for safety or concentration.

This phase creates the central strategic tradeoff of Damnation: territory rewards separation, while survival and future combat reward concentration.

## Required Work

### Whole-Team Plan Generation

- Generate candidate plans for every active AI nation before assigning final unit orders.
- Let a plan assign the units on each currently occupied building among:
  - holding their current building;
  - one or more legal neighbouring buildings.
- Include credible plan types such as:
  - remaining fully grouped;
  - detaching a small group;
  - dividing into two substantial groups;
  - spreading across several valuable destinations;
  - moving separated groups toward a common regrouping building;
  - withdrawing exposed groups toward safety or friendly support.
- Use a controlled set of approved separation patterns rather than exhaustively enumerating every mathematical partition and destination combination.
- Keep the exact candidate-pattern set configurable and approve it during the Phase 10 implementation review.

Possible six-unit starting patterns include `6`, `5+1`, `4+2`, `3+3`, `4+1+1`, and `2+2+2`; these are candidate tools for plan generation rather than a requirement that every pattern always be considered.

### Team-Level Plan Evaluation

Evaluate complete plans rather than letting every unit choose independently. Plan utility must account for at least:

- number and value of potential territory captures;
- immediate and future zone safety of every resulting group;
- concentration of friendly units near remembered threats;
- exposure of isolated small groups;
- ability of separated groups to support or rejoin one another;
- current territory standing and placement pressure;
- expected survival value of remaining grouped;
- future legal movement options after the split;
- conflicts or duplication among the nation's own proposed destinations.

The point system should make safe expansion attractive, while zone danger and credible threats should increase the value of concentration or regrouping. Exact weights remain balance variables.

### Equal-Stat Unit Assignment

- Decide group sizes before selecting the specific unit identities assigned to those groups.
- Assign specific equal-stat units through a stable neutral rule after the plan is chosen.
- Do not pretend that one base-stat unit is tactically more valuable than another during the first-match scope.
- Preserve every unit's individual identity and runtime state even though those differences do not affect current plan utility.
- Ensure every living AI unit receives at most one destination and is represented exactly once in the selected plan.

### Separation, Persistence, and Regrouping

- Support AI units from one nation occupying several buildings after resolution.
- Treat co-located friendly units as one selectable planning group on the following turn while preserving individual unit records.
- Allow groups arriving on the same building to merge naturally.
- Let later plans keep groups separate, divide them further, or route them toward regrouping.
- Prevent a permanently spreading-only strategy by giving cohesion and threat response explicit plan value.
- Avoid assuming that a nation always begins a turn on one building.

### Probabilistic Plan Choice

- Convert complete-plan utilities into controlled selection weights.
- Choose among credible team plans probabilistically.
- Give clearly self-destructive plans negligible or zero weight when a valid safer alternative exists.
- Use the shared seeded random source for reproducible tests.
- Log the candidate plan structures, major utility components, final weights, and selected plan.

### Integration and Stress Testing

- Submit the selected plan as ordinary per-unit movement orders to the existing central turn system.
- Preserve simultaneous planning and movement across all four nations.
- Keep pending encounters unresolved but correctly identified, and stop the central sequence at the encounter boundary when one occurs.
- Verify ownership updates only for uncontested destinations.
- Run repeated multi-turn simulations to detect:
  - duplicated or lost units;
  - illegal destinations;
  - units receiving several orders;
  - groups that cannot be represented on the next turn;
  - hidden-information leaks;
  - permanent idle loops;
  - uncontrolled fragmentation;
  - failure to rotate during zone pressure.
- When a stress test produces an encounter, verify the pending-encounter record and encounter gate rather than inventing a temporary combat result.
- Test at minimum safe expansion, imminent-zone rotation, nearby remembered threat, regrouping, and late-match compressed-region scenarios.

## Out of Scope

- Individual-stat-based separation or preservation.
- Combat strength simulation beyond visible or remembered unit counts.
- Weapon, target, defense, and retreat decisions.
- Intelligence detection implementation.
- AI personalities, nation doctrines, or difficulty-specific decision weights.
- Permanent roster progression, sacrifices, replacements, or Nation History.
- Machine-learning, minimax, or exhaustive optimal-play solvers.
- Final AI balance.

## Exit Conditions

- An AI nation can legally remain together, split into several groups, preserve an existing separation, and regroup.
- Every selected plan accounts for every living unit exactly once.
- All resulting orders use only hold or legal neighbouring destinations.
- Complete plans are evaluated through territory, placement, zone, cohesion, and legitimately known threat information.
- Individual unit stats do not affect Hand 2 plan selection.
- Safe territorial expansion can make separation more likely.
- Imminent zone danger or credible nearby threat can make rotation or regrouping more likely.
- Probabilistic plan selection remains reproducible under a fixed seed.
- All four nations continue to plan and resolve movement simultaneously.
- Multi-turn stress tests complete without unit duplication, disappearance, illegal orders, information leaks, or turn-flow corruption.

---

# Phase Hand 2 Completion Condition

`phases_hand_2.md` is complete when Phases 6–10 are individually reviewed, locked, and committed.

At that point, the project must contain:

- one player nation and three AI nations with six valid units each;
- building ownership, territory counts, placement, win conditions, and final match scoring;
- independent random legal deployment for all three AI nations;
- public deployment reveal and pending hot-drop registration;
- separate fair knowledge and decision contexts for every AI nation;
- probabilistic neighbouring-node selection using territory, zone, and remembered information;
- simultaneous player and AI movement resolution;
- AI team separation, continued multi-group planning, and regrouping;
- seeded diagnostics and repeatable AI behaviour tests;
- explicit extension points for future detection, combat, and stat-aware decisions.

Hand 2 does not produce complete combat-capable opponents. It establishes the strategic AI layer that chooses where units go and how they divide. Combat encounters, Intelligence-based detection, stat-aware decisions, permanent progression, and multi-year nation systems remain for later phase hands.
