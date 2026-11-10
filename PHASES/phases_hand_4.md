# Damnation — Development Phases Hand 4

> **Coverage:** Phases 16–20  
> **Status:** DRAFT FOR REVIEW  
> **Focus:** Combat AI and complete live encounter resolution  
> **Platform direction:** Mobile, Android-first, landscape  
> **Engine direction:** Unity 2D

This file proposes the fourth five-phase production hand for Damnation. Hand 3 supplies a battle rules engine, a player Battle screen, a gated encounter queue, and repeatable controlled scenarios. Hand 4 replaces scripted development opponents with fair, state-based combat AI and connects both player-involved and AI-only battles to the live board.

The current game design defines the intended rules. The Hand 3 scenario scripts remain development fixtures; they must not become the production opponent policy or silently settle real matches. Hand 4 finishes the encounter MVP: hot drops and normal contests resolve, retreat can create later encounter steps, and the board advances only after the required battles conclude.

Intelligence detection and the player Notes system are reserved for Hand 5. Hand 4 may consume legitimately available observations from existing nation knowledge, but it does not invent new scouting or reveal rules. Multi-year progression, sacrifices, Nation History, and Possession remain later work.

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

## Shared Hand 4 Rules

### One Battle Engine

- AI decisions and player decisions enter the same Hand 3 battle engine. There is no second hit, damage, armor, Bleeding, or timeline implementation for AI-only encounters.
- The AI chooses weapons, targets, directions, defenses, Flail shields, and retreat from the current battle state and its nation's legitimate information.
- When one choice clearly stands out, choose it. When several strong choices are close, select among them with controlled weighted randomness. Exact evaluation weights are tuning values for implementation review.
- Use an inspectable seed and decision trace so a controlled scenario can reproduce both AI choices and combat rolls.
- An AI-only battle runs quickly out of view without a Battle scene or human decision timer. Its existence and result do not become player-visible merely because the simulation ran.
- Within an encounter step, AI-only fights use their existing registration order. After a fight finishes, a nation may use its own casualties and other legitimately observed results in a later fight; nations absent from that fight receive no private information.

### Fair Information

- A combat AI knows its own units and the battle information its nation can actually observe. It does not read hidden board positions, enemy movement orders, or another nation's private Bow Aim target.
- Bow preparation reveals the preparing archer to all participants, while the locked target remains visible only to the aiming nation until Fire.
- A retreat destination may conceal enemy occupancy. The AI evaluates the adjacent choices using its own learned information; authoritative occupancy determines whether the move creates another encounter only after the choice.
- The player Battle screen can show AI-versus-AI actions occurring in that battle. Unrelated AI-only encounters elsewhere remain hidden.

### Board Sequence

- Finish every encounter at step 1 before step 2, then every encounter at step 2 before step 3, and so on.
- A retreat into an enemy-occupied building creates a separate next-step encounter. Several groups fleeing from the same step to that building join the next-step encounter rather than an ongoing battle.
- A group may retreat back to a building it fought on earlier in the same board turn. There is no retreat-count or repeat-visit cap; the board remains paused while successive encounters continue.
- When the player leaves an unfinished multi-nation battle, show its snapshot first, then simulate the AI-only remainder after the summary closes.
- Contested ownership waits for a valid uncontested occupier. After all required encounters, apply zone damage before eligible recovery and advance the board turn; Bleeding clears only at that turn boundary.
- If the player's last unit dies, finish that player's current Battle, including any AI-only remainder, then finalize once. Skip other registered Battles and any new encounter caused by retreat from that final Battle. For other match-ending events, finish the current event boundary and use the one-time scoring path.

---

# Phase 16 — Combat AI Context and Legal Choices

## Goal

Give each AI nation a fair, read-only view of its current battle and a complete set of legal choices for every decision the Hand 3 engine requests.

This phase establishes legality and information boundaries. It does not yet claim sophisticated tactical weighting or production AI-only resolution.

## Required Work

### Nation-Specific Battle Context

