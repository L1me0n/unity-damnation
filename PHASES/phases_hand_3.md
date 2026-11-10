# Damnation — Development Phases Hand 3

> **Coverage:** Phases 11–15  
> **Status:** DRAFT FOR REVIEW  
> **Focus:** Combat rules and a controlled, playable player Battle prototype  
> **Platform direction:** Mobile, Android-first, landscape  
> **Engine direction:** Unity 2D

This file proposes the third five-phase production hand for Damnation. It begins from the four-nation board, simultaneous movement, encounter registration, ownership, scoring, zone, and strategic AI work of Hands 1–2. Its goal is to establish the encounter gate, implement combat rules, and make player Battles playable in controlled scenarios.

The current game design defines intended rules. Where an older phase document or temporary prototype behavior disagrees, follow the current game design and record the implementation change. In particular, the **sole highest final match score** wins the year; a highest-score tie has no winner. First survival finish alone does not determine the yearly winner.

Hand 3 does **not** claim that a live match can yet resolve its encounters and advance the board. Controlled opponents follow explicit scenario scripts, not combat AI. Hand 4 adds state-based combat AI, fast AI-only simulation, retreat-chain scheduling, and complete live encounter integration. Intelligence and player Notes follow in Hand 5. Multi-year progression and Possession remain later scope.

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

## Shared Hand 3 Rules

### Intended Encounter Boundary

- Hot drops and contested buildings after simultaneous movement create encounters from authoritative unit and nation state.
- The board pauses before Turn 1 for a hot drop and after movement for normal encounters. It cannot advance while required encounters remain.
- All encounters at one step finish before the next step begins. Retreat to an occupied building creates a separate encounter at the next step; it does not join a battle already underway.
- Player-involved encounters will use the separate Battle screen. Hand 3 exercises it with scripted opponent choices in controlled scenarios; Hand 4 connects it to live match encounters.
- AI-only encounters will eventually use the same rules and AI choices without a visible Battle scene. Hand 3 records and gates them but does not simulate their outcomes.
- A contested building keeps its previous owner until the encounter leaves one uncontested occupier. Match-end scoring uses actual final ownership, not a temporary encounter placeholder.

### Combat State and Decision Boundary

- Each living participant is an individual unit with a stable identity, displayed name, current and maximum Resistance, Strength, Intelligence, Agility, armor pips, and battle effects.
- Base maximum Resistance is provisionally 15. A Resistance point adds 1 maximum Resistance. With s invested Strength points, normal raw Sword damage is provisionally 7 + 0.7s. With a invested Agility points, activation frequency is 1 + 0.15a times base frequency. At most 20 training points can be allocated across one warrior's stats.
- These stat formulas must work for controlled test builds; earning and allocating points across years is later work.
- Current Resistance supports fractions. Armor has four quarter-pips, each reducing weapon-hit damage by 16%. Bleeding and zone damage bypass armor.
- A seeded battle source makes combat rolls and identical-time activation tie-breaks repeatable. A resolution trace should identify the acting unit, submitted action, rolls, damage, and resulting state.
- Use one battle state and action resolver for player and scripted scenario decisions. Build a decision interface that Hand 4 combat AI can use without duplicating the rules. Presentation and human decision timers observe this state; they do not define the outcome.
- Scenario scripts are development fixtures. They must not become a production fallback that secretly plays for AI nations or settles live encounters.

### Phase Boundary Discipline

A phase may expose controlled development scenarios before its player-facing integration is ready. It must not reinstate the Phase 8 temporary random winner, silently settle an AI-only encounter with a script, or advance the board past an unresolved encounter just to keep the prototype running.

Numerical balance values, the exact attack and defense timer seconds, and the activation reel's visible width can be adjusted during implementation review and mobile playtesting without changing the decided rules. Combat AI weights, AI-only simulation, live queue completion, final art, audio, permanent progression, Intelligence, Notes UI, and Possession are not acceptance requirements for this hand.

---

# Phase 11 — Encounter Queue and Board Pause

## Goal

Replace the temporary encounter-result path with an authoritative encounter queue and a reliable pause in the central turn sequence.

This phase identifies which units and nations must fight, preserves contested ownership, and makes the current encounter step inspectable. It does not yet resolve a battle.

## Required Work

