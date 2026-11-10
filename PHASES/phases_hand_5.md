# Damnation — Development Phases Hand 5

> **Coverage:** Phases 21–25  
> **Status:** DRAFT FOR REVIEW  
> **Focus:** Fair scouting, historical enemy information, ownership visibility, and player Notes  
> **Platform direction:** Mobile, Android-first, landscape  
> **Engine direction:** Unity 2D

This file proposes the fifth five-phase production hand for Damnation. It follows the live encounter and board-turn loop planned in Hands 3–4. Its goal is to make Intelligence a real stat, present learned information without revealing hidden changes, and give the player a separate way to record guesses and Battle observations.

The current game design defines intended rules. Hands 3–4 are draft development plans, and Hand 2 implementation is still underway; this plan describes a future hand rather than claiming those dependencies are already complete. Where earlier prototypes or phase plans show public enemy territory totals, live enemy ownership behind a known-node flag, or a different Intelligence probability, follow the current game design and record the required change. Hand 5 does not add a new source of free information after an unfinished Battle.

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

## Shared Hand 5 Rules

### Knowledge Is Historical

- Authoritative positions, ownership, and unit counts resolve the game. A nation makes decisions from its own units, public facts, and information it legitimately observed, not from the entire authoritative board.
- A learned enemy sighting records its building, known identity and/or exact count, source, and observation turn. The turn is **internal only**. Sightings are historical; hidden movement does not update an old badge.
- Enemy ownership flags show the last owner the player legitimately learned for each building. An unseen owner change or vacancy must not refresh or erase the flag by looking up current authoritative ownership. A witnessed AI victory in a player Battle does not automatically update the enemy flag. The player's own owned buildings and current territory count remain exact.
- Enemy territory totals stay hidden during play and appear with the final match result. No turn stamps appear on flags, learned badges, or player Notes.
- A snapshot of an unfinished player Battle displays its participants and condition under the existing Battle rules. It creates no automatic learned sighting. The player can use Notes to remember what was seen. An AI-only Battle out of view reveals no result to the player merely because it ran.

### Scouting Boundary

- Scouting is the first operation of each **playable board turn**, after prior encounter chains finish and before new orders are selected. Hot drops resolve before the first such check; retreat and movement do not trigger extra checks mid-turn.
- Each distinct building containing a nation's living units makes one check, using the **highest invested Intelligence** in its friendly group. It randomly selects one enemy-occupied neighboring building among the surrounding eight, if there is one.
- With `i` invested Intelligence points, independently roll `i%` to learn identity and `0.4i%` to learn the exact unit count. Both start at zero. Scouting in a first year with no invested Intelligence cannot succeed, though deployment and Battles may provide other legitimate information.
- Separate friendly groups that scout the same building in the same turn roll independently; successful identity and count parts from that turn combine. The result is kept in the scouting nation's own knowledge only.

### Badges and Manual Notes

- Learned badges display a colored `?` for identity only, a colored number for identity and count, or a white number for count only. A failed identity and failed count reveal nothing.
- Newer intelligence for the same identified nation on the same building replaces its entire older sighting, even when less detailed. Different identified nations' historical badges may coexist on one building. A newer white count replaces the older white count on that building. A later identified observation on the building removes an earlier white count so the unidentified badge does not linger ambiguously.
- Learned sightings do not expire. Seeing a nation elsewhere does not clear an older sighting. Direct confirmation that the original building is empty clears its learned sightings.
- Notes are the player's editable guesses, separately stored from learned intelligence. Confirmed intelligence for the same building and nation color permanently deletes and supersedes that manual note. Notes never become evidence for AI decisions or for ownership flags.
- The current temporary deployment badges are separate from learned scouting badges and keep their existing short-lived behavior until deliberately reconciled. They must not become an accidental live tracker.

### Phase Boundary Discipline

Controlled test units may have invested Intelligence points before the permanent multi-year upgrade system exists. This enables the scouting rules to be tested without making point awards, sacrifice, Nation History, or AI progression part of Hand 5. Earlier prototype data structures may be reused, but their existence does not authorize public enemy totals, unrestricted board reads, automatic Battle-to-badge reveals, or silent changes to the decided information rules.

---

# Phase 21 — Nation Knowledge and Ownership Visibility

## Goal

Make historical information the source of enemy-facing board views and AI decisions, and remove the prototype's live ownership and public-total leaks before adding new scouting observations.

This phase establishes the knowledge boundary and existing-event updates. It does not roll Intelligence checks or present new learned scouting badges yet.

## Required Work

### Per-Nation Observation State