- Build the decision view from the battle state and the nation's own legitimately available knowledge.
- Include its surviving units, their current Resistance, armor, effects, prepared Bow state, and scheduled opportunities to act.
- Include participating opponents and the battle information legitimately revealed to this nation.
- Show which opposing archers are preparing a shot but hide their locked targets from other nations.
- Include retreat eligibility and adjacent destinations filtered through the nation's knowledge, not authoritative hidden occupancy.
- Keep battle facts and strategic observations separate from unrestricted authoritative board queries.
- Refresh the view when a unit dies, a nation withdraws, armor breaks, Bleeding changes, or a prepared shot resolves.

### Legal Action and Defense Generation

- Generate legal attack nations and targets among surviving opponents.
- Generate Sword and Hammer directions, Flail target nation and shield choices, Bow Aim targets and required Fire actions.
- Generate defense choices appropriate to the incoming weapon without revealing its hidden attack direction.
- Offer retreat only when every surviving participant has had a chance to act and a legal adjacent destination is available.
- Do not offer an action by an eliminated unit, target a dead unit, exceed the Flail shield allowance, or change weapon inside a Sword critical chain.
- Revalidate a choice before resolution when earlier actions have changed the battle state.

### Decision Interface and Diagnostics

- Replace the development fixture as the production source of non-player choices, while keeping fixtures available for controlled tests.
- Use the existing Hand 3 action resolver for every resulting choice.
- Keep legal option lists inspectable and log the information visible to the choosing AI.
- Use a controlled seed for any random tie or selection among otherwise legal options.
- Ensure one AI nation's view cannot silently inherit another nation's private observations.

## Out of Scope

- Full tactical utility and target-priority tuning.
- Automatic production AI-only simulation.
- Live board encounter completion and retreat-chain scheduling.
- New Intelligence detection rolls or player Notes.
- AI cheating through hidden board state or Bow targets.

## Exit Conditions

- For every weapon, the AI receives only valid attack and defense options under the current battle state.
- Bow Fire is required on its scheduled archer activation when the locked target survives; death of that target restores a normal choice as designed.
- Retreat is absent before unlock and available afterward only with legal adjacent destinations.
- Enemy Bow Aim targets and hidden adjacent occupancy do not appear in an unauthorized AI decision view.
- Controlled changes to health, armor, deaths, and nation participation update the legal choices before the next decision.
- Scenario fixtures and the AI choice source use the same Hand 3 resolver; no second combat rules path is created.

---

# Phase 17 — State-Based Combat Decisions

## Goal

Make AI nations choose attacks and defenses according to the current battle state, using strong choices consistently and seeded variation when several choices are close.

The AI does not need a perfect solver. Its evaluation must be inspectable, fair, and capable of choosing all four weapons for concrete tactical reasons.

## Required Work

### Attack and Target Evaluation

- Evaluate legal weapons, opposing nations, and individual targets as complete candidate actions.
- Account for visible current Resistance, armor, expected hit or defense outcomes, potential elimination, and the cost of spending an activation.
- Consider Sword critical follow-ups without assuming the defender's hidden future choice.
- Consider Hammer armor breaking after its current hit, including the value of later attacks by allies.
- Consider Flail's target count, shielding possibility, and team-wide damage rather than treating it as a single-target weapon.
- Consider Bow's Aim activation, delayed guaranteed Fire, target-survival risk, and possible Bleeding.
- Make direction choices without seeing the defender's unrevealed response.

### Defense and Shield Evaluation

- Choose legal Sword and Hammer reactions using only the announced weapon and other permitted information.
- Select eligible Flail shield recipients within the ceiling-of-half allowance.
- Value survival of vulnerable or strategically important friendly units without reading the attacker's hidden direction or the unrevealed Flail selection.
- Keep the special one-unit Flail 50/50 rule and weapon-specific defense inaction rules in the engine, not duplicated in AI heuristics.

### Weighted Selection and Tuning

- Prefer a clearly standout action when the evaluated state makes it substantially stronger.
- Give several close strong actions controlled selection weights rather than choosing uniformly or always taking a single highest score.
- Keep exact weights configurable for implementation review and later playtesting.
- Log candidate actions, material utility factors, weights, seed, selected action, and observed outcome.
- Exercise mixed-armored and wounded targets, multiple nations, a nearly eliminated team, Bow preparation, and Flail team-size scenarios.