### Encounter Registration

- Build encounter records from two or more nations sharing one building after deployment or simultaneous movement.
- Include the building ID, encounter step, participating nations, and the individual living units present.
- Treat a shared starting building as a hot drop that must be resolved before normal Turn 1 planning.
- Reuse or adapt the existing encounter registration without counting a participant twice or placing a pending attacker into settled ownership prematurely.
- Recognize independent contested buildings in the same board turn.
- Preserve each participant's authoritative unit identity, current condition, and nation.

### Queue and Step State

- Put initial hot drops or post-movement encounters in step 1.
- Track which encounters are pending, active, or resolved and which step is currently eligible.
- Allow the player to choose the order of their own pending encounters in the current step by tapping notifications above the relevant buildings.
- Show encounter notifications only where the player's involvement makes them legitimate; do not surface AI-only encounters on the player's board.
- Keep a way to enqueue a later step from a future retreat without allowing that arrival into an encounter already being resolved.
- Do not expose step 2 while any step 1 encounter remains unresolved.

### Turn and Ownership Gate

- Stop the turn sequence before capture, zone damage, recovery, turn advancement, or another later stage treats a contested building as settled.
- Keep the pre-encounter owner of a contested building until a valid result changes occupation.
- Expose a resumption boundary for Hand 4; do not resume the turn while the encounter queue has required work.
- Remove the prototype's random winner and forced elimination from the intended encounter path.
- Keep controlled inspection available while combat is not implemented; an unresolved encounter must remain unresolved rather than receiving a fabricated winner.

## Out of Scope

- Agility scheduling and attacks.
- Battle damage, defense, elimination, and winner selection.
- AI-only simulation outcomes.
- The full Battle screen.
- Retreat and generated next-step encounters.
- Stationary recovery and revised zone damage.
- Notes and post-battle intelligence badges.

## Exit Conditions

- A two-, three-, or four-nation hot drop is registered once and blocks Turn 1.
- Separate contested buildings after movement appear as independent step 1 encounters.
- The player can identify and select only their own pending step 1 encounters.
- AI-only encounters remain invisible from the player's board view.
- No contested building changes owner merely because units arrived.
- Zone damage, recovery, capture of the contested building, and board-turn advancement do not run while an encounter is pending.
- There is no active code path that silently awards a random encounter winner.
- A controlled debug view can inspect the queue and all participant identities without granting that view to the player.

---

# Phase 12 — Battle State, Agility, Sword, and Hammer

## Goal

Build the deterministic battle-state engine and its continuous Agility timeline, then resolve the two individual-target melee weapons.

Controlled battle scenarios use scripted or development-driven opponent decisions while the player Battle screen is built in Phase 14. Hand 4 supplies state-aware combat AI. The engine must already separate action choice from action resolution.

## Required Work

### Battle State and Decisions

- Create a battle from an encounter's living individual participants and nation assignments.
- Maintain current Resistance as fractional authoritative data, maximum Resistance, armor pips, active effects, alive status, eliminated-unit record, and each participant's action history.
- Keep board location and nation ownership authoritative outside temporary battle visuals.
- Expose a common decision contract for the current actor and, where applicable, the targeted defender.
- Require an attack to choose an enemy nation and then a specific unit when the weapon targets one unit.
- Resolve each action through the same rules regardless of whether a scripted test or player UI supplied the choices; leave that same interface ready for Hand 4 AI.
- Prevent eliminated units from receiving later activations or defense decisions.

### Continuous Agility Timeline

- Schedule the first and later activations from the provisional rate 1 + 0.15a relative to base frequency.
- Allow a faster unit to act several times before a slower participant's first activation.
- Resolve activations due at exactly the same time with the match's seeded random source.
- Update the future activation schedule when participants are eliminated or an action changes battle participation.
- Supply a chronological past/current/future activation stream for the later moving-head UI.
- Track whether each surviving participant has had a chance to act, so Phase 13 can unlock retreat at the correct boundary.

### Damage and Armor

- Use provisional base raw Sword damage 7 and +0.7 per invested Strength point.
- Apply 16% weapon-hit reduction per remaining armor pip, up to 64% at four pips.
- Keep fractional Resistance through repeated hits and eliminate at actual Resistance less than or equal to zero.
- Cap healing at maximum Resistance when future effects use it.
- Preserve damage and armor state for later encounters rather than recreating full-health units per battle.
- Expose the actual hit, miss, critical, armor change, and fractional damage as resolution events for UI and diagnostics.