- Keep the authoritative building owner, occupants, and unit condition separate from each nation's learned observations and last-known ownership.
- Preserve the observation turn and source internally so later scouting can merge same-turn information and replace older information correctly.
- Let each nation retrieve only its own units, public deployment and zone information, and its own legitimate historical observations when choosing orders or retreats.
- Reuse the Phase 8 observation foundation where suitable without treating a stored building ID as permission to query its current hidden occupants or owner.
- Record direct confirmations through actual permitted events; do not infer an unseen enemy move, capture, or vacancy from the authoritative state while building an information view.

### Ownership Projection

- Show the player's current owned buildings and current territory count accurately through the Owned Buildings view and HUD.
- Show an enemy flag only for a last-known enemy owner legitimately learned at that building. Preserve it across unseen transfers or vacancies until a legitimate later observation changes that knowledge.
- Do not turn a player-witnessed AI Battle victory into an automatic enemy ownership update. Remove or correct a former player-owned flag when that building is no longer owned by the player, without inserting a new enemy flag solely from the Battle result.
- Keep enemy ownership markers and the player's own markers independent from selection highlights, zone danger colors, and learned position badges.

### Territory Visibility and Results

- Hide each AI nation's current territory total from the player throughout the match, including any HUD or standings surface using the earlier public-total assumption.
- Continue using authoritative final building ownership for score calculation. Reveal each nation's territory points in the final result with survival finish, placement bonus, total score, and winner or top-score tie.
- Keep internal AI access to its own current territory exact; do not pass another nation's hidden territory total into strategic movement weights.

## Out of Scope

- New Intelligence scouting rolls.
- Learned scouting badges and manual Notes UI.
- Changes to the four-weapon Battle rules or encounter scheduling.
- Permanent stat upgrades and multi-year progression.
- A new public event that announces enemy ownership changes.

## Exit Conditions

- An unseen enemy capture or abandonment does not change a previously learned enemy flag merely because the Owned Buildings view refreshes.
- A player-witnessed AI victory does not by itself create the winning AI's flag; the player's own live territory and flags remain accurate.
- Enemy territory totals are hidden until the final match result, whose actual scored territory remains correct.
- Independent AI nations receive their own historical observations, never another nation's private knowledge or live hidden ownership as a decision input.
- Public deployment information and legitimate battle observations remain usable without being mistaken for current enemy positions.

---

# Phase 22 — Start-of-Turn Intelligence Scouting

## Goal

Resolve the decided Intelligence check for each nation at the start of each playable board turn and write its results as internal, time-aware observations.

The roll is independent from the eventual badge UI. Controlled upgraded units make nonzero probabilities testable before yearly training exists.

## Required Work

### Board-Turn Scheduling

- Run scouting once at the first operation of each playable board turn, after all prior-turn encounter steps and before the player or AI commits new orders.
- For Turn 1, wait for hot-drop encounters to finish. Do not scout during deployment, mid-turn movement, retreat, or a Battle callback.
- Make the scouting operation repeat-safe if turn entry is invoked more than once; each distinct friendly-occupied building must receive only its intended check for that turn.
- Skip a nation with no living units or a friendly building without any enemy-occupied neighbor.

### Group Selection and Rolls

- Group each nation's living units by building. For a shared building, use only the highest invested Intelligence value in the group; do not sum levels or roll per unit.
- Randomly select one enemy-occupied building among up to eight surrounding buildings for each friendly group.
- Independently roll identity at `i%` and exact count at `0.4i%` for that group's `i` invested points. At zero points, both probabilities are zero. At 20 points, they are 20% and 8%.
- Use the match's controlled random source so target selection and both rolls can be inspected and reproduced with the same state and seed.

### Same-Turn Observations

- Store an observation with internal turn/source information only when one or both parts succeed; preserve the possibility of identity without count or count without identity.
- Let separate friendly groups roll independently when they choose the same enemy building.
- Combine successful parts from such scouts in the **same turn** before presenting or superseding a prior-turn sighting; neither same-turn observation counts as older.
- Put each result in the scouting nation's own information state, never in a shared global enemy-knowledge pool.

## Out of Scope

- Displaying learned badge combinations or replacing old badges.
- The player's manual Notes menu.
- Intelligence point awards, purchases, and AI multi-year upgrades.
- An extra detection check after movement, retreat, or an AI-only Battle.

## Exit Conditions