## Out of Scope

- Retreat policy and destination evaluation, which Phase 18 adds.
- AI-only out-of-view batch execution.
- New strategic movement plans or nation personalities.
- Privileged knowledge of unrevealed attack directions, Bow targets, or hidden board positions.
- Permanent stat progression and difficulty variants.

## Exit Conditions

- AI can make a valid state-based attack and defense decision for every weapon in a controlled two-, three-, or four-nation battle.
- Changing a visible tactical factor such as target health, armor, or team size can change candidate values and selection weights.
- A clearly stronger choice is selected under the approved policy; credible close choices retain seeded variation.
- The same seed and state reproduce the same choices, while different seeds can vary close choices.
- Debug traces show what the AI knew and why it selected the action without exposing forbidden information.
- Controlled scenario runs complete without illegal targets, unavailable defenses, duplicated actions, or special-case combat code outside the shared engine.

---

# Phase 18 — Retreat Policy and Fast AI-Only Simulation

## Goal

Complete combat AI with group retreat decisions and run battles involving only AI nations quickly through the shared engine.

This phase produces trustworthy battle results and retreat requests for the queue, but does not yet connect all live board events or finish an entire encounter chain.

## Required Work

### Retreat Evaluation

- Consider retreat only after the Hand 3 participant-activation unlock condition is satisfied.
- Evaluate whether continuing the battle or spending one unit's activation to withdraw the entire surviving group is stronger for that nation.
- Consider the nation's current losses, remaining opponents, prepared hostile archers it can see, and legitimately known adjacent options.
- Evaluate destination safety, territory, and known threats only from that nation's own observations and public zone information.
- Do not reject an apparently legal destination because a hidden enemy occupies it; the board will resolve that surprise afterward.
- Return the selected destination and all surviving nation members as a single retreat request.
- Allow the battle engine to resolve every prepared hostile Bow shot at locked targets as one simultaneous volley before withdrawal.

### AI-Only Execution

- Drive all participating AI nations' action and defense requests using the Phase 16–18 policies.
- Run the same Agility timeline, weapon resolutions, armor changes, Bleeding ticks, and retreat rules used in a player Battle.
- Skip the full Battle screen, per-action animation waits, and human attack/defense timers.
- Record action count and outcomes so a stalled encounter can be diagnosed during testing without inventing an arbitrary winner.
- Return the surviving units and their condition, eliminated units, winner or continuing nations, and retreat events.
- Do not publish the location, participants, casualties, or result to the player merely because an AI-only encounter resolved.

### Equivalence and Fairness Checks

- Compare a controlled AI-only run with the same action sequence run through the shared engine in visible diagnostic mode.
- Repeat seeds and verify reproducible choices, rolls, activation order, damage, and retreat requests.
- Verify that changing a hidden board position or another AI nation's private Aim target cannot change this AI's decision before a legitimate reveal.
- Verify that AI-only processing completes without human-speed delays while preserving every combat event that affects authoritative state.

## Out of Scope

- Showing AI-only results on the player's board.
- Live player Battle launch and current-step encounter orchestration.
- Applying retreat movement to authoritative board occupancy or scheduling next-step encounters.
- Zone and recovery after the encounter chain.
- Notes, Intelligence detection, permanent progression, and Possession.

## Exit Conditions

- An AI can choose to fight or retreat after unlock for observable tactical reasons and never retreats before unlock.
- A retreat choice uses only known destination information; actual occupancy is left for later authoritative resolution.
- All prepared Bow shots against a withdrawing group fire simultaneously before that group leaves.
- AI-only battles produce accurate survivors, casualties, effects, and retreat requests using the shared Hand 3 engine.
- The same seed and initial state reproduce the same AI-only outcome; no forbidden board information enters a decision.
- An AI-only battle resolves without Battle UI or human-speed waits and does not reveal its result to the player.

---

# Phase 19 — Live Encounter Entry and Current-Step Battles

## Goal