### Sword

- Let the attacker choose Up, Down, Left, or Right; let the defender choose one of those or Center/Guard.
- Matching directions cause a hit at 1.2× normal Sword damage and grant another Sword exchange, with no more than three hits in the activation.
- Keep all extra exchanges on the same target and with Sword; stop the chain if the target dies.
- Opposite dodge avoids the whole hit. Perpendicular dodge or Center/Guard takes a normal hit without a critical.
- Treat defense inaction as Center/Guard. Attack inaction consumes the activation without an attack.
- Resolve each exchange using its own attack and defense choices, including extra hits.

### Hammer

- Let attacker and defender each choose Left, Center, or Right; matching choices hit and all mismatches miss.
- On a hit, deal 0.4× normal Strength-scaled Sword raw damage and remove one armor pip afterward.
- Apply the target's armor reduction **before** that hit breaks the pip.
- Allow a hit against zero armor without reducing armor below zero.
- Treat defense inaction as Center; a Center attack then hits while Left and Right miss.

## Out of Scope

- Flail, Bow, Bleeding, and retreat.
- AI combat strategy and AI-only encounter automation.
- Player Battle screen, attack/defense timers, and animations.
- Permanent training awards and upgrades.
- Full board-turn completion and chained encounters.

## Exit Conditions

- Scripted battles reproduce the same timeline, tie order, rolls, and result with the same seed and choices.
- At 20 invested Agility points, the unit's scheduled activation frequency is 4× the base rate; at 10 points it is 2.5×.
- Fast units can act before a slower unit's first action without a fixed one-action-per-round barrier.
- Six landed normal base-Strength Sword hits defeat a fresh 15-Resistance target with four armor pips; three defeat that target unarmored.
- A max-Strength normal Sword hit defeats a fresh unarmored base target, and two defeat one with full armor.
- Seven consecutive landed base-Strength Hammer hits defeat a fresh fully armored base target; the pip broken by a hit does not change that same hit's reduction.
- Sword matching, opposite, perpendicular, and Center choices produce the specified critical, miss, and normal outcomes; a critical chain cannot exceed three hits.
- Damage, armor, and elimination are recorded once and survive handoff to another encounter.

---

# Phase 13 — Four-Weapon and Retreat Resolution

## Goal

Complete the battle rules for Flail, Bow, Bleeding, and retreat. Use controlled scripted choices to exercise all four weapons and multiple nations without making any opponent behavior a production AI policy.

The engine returns battle events, survivors, eliminations, and retreat requests through a neutral result interface. Hand 4 will connect those results to live board movement and chained encounters.

## Required Work

### Flail and Team Defense

- Let the attacker choose an opposing nation rather than an individual target.
- Randomly select the ceiling of half that nation's surviving participants.
- Let the defender shield the ceiling of half its surviving participants **before** random selection is revealed.
- Apply 0.25× normal Strength-scaled Sword raw damage to each selected, unshielded unit, reduced by that unit's armor.
- Preserve the special one-unit case: with a timely shield choice, the attack has a 50/50 hit chance.
- On defense inaction, shield nobody; the one-unit case then always hits.
- Keep random target selection seeded and inspectable.

### Bow and Bleeding

- Aim at one individual enemy on one archer activation and Fire on that archer's next activation.
- Show the preparing archer to every participant, but keep the locked target private to the aiming nation until Fire.
- Make Fire hit with certainty for 1.75× normal Strength-scaled Sword raw damage, with no critical and no defender choice.
- Roll the 50% Bleeding application on a successful shot using the seeded battle source.
- If the locked target dies before the planned Fire activation, discard the prepared shot and give the archer a normal action on that scheduled activation.
- Apply each Bleeding stack for 1% of the affected unit's maximum Resistance at the **start of its own activations**, before it chooses an action. Stacks accumulate and bypass armor.
- Preserve Bleeding through encounters chained within the same board turn. Expose a board-turn-advance operation that clears it later; do not clear it when a battle alone ends.

### Retreat and Prepared Arrows