- Scouting occurs before any new-turn orders, exactly once per eligible friendly-occupied building, and never during a chained encounter.
- A shared group uses its highest Intelligence; splitting into two occupied buildings can create two independent checks.
- The target comes from occupied enemy neighbors in the eight surrounding positions; an empty neighborhood produces no target and no disclosure.
- Controlled rolls verify zero at base Intelligence, independent identity and count outcomes, and the 20-point chances of 20% and 8%.
- Two same-turn scouts can produce a combined identity-and-count observation without one discarding the other's successful part.
- Identical controlled states and seeds reproduce selected targets and observation outcomes.

---

# Phase 23 — Learned Sightings and Historical Badges

## Goal

Convert legitimate scouting observations into persistent, readable board badges while respecting partial knowledge, recency, and direct empty-building confirmation.

This phase presents learned information. The player-authored Notes layer follows in Phase 24.

## Required Work

### Observation Reconciliation

- For the same building and identified enemy nation, replace the entire older sighting with a newer one; do not carry an old exact count into a newer identity-only sighting.
- Keep older badges for other identified nations on that building when a newly observed nation is different.
- Replace an older unidentified white count on a building with a newer white count there.
- When a later observation identifies an enemy on that building, remove the older white count rather than retaining an ambiguous unidentified badge; preserve other identified nations' badges.
- Use internal observation turns to distinguish newer information from same-turn complementary successes without displaying turn metadata.
- Clear learned sightings on a building when that building is directly confirmed empty. Do not expire them merely because time passes or a nation is seen elsewhere.

### Board Badge Presentation

- Show a colored `?` when identity alone is learned, a colored `1`–`6` when identity and exact count are learned, and a white `1`–`6` when only count is learned.
- Show no learned badge for a check in which both rolls fail.
- Present learned badges when Notes is off, separated visually from owner flags and the temporary post-deployment count badges.
- Keep the badge readable on an Android landscape board with one or more identified nations' historical badges on the same building, and never show an observation turn stamp.

### Event Boundaries

- Do not create a learned badge from an unfinished player Battle snapshot; its information remains something the player may manually note.
- Do not expose AI-only encounter participants or outcomes to a player who did not legitimately see them.
- Keep historical badges fixed through hidden enemy movement and unseen captures. A direct empty confirmation can clear them even when the building's last-known ownership flag remains historical.

## Out of Scope

- Player-authored notes, editing, and the Note toggle.
- Live enemy radar or expiration timers.
- New public Battle outcome announcements.
- Changing scouting probabilities or encounter visibility.

## Exit Conditions

- The three partial-knowledge forms display correctly, and two failed rolls display nothing.
- An older white `3` followed by a newer white `2` on the same building displays white `2`, not both.
- An older white `3` followed by an identified Blue `?` displays Blue `?` without the old white count.
- An older Red `3` followed by Blue `?` keeps both identified nations' historical badges; a newer Red `?` then replaces Red `3` rather than retaining its old count.
- Same-turn complementary rolls combine into a single colored count; internal turns never appear in the UI.
- A sighting remains across later turns and sightings elsewhere, but direct confirmation of its building as empty clears its learned badges.

---

# Phase 24 — Editable Player Notes

## Goal

Let the player mark uncertain enemy information on buildings without turning a manual guess into a confirmed sighting or a leak from hidden occupancy.

Use the same colored-number visual language while keeping the player's entries distinct in state and visibility from the learned badge layer.

## Required Work

### Notes Toggle and Menu

- Place the **Note** button above **Owned Buildings**.
- When Notes is on, allow a tap on any building without a visibly present player unit to open a small note menu. Do not use hidden AI occupancy to disable the tap or change its response.
- Offer a non-player nation color and a mobile wheel picker with `?` and `1`–`6` as values.
- Let the player create, edit, or delete a note for a building and enemy color while Notes is on.
- Show player-authored notes only while Notes is on; do not show their internal creation or modification times.

### Confirmed Intelligence Interaction

- When confirmed intelligence arrives for the same building and enemy color, delete that player-authored note permanently and show the confirmed observation instead.
- When Notes is on, confirmed intelligence supersedes a note of the same color; when Notes is off, show the confirmed learned badges without the manual layer.
- If a learned badge later clears after direct empty confirmation, do not resurrect the deleted manual note.
- Keep manual notes out of AI knowledge, ownership determination, territory counts, scouting target selection, and score calculation.

### Input and Readability

- Keep Notes interactions distinct from the existing building command, encounter notification, zone, and Owned Buildings controls.
- Support multiple nation-colored notes on a building without implying those guesses are current or confirmed.
- Preserve the ability to use Notes to record a player Battle snapshot after leaving the Battle screen; no automatic note is generated.

## Out of Scope

- Sharing manual notes with an AI nation.
- Automatic note creation from Battles or scouting failures.
- Visible turn stamps, note expiry, or handwriting/free-text fields.
- A redesign of player movement commands or combat controls.