Connect player Battles and fast AI-only fights to the Phase 11 encounter queue for the current encounter step.

This phase makes hot drops and normal contested buildings use real combat. Phase 20 completes retreat-generated later steps, capture, and the remaining board turn.

## Required Work

### Player-Involved Battle Entry

- On a player-selected encounter notification, build the Battle from the actual living units and conditions at that building.
- Use the Phase 14 Battle screen for player attack and defense choices and the Phase 16–18 policy for all AI choices.
- Keep AI-versus-AI exchanges visible and quick when they occur in the player's Battle.
- Return one battle outcome or withdrawal request to the queue when the player-facing segment ends.
- Show the winner summary when resolved, or the accurate unfinished snapshot when the player leaves while AI nations remain.
- Wait for the player to close an unfinished snapshot before Phase 18 simulates the AI-only remainder.

### AI-Only Current-Step Battles

- Start AI-only encounters of the current step as the board pauses, without a full Battle scene.
- Process their outcomes through the same queue and authoritative result contract used for player Battles.
- Keep AI-only location, participants, and outcome hidden unless the player's nation later learns them through an established legitimate event.
- Process AI-only fights in existing encounter registration order; apply each result to participating nations' knowledge and unit condition before a later registered fight asks them to act.
- Allow the player to choose the order of their own pending current-step Battles while AI-only fights are resolved out of view.
- Do not start a next-step encounter until all current-step work is resolved.

### Hot-Drop and Gate Integration

- Launch a player-involved shared deployment battle before Turn 1 planning.
- Keep contested ownership and normal-turn zone, recovery, and advancement blocked while encounters remain.
- Remove every remaining production dependency on the Phase 8 temporary random winner.
- Preserve the result and retreat-event queue for Phase 20 instead of prematurely capturing a contested building or moving a withdrawing group.
- If the player's last unit dies, finish their current Battle before scoring, including its AI-only remainder; do not process other pending Battles or a retreat-created next step afterward.

## Out of Scope

- Complete next-step retreat scheduling and repeated chains.
- Final contested capture and normal-turn resumption after all encounters.
- New player Notes or Intelligence sightings.
- Multi-year winner rewards and Nation History.
- Full art/audio polish.

## Exit Conditions

- A live two-, three-, or four-nation player encounter opens with correct units and conditions, accepts player and AI decisions, and produces the correct result or snapshot.
- A live AI-only encounter uses the same mechanics but no full Battle scene and gives no unintended notification.
- A hot drop blocks normal Turn 1 decisions until its real combat result is available.
- The player can choose among their current-step encounter notifications; the queue does not process step 2 while step 1 remains.
- A remaining AI-only fight starts only after the unfinished player snapshot is closed.
- No live encounter falls back to random winner assignment, stale scripted fixtures, or premature ownership transfer.
- Result events and retreat requests are available for Phase 20 without double application.

---

# Phase 20 — Retreat Chains and Board-Turn Completion

## Goal

Finish live encounter steps, apply retreats and capture to the authoritative board, then resume the normal turn through zone damage, recovery, Bleeding cleanup, and match-end scoring.

This phase completes the first full match loop with fair combat AI and off-screen AI-only battles.

## Required Work

### Retreat Movement and Later Steps

- Move every surviving member of a retreating nation together to the chosen orthogonally adjacent building after the simultaneous prepared Bow volley.
- Preserve current Resistance, armor, and same-board-turn Bleeding; reset rest progress for fighting or retreating units.
- If the destination contains an enemy nation, create a **separate encounter at the next step** rather than joining an ongoing Battle.
- Combine several groups fleeing from the same step to the same building into one next-step encounter.
- Finish every encounter at the current step before starting the next step, regardless of whether player and AI-only battles coexist.
- Recompute retreat unlock independently in every new encounter; further retreats may create more steps.
- Allow a group to revisit buildings fought on earlier that turn and continue the chain without a retreat or visit cap.
- Keep hidden enemy occupancy unknown to the chooser until the move resolves.

### Occupancy, Capture, and Match End