- Unlock retreat only after every surviving participant has had a chance to act at least once in that encounter.
- Let a nation choose retreat on one of its unit's later activations; it replaces that unit's attack and attempts to withdraw all its surviving participants together.
- Accept one orthogonally adjacent destination from the board adapter, including one whose enemy occupancy is hidden from the choosing nation.
- Show the choosing nation only information legitimately known about retreat destinations.
- Fire all prepared hostile Bow shots locked on live targets in the withdrawing group as one simultaneous volley **before** retreat completes.
- Consume those prepared shots and avoid firing one again on a later scheduled activation.
- Return surviving withdrawing units and their chosen destination as a result for the future encounter queue; damage, armor, and same-board-turn Bleeding remain on the units.
- Continue a multi-nation battle when two or more nations remain after another leaves.

### Scripted Decision Scenarios

- Provide development fixtures that submit explicit legal choices for attacker, defender, Flail shielding, Bow Aim, and retreat when the battle engine requests them.
- Cover two-, three-, and four-nation scenarios, including attacks between two non-player nations in a player-involved Battle.
- Keep scripts independent from tactical scoring, hidden board state, and the player's unconfirmed choices. They are repeatable test inputs, not an AI implementation.
- Return an encounter result containing remaining named units and effects, eliminations, a winner or continuing state, and any retreat requests.
- Record the chosen scripted actions and random results so Hand 4 can compare future AI-driven outcomes through the same resolver.

## Out of Scope

- Full player Battle screen and human input timers.
- Live board movement to retreat destinations and next-step encounter creation.
- State-based combat AI, production AI-only simulations, and automatic completion of live encounters.
- Player-facing Notes or intelligence badges.
- Multi-year unit training and sacrifice systems.
- Final numerical balance.

## Exit Conditions

- Controlled four-weapon battles use one timeline and one damage/elimination path for every nation.
- Flail ceiling selection and shielding work for odd and even team sizes; one timely defender has a true 50/50 outcome, while one-unit defense inaction guarantees a hit.
- Bow Aim consumes one archer activation, Fire uses the next, targets remain private until Fire, and death of a locked target returns a normal action on the Fire activation.
- Bleeding stacks trigger at the start of the affected unit's own activations, bypass armor, and persist for the rest of the board turn across encounter state transfers.
- Retreat cannot occur before every surviving participant has had a chance to act, and replaces one of the retreating nation's unit actions afterward.
- Several prepared arrows against a withdrawing group fire simultaneously at their locked targets before the group leaves.
- Scripted choices can exercise all four weapons, defense, and retreat for any participating nation through the same resolver; identical seeds and choices reproduce results.
- An outcome or retreat request contains the exact surviving-unit state and casualties needed for Hand 4 without changing live board occupancy in this phase.

---

# Phase 14 — Player Battle Screen and Timed Decisions

## Goal

Make a controlled player-involved encounter playable on an Android-first separate Battle screen, with understandable turn order, named units, responsive timed choices, quick animations, and a short battle summary.

The screen observes the Phase 12–13 battle engine. Its visual timing must not silently alter the authoritative outcome.

## Required Work

### Team and Unit Presentation

- Place the player's nation at the bottom.
- Place one enemy at the top; two at top and left; three at top, left, and right.
- Keep assignment of particular enemy nations to those available slots stable within a battle; finalize the assignment rule during implementation review.
- Show every participating unit's name, nation color, current and maximum Resistance, armor, relevant stats, and in-battle effects.
- Round current Resistance for the unit panel, but display a positive amount that would round to zero as less than 1; let damage feedback show fractional values.
- Generate stable random names for AI units and preserve their identities in battle records. Provide readable names for player units without requiring the later roster-naming flow.
- Refresh unit views after damage, armor breaking, Bleeding, elimination, and retreat.

### Activation Reel and Messages

- Show a horizontal moving-head reel of 2n + 1 cells: n past activations, the current one, and n future activations.
- Begin with empty past cells, then move the head right as activations resolve.
- Show each cell's unit name and nation color; keep future cells synchronized with timeline changes and eliminated units.
- Put concise battle messages immediately above the reel.
- Choose the mobile-readable value of n and a final thematic reel name during UI implementation review.

### Player Attack and Defense