## Exit Conditions

- The Note toggle sits above Owned Buildings and permits creation, editing, and deletion only while active.
- A building with hidden AI occupants responds to note input the same way as an apparently empty building, unless a player unit is visibly present.
- Manual badges disappear when Notes is off; learned badges still show. A confirmed same-color observation removes its old manual note permanently.
- The player can manually record an unfinished Battle observation, and doing so changes neither AI knowledge nor authoritative board state.
- Notes remain usable on the Android landscape board without misfiring an encounter or movement command.

---

# Phase 25 — Knowledge Integration and Match Verification

## Goal

Connect scouting, remembered sightings, ownership flags, and Notes to the live four-nation match and verify that every nation acts only on information it has earned.

This phase closes the Hand 5 loop across deployment, turn entry, encounter chains, AI decisions, board UI, and final results. It does not add the later yearly progression system.

## Required Work

### Fair Decision Inputs

- Feed each AI nation's permitted scouting and encounter observations into its existing strategic decision context without replacing remembered facts with current hidden board values.
- Preserve independently learned information for each AI nation. A scout success for one nation is not automatically known to the other two or the player.
- Ensure an observation can inform later decisions while remaining historical; a newer same-building, same-nation result supersedes an older one under the badge rules.
- Maintain the Hand 4 rule that only actual participants and legitimately informed nations can use a Battle's outcome. Do not reveal an AI-only simulation to the player just because it resolved.
- Keep manual Notes private to the human interface; they do not alter AI choices or any nation's formal knowledge.

### Live Turn and UI Integration

- Exercise hot drops, Turn 1 scouting, movement, player Battles, AI-only encounters, retreat chains, and subsequent turns in the real board pipeline.
- Ensure all pending encounters finish before the next playable turn's scouting and that scouting precedes that turn's planning.
- Reconcile temporary deployment badges with persistent learned badges so the former still vanish at their existing boundary without deleting an independent learned sighting.
- Refresh own live ownership and territory while leaving unseen enemy flags and enemy totals hidden.
- Present actual enemy territory and score details only in the final result; keep existing sole-highest-score and top-score-tie rules.

### Verification and Documentation

- Use controlled seeded scenarios with upgraded Intelligence test units to inspect target selection, partial rolls, same-turn merges, newer observation replacement, and divergent nation knowledge.
- Check an AI capture out of view, a player-observed AI victory, a directly confirmed empty building, a Battle snapshot followed by a manual Note, and a chain that ends before the next scout check.
- Check that UI refreshes and strategic AI candidate evaluation do not query live hidden occupancy, ownership, or enemy totals through a known building reference.
- Review the Android landscape board for flag, temporary deployment badge, learned badge, and Note readability and tap priority.
- Record any implemented deviation and update the feature list and design documents during phase lock.

## Out of Scope

- Yearly award amounts, unit training allocation, sacrifice, blank replacements, and Nation History.
- AI progression-aware difficulty scaling and a new strategic doctrine.
- Possession, final art/audio, and exhaustive combat or zone balance work.
- Additional intelligence sources or public ownership announcements not already decided.

## Exit Conditions

- Every playable turn scouts once before orders and only after any prior encounter chain completes; live battles do not trigger mid-turn scouting.
- The player sees their own exact holdings, historical learned enemy flags and badges, editable Notes in Note mode, and no live AI territory totals before the result.
- Three AI nations can hold different, internally time-stamped knowledge of the same building and choose from only their permitted views.
- Newer partial sightings, white-count replacements, same-turn merges, different-nation coexistence, and direct empty confirmation obey the decided rules.
- Unfinished player Battles and unseen AI-only encounters do not create free player badges or updated enemy owner flags.
- Seeded Android landscape playthroughs finish with correct authoritative ownership, final scores, and no hidden-information leak through decisions or UI.

---

# Phase Hand 5 Completion Condition

phases_hand_5.md is complete when Phases 21–25 are individually reviewed, locked, and committed.

At that point, the project must contain:

- nation-specific historical observations and last-known enemy ownership presentation;
- hidden in-match AI territory totals and accurate final territory scores;
- start-of-turn Intelligence checks with independent identity/count rolls and seeded same-turn combination;
- persistent partial-knowledge badges with the decided replacement and direct-empty rules;
- editable player Notes that cannot disclose hidden occupancy or contaminate AI knowledge;
- a verified deployment-to-result board loop that keeps scouting, encounters, ownership, and information boundaries in order.

Multi-year progression, Nation History, and Possession remain outside this hand.