- Update board occupancy exactly once for each eliminated, withdrawn, or surviving unit; prevent duplicate positions and lost units.
- Keep a contested building's previous owner until only one nation occupies it uncontested; preserve ownership if it becomes empty.
- Apply captures from resolved occupation through the established building-ownership system.
- Refresh territory counts and match-end state at their approved boundaries.
- If the player is eliminated, finish their final Battle before finalizing once; skip other pending Battles, any next-step encounter generated by that Battle, and later normal-turn stages. If all AI nations are eliminated, finish the current event invocation and finalize once without processing unrelated pending encounters.
- Preserve the **sole highest final score** as yearly winner and a highest-score tie as no winner, superseding the older survival-first winner wording.

### Zone, Recovery, and Bleeding Boundary

- Once no required encounter step remains, resume the central turn sequence exactly once.
- Apply the Hand 3 proportional zone damage before stationary recovery, including to units on unsafe buildings.
- Do not heal or repair a unit eliminated by its zone tick.
- Preserve the Hand 3 first-idle-turn delay, 20% maximum-Resistance healing or one armor pip, player repair choice, and full-Resistance automatic repair.
- Keep recovery blocked between encounter steps and reset rest progress for any unit that fought or retreated.
- Clear Bleeding only when the board turn advances, not after an individual encounter or retreat.
- Avoid a second zone or recovery application when several encounter callbacks finish during one board turn.

### End-to-End Verification

- Exercise hot drops, multiple independent step 1 encounters, multi-nation fights, AI-only battles, retreat to unknown occupancy, merged next-step arrivals, and repeated chains.
- Exercise a player withdrawal followed by an unseen AI-only continuation, match-ending elimination during a wave, and zone-lethal damage after all battles.
- Test controlled seeds for fairness, repeated event callbacks, correct score ownership, hidden-information leaks, and turn progression.
- Verify the deployment-to-result loop in an Android landscape build without stuck encounter markers or human-speed AI-only delays.

## Out of Scope

- Intelligence detection rolls, learned-intelligence badges, and player Notes; these belong to Hand 5.
- Permanent training rewards, sacrifices, blank replacements, Nation History, and multi-year save flow.
- Possession and its demon-path ideas.
- New strategic AI doctrines, difficulty modes, machine-learning solvers, or hidden-information advantages.
- Final art/audio and exhaustive balance polish.

## Exit Conditions

- Hot drops resolve before Turn 1; normal contested moves pause the board for real combat.
- AI choices in player Battles and AI-only simulations use the same rules and fair information.
- All step 1 encounters finish before step 2; same-step retreats to one building merge at the next step and never join an ongoing Battle.
- A player withdrawal shows the snapshot first; the remaining AI-only battle then resolves silently.
- Bow retreat volleys fire before group movement, and no prepared shot is applied twice.
- Each unit occupies at most one building; contested ownership, capture, and territory counts remain correct after every battle or retreat.
- Health, armor, and Bleeding persist through chained encounters, with Bleeding cleared only on board-turn advance.
- Zone damage and stationary recovery execute in the correct order once after the final encounter step.
- Match-end scoring and winner or top-score tie are finalized once, including early elimination boundaries.
- Complete seeded Android playthroughs finish without placeholder combat results, leaked AI-only information, stuck steps, or premature turn advancement.

---

# Phase Hand 4 Completion Condition

phases_hand_4.md is complete when Phases 16–20 are individually reviewed, locked, and committed.

At that point, the project must contain:

- fair, inspectable AI combat contexts and legal choices for every weapon, defense, and retreat;
- state-based seeded tactical decisions with weighted variation among close strong choices;
- fast, invisible AI-only battles using the shared Hand 3 engine;
- live player Battles and hot drops with AI decisions and accurate summaries;
- orderly encounter steps and chained retreats, including simultaneous prepared Bow volleys;
- correct authoritative occupancy, contested capture, scaled zone damage, recovery, Bleeding cleanup, and match-end scoring;
- repeatable controlled tests and an Android landscape deployment-to-result playthrough.

Hand 5 will develop Intelligence and player Notes. Persistent multi-year progression and Possession remain later scope.