- On a player unit's activation, put the legal action choices below the reel.
- For an individual attack, choose enemy nation, then unit, then the weapon-specific direction where required.
- Provide Flail nation targeting, Bow Aim targeting, and Bow Fire feedback through the same battle action flow.
- When a player unit defends, identify the targeted unit and incoming weapon, then present that weapon's legal defense choices without prematurely revealing the hidden attack direction or another nation's Bow Aim target.
- Let the player choose which eligible units to shield from Flail.
- Once retreat unlocks, offer it as a player action and let the player choose a legal adjacent destination using only known destination information. Controlled scenarios provide the destination set; Hand 4 later applies the move to the live board.
- Give attack choices a longer timer than defense choices; tune the exact seconds on a mobile build.
- Attack timeout ends the acting unit's activation without action. Defense timeout uses Sword Center, Hammer Center, or no Flail shields as defined; Bow Fire offers no defense choice.
- Supply other nations' decisions from the controlled scenario scripts and show their exchanges quickly in a player-involved Battle. Do not present those scripts as combat AI.

### Animation and Summary

- Animate melee attackers moving toward their targets and returning to their positions.
- Keep an archer in its slot while the arrow travels to the target on Fire; do not disclose a private Aim target through presentation.
- Quickly show the defender's chosen dodge or raised shield and the hit, miss, critical, armor break, Bleeding, and elimination outcomes.
- Avoid human-speed delays for scripted non-player actions while keeping each visible choice and target understandable.
- Show a short summary when the player leaves or finishes a Battle: winner when resolved, current-state snapshot when the encounter still has AI participants.
- In either summary, list surviving units by nation with names, stats, and effects, plus units eliminated in that Battle.
- Provide a clear close action; Hand 4 later connects an unfinished snapshot to AI-only continuation and the board.

## Out of Scope

- Final character art, elaborate effects, audio, and fully polished visual identity.
- Player-facing speculative Notes and learned-intelligence badges.
- Permanent roster naming or training UI.
- Strategic AI movement changes from Hand 2.
- State-based combat AI, live player-encounter integration, AI-only simulation, board-wide retreat chains, turn advancement, and final match results polish.

## Exit Conditions

- A player can finish a controlled two-, three-, or four-nation encounter using all four weapon choices and appropriate defenses.
- The screen shows the specified team positions, names, stats, effects, messages, and 2n + 1 activation reel at supported landscape mobile sizes.
- Scripted exchanges between two non-player nations within a player Battle remain visible but resolve quickly.
- Hidden Sword/Hammer attack choices and private Bow Aim targets are not leaked by controls, reel, or animation before reveal.
- Attack and defense timers have the correct relative lengths and timeout outcomes, including the one-unit Flail inaction exception.
- A Sword critical chain prompts and resolves each exchange without changing weapon or target.
- Quick melee, Bow, dodge, guard, damage, and elimination feedback matches actual engine events.
- A resolved Battle shows its winner and all survivors/casualties; an unfinished Battle shows an accurate current-state snapshot.

---

# Phase 15 — Controlled Battle Prototype and Recovery Foundation

## Goal

Finish a repeatable, playable player Battle prototype and define its result handoff for Hand 4. Implement scaled zone damage and stationary recovery for board turns that have no unresolved encounters.

This phase does not use scripted opponents to settle live matches. The production encounter queue remains paused until Hand 4 supplies combat AI and completes live board integration.

## Required Work

### Controlled Battle Entry

- Launch a player-involved Battle from a controlled two-, three-, or four-nation scenario with a fixed seed and explicit non-player decision script.
- Include ordinary and hot-drop participant compositions, existing damage and armor, prepared effects where relevant, and test units with allocated stat points.
- Feed player choices from the Battle screen and scripted choices from the scenario through the same Phase 12–13 decision interface.
- Let controlled battles reach a victory, player elimination, or withdrawal without making the scenario script a production encounter resolver.
- Inspect the order and result of each exchange, including Bow preparation and Fire, Bleeding, critical chains, armor breaks, and retreat volleys.

### Battle Outcome Handoff

- Produce a neutral result containing the encounter identity, surviving unit identities and condition, eliminated units, remaining nations, winner or continuing state, and any withdrawing group with its destination.
- Preserve Resistance fractions, armor pips, and same-board-turn Bleeding in the returned state.
- Show the resolved winner or unfinished snapshot through the Phase 14 summary before reporting that the player has closed it.
- Keep the result separate from authoritative board occupancy, capture, scoring, and match-end calls. Hand 4 will consume it at the encounter boundary.
- Ensure neither a replayed summary nor a repeated result callback can report a second battle outcome.

### Zone and Stationary Recovery

- For uncontested board turns, change zone damage to (stage / 15) × each affected unit's maximum Resistance, preserving 1–5 damage across stages for a base-15 unit.
- Apply zone damage before stationary recovery; do not heal or repair a unit killed by that tick.
- Track consecutive stationary turns per unit. Moving, retreating, or fighting resets rest progress.
- Give no recovery on the first stationary turn; from the second consecutive stationary turn, restore 20% of maximum Resistance or one quarter-pip of armor.
- Allow recovery on safe and unsafe buildings. Heal by default; when Resistance is full, automatically repair armor; allow the player to select armor repair instead.
- Cap health at maximum Resistance and armor at four pips. Keep fractional health authoritative.
- Clear Bleeding on actual board-turn advance in uncontested turns. Preserve a hook so Hand 4 can do the same only after all chained encounters finish.
- Leave zone damage and recovery blocked whenever the production encounter queue is unresolved.

### Mobile and Rule Verification

- Run controlled battles with one, two, and three enemy nations and repeat seeds to check timing, rolls, privacy, results, and summaries.
- Check the player Battle screen at supported Android landscape sizes with touch input, readable stats, and no accidental board interaction.
- Verify that unit state and battle results remain valid for later Hand 4 consumption even when a player leaves an unfinished multi-nation encounter.
- Check uncontested stationary, moving, unsafe, zone-lethal, and full-Resistance automatic-repair cases.
- Keep production encounters paused rather than falling back to a random winner or an unapproved scripted opponent.

## Out of Scope

- State-aware combat AI and live AI-versus-AI simulation.
- Production resolution of player-involved encounters and AI-only encounters.
- Live retreat-chain scheduling, contested capture, and full board-turn resumption after battles.
- Notes, Intelligence presentation, and learned enemy badges.
- Permanent training awards, sacrifice, replacement, Nation History, and Possession.
- Final character art, audio, and complete match-result visual polish.

## Exit Conditions

- A controlled player Battle can be played to a resolved or unfinished summary with scripted non-player choices and reproducible results.
- Two-, three-, and four-nation scenarios exercise every weapon, defense, Bleeding, critical chain, retreat, and simultaneous Bow volley.
- The battle result contains exact survivor condition, casualties, and any retreat request without altering live board ownership or occupancy prematurely.
- A repeated summary close or outcome callback cannot duplicate battle effects.
- Zone damage scales with maximum Resistance, resolves before recovery, and can eliminate a unit before it heals.
- The first stationary turn gives no recovery; later eligible turns heal or repair according to the chosen priority, even outside the safe region.
- Bleeding clears on an actual uncontested board-turn advance, while an unresolved encounter never triggers zone damage, recovery, or turn advance.
- The controlled Battle screen is playable on Android landscape; live encounters remain gated for Hand 4 instead of being settled by the scenario fixture or temporary random resolver.

---

# Phase Hand 3 Completion Condition

phases_hand_3.md is complete when Phases 11–15 are individually reviewed, locked, and committed.

At that point, the project must contain:

- authoritative hot-drop and post-movement encounter records that pause the board;
- a fractional-health, armor, Agility, and four-weapon battle engine with retreat and Bleeding rules;
- a common decision interface for player input, scripted test choices, and future AI choices;
- a playable, timed, named-unit Battle screen with activation reel, quick animations, and resolved or unfinished summaries;
- repeatable controlled battles with two, three, or four nations, including simultaneous prepared Bow volleys;
- precise battle-result handoff data for future live occupancy and encounter-step integration;
- scaled zone damage and stationary recovery on uncontested board turns;
- Android landscape verification of the controlled Battle prototype.

Live combat AI, AI-only simulation, retreat-chain scheduling, contested capture, and complete board-turn resumption after battles belong to Hand 4. Intelligence and Notes belong to Hand 5. Permanent progression and Possession remain separate later scope.
