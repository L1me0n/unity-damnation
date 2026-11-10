# Damnation — Game Design

> **Design authority:** This document is the single source of truth for Damnation's intended game rules as of 2026-11-10.  
> **Platform:** Android-first, landscape, Unity 2D.  
> **Current development state:** Hand 1 (Phases 1–5) and Hand 2 Phases 6–9 are locked; Phase 10 is next. Phases 7–9 are prototype locks. Hand 3 (Phases 11–15), Hand 4 (Phases 16–20), and Hand 5 (Phases 21–25) have draft development plans. Hand 5 adds scouting, historical information presentation, and player Notes after the encounter MVP.  
> **Status vocabulary:** **Decided** is a game rule; **Implemented** describes the current prototype; **Planned** is an agreed direction awaiting implementation; **Open** requires a design decision or balance pass.

## 1. How to use this document

This file defines the game. `phases_hand_1.md` through `phases_hand_5.md` are development plans and acceptance checklists; Hands 3–5 remain drafts. `feature_list.md` records what was actually locked and implemented. The playthrough is a draft of a possible player experience. The earlier READMEs and `backlog.md` are historical inputs, not parallel sources of design authority. When a phase plan or implementation differs from this file, record the difference and decide whether to change the game design or the implementation. Do not silently treat a prototype shortcut as a new rule.

The most recent explicit design decision has priority over older text. **A sole highest final match score wins the year.** Earlier statements that survival first place automatically wins are superseded. If several nations tie for the highest final score, the year has no winner; all tied leaders are exempt from sacrifice, while nations below them sacrifice when multi-year progression is implemented.

### Source reconciliation

| Source supplied for this consolidation | Role and assessment |
|---|---|
| `README(3).md` | Broadest recent design baseline: four nations, four stats, armor, weapon-specific combat, retreat, and persistent history. Its claim that combat is the next hand is out of date. |
| `README(2).md` | Earlier, shorter concept. Its three-stat model, simultaneous combat attacks, restricted retreat, graph-distance zone, and account-level progression are superseded or unconfirmed. |
| Earlier `README(1).md` | Almost the same full design as `README(3).md`; the two differ mainly in their outdated next-hand status lines. |
| `backlog(2).md` | Older design and 36-phase production backlog. Useful for historical intent and compatible scope principles; its claim to be authoritative, old phase numbering, and conflicting mechanics are superseded. |
| `phases_hand_1(3).md` | Approved Hand 1 implementation plan. Read alongside the feature list to see what actually shipped. |
| `phases_hand_2(1).md` | Approved Phases 6–10 implementation plan. Its winner-by-survival statements, globally public exact ownership assumption, Phase 8 decision-context boundary, and pre-turn unresolved-hot-drop requirement are amended by this document and later explicit decisions. |
| `feature_list.md` | Record of locked implementation and prototype deviations. Implementation records do not by themselves redefine intended game rules. |
| `playthrough(2).md` | Draft player journey and interface ideas. Menus, Competitive mode, and XP displays in that draft are not locked requirements. |

## 2. Core identity

Damnation is an offline, single-player, hidden-information tactical strategy game. Four persistent nations compete across an 8×8 board of 64 mechanically identical building nodes. The player's nation is a **Classical Republic**; three nations are AI-controlled. Each fields six individual warriors. One match represents one year. A sole highest-scoring nation wins that year and earns progression. In the later multi-year system, every nation below the highest score sacrifices a warrior; nations tied for the highest score do not.

The central choice is how widely to spread for territory while keeping enough force together to survive encounters and rotate before the zone consumes the board. Unit identities and annual sacrifice give those decisions a persistent cost.

The first release uses one board ruleset, adjacent node movement, four weapon interactions, and fair AI information. There is no online multiplayer, inventory, loot, unit-class system, or unique building bonus in the initial scope. Weapon choices in encounters are combat actions, not equipment inventory.

## 3. Match setup and strategic turns

### Board and deployment — Decided

- The battlefield has eight rows and eight columns. Orthogonally adjacent buildings connect; a unit may hold or move one link up, down, left, or right on a normal turn.
- Turn 0 is for deployment. Two independent rolls choose unavailable deployment lines from the eight rows and eight columns; both rolls may select the same line. These restrictions disappear after deployment.
- Each nation selects one legal starting building and drops all six units there. AI starting buildings are chosen independently and uniformly among legal buildings. A choice does not reserve the building for another nation.
- Starting buildings become public after all four nations commit. Later enemy movements are hidden unless a rule legitimately reveals them.
- Two, three, or four nations can land together. The intended combat system resolves a shared building before normal Turn 1 play; it does not assign an arbitrary winner or uncontested owner.

The Phase 8 prototype registers shared drops as encounters with separate defenders and pending attackers. Its existing temporary random encounter resolver chooses a winner when deployment advances the turn, before the player can issue normal Turn 1 orders. The winner occupies and can claim the building; losing units are eliminated. This temporary resolution enables testing and does not define the eventual combat rule or the final pre-turn encounter boundary.

### Movement and resolution — Decided / Planned

- Friendly units sharing a building form a convenient selectable group but remain individually commandable. Units without an order hold position.
- During planning, orders can be previewed, edited, and cancelled. The current building remains authoritative until the turn resolves; target ghosts are only previews.
- All player and AI orders are committed without access to opponents' unconfirmed choices. Movement then resolves through a central turn sequence. Crossing in opposite directions does not by itself create an encounter under the current design; co-occupation after movement does.
- At the start of each playable board turn, after the previous turn's encounter chains are complete and before anyone issues new orders, resolve the intelligence scouting checks. No extra scouting occurs during mid-turn movement or retreat. After movement, encounters are evaluated. Capture is granted only for uncontested occupation or after an encounter winner is known. Recovery, zone damage, elimination, and match-end checks follow at their appropriate sequence boundaries.
- Encounters pause the board after movement. The normal board turn cannot advance while encounters remain unresolved, including encounters chained by retreats. No normal-turn zone damage or ownership award should be processed as though a contested node had already been resolved. The match-end boundary after eliminating the player or all AI nations remains as described in Section 5.

The current prototype commits one whole-squad movement choice for each active AI before centrally resolving player and AI destinations. Registered encounters use a temporary random winner. If the match continues, it applies zone damage, advances the zone, and refreshes ownership, territory counts, and the HUD. If an elimination ends the match during an encounter or zone-damage callback, that resolution finishes, ownership and points refresh, and scoring runs before later turn stages. Actual combat, team separation, and recovery remain planned.

## 4. Ownership, information, and territory

### Authoritative ownership — Decided

- Every building begins unclaimed. Its owner is either one nation or nobody.
- A nation captures a building when its units occupy it uncontested. A defeated or retreated opponent can also leave a single nation able to claim it after the encounter resolves.
- A claim persists when the units leave, including when the building later becomes unsafe. An uncontested arrival by another nation transfers ownership.
- A shared deployment building or contested destination retains its previous owner until the encounter determines an occupying winner. Merely setting a unit's current node must not change ownership.
- Ownership belongs to authoritative board state. Tile color, corner flags, HUD counters, and AI observations are views of that state, not owners themselves.

### What a nation knows — Decided

- Each nation knows its own units and exact owned buildings. All nations know revealed starting buildings and public zone information.
- Normal enemy movements and orders are private. Intelligence, encounters, retreat, and other legitimate events may reveal enemy observations; a remembered observation is stamped with its turn and does not silently track later hidden movement.
- Retreat destinations show only information the choosing nation has actually learned. An unknown enemy can be waiting at the chosen neighboring building and cause a chained encounter. AI-only encounters elsewhere on the board resolve silently from the player's perspective; they do not reveal locations, participants, casualties, or results merely by occurring.
- The player sees the live count and locations of their own owned buildings. Each AI nation's current territory total is hidden until the final match result. Enemy ownership markers show only the last owner legitimately learned for that building; an unseen ownership change does not update or erase that historical flag. Seeing an AI win a player-involved Battle does not automatically create or refresh an enemy ownership flag. Own territory still reflects actual ownership.
- AI nations use separate knowledge states and the same visibility limits. The authoritative board resolves actions after orders are committed; it must not supply secret decision inputs.

### Ownership presentation — Implemented foundation / Planned completion

The prototype has corner ownership flags and an Owned Buildings toggle independent of movement and zone colors. Flags refresh from current owner state while the toggle is on: player-owned nodes appear, and enemy-owned nodes appear only if the node is in `knownNodes`. The player-knowledge population was commented out during debugging, so the full intelligence flow is not yet in use. A known node still consults its live owner rather than a historical observation; Hand 5 must make the enemy flags last-known without leaking an unseen new owner or a newly vacant building. Unknown enemy ownership is unmarked. No turn stamp appears on the flags. The ordinary tile color continues to communicate movement selection and zone danger. Separate temporary colored count badges reveal enemy deployment locations after the player confirms a drop. They disappear at the next turn transition, or earlier on a node where the player wins an encounter; they do not represent live enemy positions or ownership.

### Player Notes and learned sightings — Planned

Implement the Notes toggle and learned intelligence badges in Hand 5, after the encounter MVP.

- Place a **Note** toggle above **Owned Buildings**. With Notes off, show only learned intelligence badges. A colored `?` means identity known but count unknown; a colored number means both known; a white number means count known but identity unknown. When neither roll succeeds, show no badge. These are historical sightings, not a live tracker.
- With Notes on, the player can see and write their own speculative badges. Tap any building without a visibly present player unit to open a small note menu. Hidden AI occupancy must not block this menu or disclose itself. Choose a non-player nation color and a value of `?` or `1`–`6` using a mobile wheel picker. The badges reuse the colored-count visual language. Manual notes can be edited or deleted in this mode.
- Player-written badges are visible and writable only while Notes is on. For a given building and enemy color, confirmed intelligence replaces and permanently deletes the player-written note; an old guess does not reappear if that intelligence is later cleared. Confirmed intelligence remains available when Notes is off and supersedes the same-color manual note when Notes is on.
- A learned sighting persists without expiry. Seeing the same nation elsewhere does not erase the earlier building's badge: that nation may have split. Directly confirming the original building empty clears its learned sightings. An unfinished player Battle produces a visible snapshot, but does not automatically create a learned intelligence badge; the player can record it using Notes.
- Newer intelligence on the **same building about the same identified nation** replaces that nation's older sighting in full, even if the new observation contains less detail. Other identified nations' older badges on that building remain. A later white count replaces the older white count for that building; a later identified sighting on that building removes the older white badge rather than preserving an ambiguous unknown count. Independent scouts observing the same building during the **same turn** combine their successful identity and count parts before display; neither same-turn result is older.
- Keep observation-turn information internally for recency and same-turn combination, but show **no turn stamps** on learned badges, manual Notes, or ownership flags. Temporary deployment badges remain a separate prototype feature.

### Territory points — Decided

At match end, snapshot every building's authoritative owner. **One owned building equals one territory point.** Vacant and unsafe buildings remain worth a point if owned. A building does not produce a new point each turn or each time its flag is displayed. During play, show only the player's current territory count; enemy totals appear at the final result.

## 5. Elimination, score, and yearly winner

### Match end and survival finish — Decided; prototype ordering specified

- A nation is eliminated from the current match when no units survive. It stops planning, moving, and capturing.
- Record elimination order. The match ends when the player's nation is eliminated or all three AI nations have been eliminated. The prototype completes the current encounter or zone-damage event invocation, then refreshes ownership, territory points, and HUD before calculating the result; it skips later stages of that turn. Pending encounters later in the encounter list are not resolved after the match becomes endable.
- In full combat, if the player's last unit dies in a Battle, finish that entire Battle before scoring, including any AI-only remainder. Then finalize the match; do not play another registered Battle or a new encounter caused by retreat from the final Battle. Capture and territory at scoring reflect only events that actually finished.
- Assign a **survival finish** from fourth to first. If the player is eliminated while AI nations remain, its finish is locked and the surviving AI nations are ordered by surviving-unit count. Equal surviving-unit counts are broken randomly; the current prototype initializes Unity's random source from a serialized, logged match seed before deployment, so identical play sequences can be replayed with the same seed.
- When the player is the last surviving nation, it receives first survival finish. Multiple eliminations during one zone step take their existing death-callback order. Future combat will apply its actual elimination order. No separate simultaneous-elimination rule is required for the Phase 7 prototype.

### Final match score — Decided

| Survival finish | Placement bonus |
|---|---:|
| 1st | 5 |
| 2nd | 2 |
| 3rd | 1 |
| 4th | 0 |

For each nation:

`final score = placement bonus + number of buildings owned at match end`

**A sole highest final score wins the year.** Survival finish awards a bonus and determines when the match ends; it is not automatically the yearly winner. This applies to every nation, including an eliminated nation whose persistent territory puts it ahead. For example, a fourth-finishing nation with 12 buildings scores 12, while a first-finishing nation with three buildings scores 8; the former wins the year. When two or more nations tie for the highest final score, the year has **no winner**. All nations at that top score avoid sacrifice; nations below it sacrifice when that system is implemented.

The eventual result record should keep survival finish, surviving-unit count, placement bonus, territory points, final score, and yearly winner or tie separate. Finalize results once; repeated end checks must not duplicate progression or sacrifices. The player can end a year by being eliminated yet still win on score, or be the final survivor yet lose on score. Phase 7 calculates final scores and tied leaders locally, retains the placement and point lists, and guards its end action against repeats; a complete immutable result record belongs to later match-lifecycle work.

### HUD and results — Prototype / Planned completion

The prototype updates the player's current ownership count on the HUD and shows a basic winner or tie message at match end. Do not label the player's live ownership count as banked match points: final placement bonuses and the ownership snapshot arrive when the match ends. Enemy territory totals are hidden during play. A later result screen should show each nation's survival finish, placement bonus, territory, total score, and sole winner or top-score tie.

## 6. Units, stats, and recovery

Every warrior retains identity, team, current and maximum stats, current and previous building, planned destination, and match alive state. All units start the first year with the same base stats; later upgrades make individuals different. Eliminated units leave the current battle. Sacrifice, not ordinary match elimination, permanently removes a warrior from the persistent roster under the present long-term design.

| Stat or resource | Design role |
|---|---|
| Resistance | Health: provisional first-year maximum 15, increasing by one maximum Resistance per invested training point. The authoritative current value supports fractions; damage persists between encounters during a match, and zero removes the unit from that match. The unit panel rounds its display to a whole number, except a positive value that would round to zero displays `<1`. Survival uses the unrounded value; damage numbers may show fractions. The exact storage type and damage-number precision are implementation choices. |
| Strength | Direct combat damage, separate from raw weapon damage. With `s` invested Strength points, provisional normal Sword raw damage is `7 + 0.7s`, from 7 at zero points to 21 at 20 points. Weapon multipliers use this Strength-scaled value. |
| Intelligence | Adjacent scouting uses separate chances to reveal an enemy team's identity and its exact unit count. Either can succeed without the other; it does not reveal exact stats or health. At `i` invested Intelligence points, the identity chance is `i%` and exact-count chance is `0.4i%`, both zero at base Intelligence. |
| Agility | Continuous shared combat timeline: with `a` invested points, provisional activation frequency is `1 + 0.15a` times the base frequency, evenly from 1× at zero to 4× at 20 points. Both the first activation and later intervals reflect this rate; identical scheduled times use a seeded random tie-break. |
| Armor | Persistent defensive integrity separate from Resistance, with four quarter-pips from 0 to 1. Each remaining pip reduces weapon-hit damage by 16%, for a provisional maximum 64% reduction at four pips. Bleeding and zone damage bypass armor. The 64% value is a starting balance value to test. |

Scouting resolves first at the start of each playable board turn, after any prior-turn encounter chain and before new orders. Each distinct building occupied by one nation's living units makes one check. Use the **highest invested Intelligence** among that friendly group; multiple units at the same building do not each roll. Randomly select one enemy-occupied building among the eight surrounding positions, if any, then roll identity and exact count independently at `i%` and `0.4i%` for `i` invested Intelligence points. No invested points means no scouting successes in the first year. Two separate friendly groups may select the same target and both roll; their successes in that same turn combine. Observations store their turn internally and may contain identity alone, count alone, both, or neither. Nothing is displayed if neither part succeeds. These detection rolls are not yet implemented.

Remaining on the same building for consecutive board turns can restore Resistance or repair armor, including on an unsafe building. The first idle turn gives no recovery; healing or armor repair begins on the **second consecutive stationary turn** and continues on later eligible stationary turns. Moving, retreating, or fighting resets that unit's rest progress. Healing is the default priority; if Resistance is full, armor repair is selected automatically. The player can choose armor repair instead. Each eligible healing turn restores **20% of that unit's maximum Resistance**, capped at its maximum; each eligible armor-repair turn restores one quarter-pip. Zone damage resolves before recovery on the same board turn. A lethal zone tick eliminates the unit before it can heal or repair. Remaining stationary costs the opportunity to claim new buildings and improve position; it is not restricted to safe nodes.

## 7. Encounters and weapons — Designed, not yet implemented

Different nations sharing a building after movement or a hot drop enter an encounter. Two-, three-, and four-nation encounters are possible. The board waits for every pending encounter step to resolve before advancing its normal turn. A player-involved encounter opens the full Battle screen; an AI-only encounter uses the same combat rules and AI choices on both sides, simulated quickly out of view. AI-only encounters at the current step resolve as soon as the board pauses for that step. Their results remain hidden unless a subsequent event legitimately involves or reveals the player's units. When the player leaves an unfinished multi-nation Battle, show its current-state summary; after that summary closes, the remaining AI nations finish the encounter automatically out of view.

The player chooses the order of their pending Battles within the current step by tapping encounter notifications above the corresponding buildings. AI-only encounters in the same step resolve in their existing registration order. A nation can use casualties and other legitimately observed results of an earlier completed fight when it later acts in that step; it learns nothing from a fight it did not observe. Finish **every step 1 encounter** before any step 2 encounter, then finish every step 2 encounter before step 3, and so on. A retreat into a contested building creates a separate encounter at the next step instead of inserting arrivals into a Battle already underway. If multiple groups from one step flee to the same building, they enter one combined encounter at the next step. Further retreats may extend the chain within the same board turn. Which enemy takes each screen position when multiple enemies are present remains open.

### Combat timeline and actions — Decided direction; provisional rate

Agility determines a continuous activation timeline across all surviving participants. At `a` invested Agility points, activation frequency is provisionally `1 + 0.15a` times base frequency; the first activation also comes earlier for faster units. Faster units can act several times before every participant has had one chance. Exact-time ties use seeded random order. On an attack activation, the acting nation chooses one opposing nation, an individual unit target where required, and one weapon. For an individual target, choose its enemy nation first and then that nation's unit; directional attacks choose a direction after targeting. The defender learns the weapon type and chooses the defense that weapon allows before the hidden attack choice is revealed in the quick resolution animation. Only one weapon type is used during an activation, including every extra hit in a Sword critical chain. The older backlog's all-attacks-simultaneous combat is superseded.

The player has a timed attack decision and a shorter timed defense decision; exact seconds will be chosen through mobile playtesting. If the attack timer runs out, the unit's activation ends without acting. If the defense timer runs out, the weapon-specific inaction below applies. Bow Fire has no defender decision. AI choices in a player Battle remain visible through quick attack animations, including AI-versus-AI exchanges; AI-only encounters have no Battle scene.

| Weapon | Attack and defense | Result and defense inaction |
|---|---|---|
| Sword | Attacker secretly chooses Up, Down, Left, or Right; targeted defender chooses one of those or Center/Guard. | Same direction: critical damage is **1.2×** a normal Sword hit and grants another Sword exchange, at most three hits in that activation. The bonus hits stay on the same target; the chain ends if that target dies. Opposite dodge: complete miss. Perpendicular dodge or Center/Guard: normal hit; Guard cannot produce a critical. Defense inaction counts as Center/Guard under these normal rules. |
| Hammer | Attacker and targeted defender each choose Left, Center, or Right. | Matching choices hit; mismatches miss; no critical. A hit deals **0.4×** normal Strength-scaled Sword damage and removes one armor pip (0.25). The armor present before the pip breaks reduces that same hit's damage. Defense inaction counts as Center: an attacking Center hits, attacking Left or Right misses. |
| Flail | Attacker chooses an enemy nation. Randomly select `ceil(surviving target units / 2)` from that nation; the defending nation can shield `ceil(surviving target units / 2)` units before the random selection is revealed. | Each selected and unshielded unit takes **0.25×** normal Strength-scaled Sword damage. With one defending unit, a timely defense gives a 50/50 chance of a hit despite the normal ceiling counts. Defense inaction shields nobody; selected targets are hit, and the one-unit case always hits on inaction. |
| Bow | Aim locks one enemy unit on one archer activation; Fire uses the next archer activation. No defense choice against Fire. | Fire hits with certainty for 1.75× normal Sword damage, cannot critical, and has a 50% chance to add a Bleeding stack. If the locked target dies before the shooting activation, the prepared shot is lost and the archer takes a normal action on that scheduled activation instead. |

Bow preparation identifies the preparing archer to all participants, but only the aiming nation sees the locked target until the shot fires. Each Bleeding stack removes 1% of the bleeding unit's maximum Resistance at the **start of that unit's own activations**, before it chooses an action. Stacks accumulate; Bleeding bypasses armor. Bleeding follows a retreating unit into chained encounters within the same board turn and clears when the board turn finally advances, rather than when a single encounter ends.

### Retreat and encounter chaining — Designed

- Retreat unlocks after **every surviving participant in that encounter has had a chance to act at least once**. Faster units can have acted more often by then. On a subsequent activation of one of the retreating nation's units, choosing retreat replaces that unit's attack and withdraws **all** its surviving participants together to one orthogonally adjacent building, including an enemy-occupied building.
- Prepared hostile Bow shots locked on targets in the retreating group fire at those targets as one **simultaneous volley** before the retreat completes. Every prepared shot at a live locked target is released even if multiple archers target the same unit. This is the only defined free retreat attack.
- Damage, armor, and same-board-turn Bleeding persist; retreat resets rest progress. An enemy-occupied destination starts a separate encounter at the next step. In that new encounter, every surviving participant must again have had a chance to act before retreat unlocks.
- A group may return to a building where it already fought during the same board turn. There is no retreat-count or repeat-visit limit: chained encounters may continue indefinitely until resolved under the ordinary combat and retreat rules. The board remains paused throughout.
- A nation leaves its old encounter when its participants die or escape. A multi-nation encounter continues until only one nation remains or the others retreat; if no player units remain involved, its remainder runs as an AI-only simulation after the player's snapshot summary closes.

For initial balance, a fresh 15-Resistance warrior with four armor pips takes six landed normal base-Strength Sword hits or seven landed base-Strength Hammer hits to defeat. A 20-point Strength specialist needs two landed normal Sword hits on that fresh armored target, or one on an unarmored base target. The 64% full-armor reduction, raw damage, healing, and Agility-rate numbers are starting values to playtest. Combat AI chooses weapons, targets, defenses, and retreats from its legitimately known battle state: choose a standout option when one exists; when several strong choices are close, use weighted randomness among them. The exact evaluation weights, attack/defense timer seconds, and numerical popup precision remain open. Hand 3 and Hand 4 development phase plans are drafted separately.

## 8. Shrinking zone — Implemented foundation

The zone compresses the board and penalizes indefinite hiding. The current Unity prototype uses nested, axis-aligned safe regions with four moving damaging edges. It does not use the graph-distance focus rings in the older backlog. A node made unsafe stays unsafe. Borders warn of a future region before damaging edges advance, and unsafe nodes receive a persistent danger color beneath temporary interaction highlights.

| Stage | Border introduced at turn | Four-turn shrink window |
|---|---:|---|
| 1 | 1 | 5–8 |
| 2 | 9 | 12–15 |
| 3 | 16 | 18–21 |
| 4 | 22 | 24–27 |
| 5 | 28 | 30–33 |

For a base unit with 15 maximum Resistance, the stage's damage rises from one in Stage 1 to five in Stage 5. The intended damage scales proportionally with each unit's maximum Resistance: `stage damage = (stage / 15) × unit maximum Resistance` for stages 1–5. Thus a 35-Resistance unit takes approximately 2.33–11.67 across the five stages. Zone damage bypasses armor and resolves before stationary recovery; eligible stationary units may heal outside the safe area if they survive the tick. The current Unity prototype's fixed 1–5 damage must be updated to this intended rule. Movement resolves before zone damage in the current turn pipeline; future combat resolution must occur before damage is assessed. AI units take the same node-safety-based zone damage as player units. `ZoneBuilder` exposes whether a future safe area has been revealed and whether a building lies completely inside its current borders. The HUD shows turn, stage, countdown, and shrink progress; state transitions have tap-to-dismiss messages. Exact final safe-region dimensions and balance values can be revised through a later explicit balance decision.

## 9. AI strategy — Hand 2 design

All three AI nations use the same rules as the player and independent knowledge states. They know their own unit positions and condition, revealed public information, and their own prior observations. They cannot see hidden player movement, private stats, or unconfirmed orders. Random deployment is independent and uniform among valid buildings; strategic weighting begins with turn decisions.

Hand 2 develops this in stages:

1. **Phase 6, locked:** create three six-unit AI rosters and register them. At that phase boundary, the prototype also assigned preliminary random starting nodes but had not yet integrated AI battlefield occupancy or zone damage.
2. **Phase 7, prototype locked:** capture ownership from resolved occupancy, count territory, track elimination and survival finishes, compute scores, and announce a sole highest-score winner or top-score tie. AI units can enter the encounter/occupancy flow, with a temporary random encounter winner. A complete immutable result record and detailed result UI are deferred. Its older winner-by-survival wording is amended by Section 5 here.
3. **Phase 8, prototype locked:** AI nations independently choose uniformly among legal deployment buildings, including buildings another nation has selected. All confirmed positions are revealed after the player's confirmation. AI deployment and player hot drops enter the encounter registry; the prototype's temporary random resolver settles them before normal player orders. All teams are then affected by authoritative node safety. Each AI keeps its own territory and turn-stamped, source-tagged enemy position, territory, and unit-snapshot observation structures. Public deployment sightings record team, building, and starting count without claiming ownership. Detailed intelligence and encounter observations have prepared update methods but are not called until those systems exist. This lock does not supply a full decision-time information context or relative utility helpers.
4. **Phase 9, prototype locked:** each active AI evaluates holding and its orthogonal neighbors from its own current building. It weights immediate node safety, the revealed future safe area, its claimed buildings, remembered enemy ownership, and the age and observed count of enemy positions. The utility lanes are combined into positive, clamped weights and sampled probabilistically. One selection is assigned to the whole surviving squad; all AIs commit before the central movement and encounter step. The prototype initializes Unity's shared random source from an inspector-set match seed and logs the seed, candidate components and final weights, roll, and selected building. It does not use hidden enemy positions, opponent territory totals, or other teams' unconfirmed orders. Ranking-dependent territory pressure is excluded because the AI does not know opponents' current scores; evaluating onward route options is deferred to Phase 10. A separate decision-context object is optional while the information boundary remains intact.
5. **Phase 10, planned:** evaluate complete team plans, choose whether to group, split, persist in several groups, or regroup, then assign each unit one order. Candidate patterns may include 6, 5+1, 4+2, 3+3, 4+1+1, or 2+2+2. Weights respond to territory, safety, exposure, and support.

The first-match AI may use unit counts without comparing individual stat values because all starting units have equal stats. Later years need stat-aware decisions without replacing the nation/knowledge separation. Strategic choices should favor stronger options probabilistically and retain controlled variation. The Phase 9 prototype supports repeatable choices for the same match seed and sequence of inputs; full combat and future systems must preserve that replay property. Difficulty must not come from privileged hidden information; progression-aware difficulty scaling remains open.

## 10. Years, sacrifice, and national history — Designed for later work

The same four nations persist between years and rebuild six-unit rosters for the next match. A **sole highest-scoring** nation wins the year, increases its victory total and current win streak, and receives at least one training point for unit progression; the exact award formula is open. When the highest final score is tied, there is no yearly winner or winner reward. Each nation **below** the highest final score sacrifices one active warrior, permanently removes that warrior from the roster, and adds a blank replacement with base stats, no allocated training points, and no personal history. Nations tied at the top do not sacrifice. AI nations follow equivalent progression and sacrifice rules; their nomination logic is open. This multi-year progression is not part of Phase 7.

The Classical Republic uses a three-contender lottery when its final score is below the year's highest. Three of its six active roster members are drawn uniformly, including blank replacements; the player chooses one of those three to sacrifice. A unit can receive at most 20 allocated training points across its stats. The provisional gains are +1 maximum Resistance per Resistance point, +0.7 raw Sword damage per Strength point (and corresponding weapon multipliers), +0.15× base activation frequency per Agility point, +1 percentage point identity-scout chance per Intelligence point, and +0.4 percentage point exact-count scout chance per Intelligence point. Additional sources of points, award amounts, and difficulty scaling still require decisions.

Nation History preserves each year's winner or top-score tie, victory totals and streaks, nominees, sacrifice, replacement, active roster, and departed warriors. A historical warrior keeps identity, nation, arrival/departure years, service length, allocated build, combat record, nominations, and final fate.

The older backlog and short README describe account XP and levels as the primary source of training points. The draft playthrough displays XP and a Competitive mode. These are historical or exploratory concepts; they do not override the newer winner-training-point and four-nation history design. Whether XP/account levels also exist is **open**.

## 11. Player experience and presentation

### Current prototype — Implemented through Phase 9

- A generated 8×8 board with mobile tap, pan, zoom, and bounded camera movement.
- Two disabled deployment lines, player drop confirmation, six player units, a central registry, and three initialized AI rosters.
- Player command and decision menus with per-unit selection, adjacent legal highlights, ghost previews, cancellation, and turn confirmation.
- A five-stage shrinking-zone backend with warning borders, danger nodes, damage, stage HUD, and blocking transition messages.
- Independent legal AI deployment, shared-drop encounter registration, and temporary random encounter results. The player sees colored count badges for all enemy deployment buildings, with multiple badges on shared buildings. Badges hide on the next turn or when the player wins on that building.
- AI-specific historical deployment sightings and own-territory tracking, with observation structures prepared for intelligence and encounter information. AI units receive node-safety-based zone damage. Whole-squad AI movement now samples weighted hold or adjacent moves from zone safety, its own territory, and its latest unsuperseded enemy observations. Team separation remains for Phase 10.
- An inspector-set match seed initializes Unity's random source before deployment and is logged. Movement diagnostics log each candidate's component scores and final weight, the random roll, and the selected building.
- Persistent building ownership, current territory counts, an ownership flag toggle, elimination placements, survival bonuses, final-score calculation, and a basic winner/tie message.

### Phase 7 lock — Prototype scope

`OwnershipChecker.CheckOwners` scans actual node occupants after encounter resolution. Pending attackers do not join the occupant list until `MoveWinner` settles the encounter, so the first occupant represents the sole occupying nation at the point of this check. A vacant node keeps its previous owner. Phase 8 hot drops and Phase 9 movement preserve this invariant; future movement changes must continue to preserve it or update the capture logic. The flag toggle refreshes while active and gates enemy markers through `knownNodes`; collection of observations was disabled for debugging, and its live-owner lookup must later become turn-stamped knowledge. Match end uses a one-time guard and shows a basic message. The match seed was added in Phase 9; an immutable four-team result record, polished result UI, multi-year progression, and full match start/end management remain deferred.

### Phase 8 lock — Prototype scope

AI drop locations are stored before the player confirms deployment but revealed only after all four choices are committed. Shared buildings are registered as encounters without adding pending attackers to settled occupancy. The existing random resolver determines their temporary winners as deployment advances to Turn 1; actual combat and a pre-turn hot-drop boundary remain future work. Player hot-drop units avoid duplicate encounter registration, and winners are placed into node occupancy before ownership and scoring refresh. Each AI's public deployment observation stores a building ID and observation turn, so later hidden movement cannot change that record. Its owned-building list follows authoritative ownership, including losses; enemy ownership is not inferred merely from a deployment. Intelligence rolls, detailed encounter reporting, decision-time selection of the latest observations, relative information helpers, and the player-facing historical ownership view remain deferred. This deliberately revises the broader Phase 8 context and hot-drop exit conditions in `phases_hand_2.md` without changing the intended final combat and fair-information rules.

### Phase 9 lock — Prototype scope

On each normal movement turn, each living AI nation evaluates its squad's current building and orthogonally adjacent buildings, scores them using current and revealed future zone information, its exact own claims, and its most recent unsuperseded enemy position or ownership observation at each candidate. Observation age reduces the weight of old enemy claims and gradually restores the weight of old enemy sightings; no live hidden enemy state is consulted. Every candidate receives a positive final weight before a seeded weighted roll, and the chosen destination is given to every living unit in that nation. Holding issues no move. The turn orchestrator obtains all AI orders before MatchManager resolves destinations for player and AI units together; contested destinations enter the existing temporary encounter resolver before ownership, zone damage, and scoring continue. Eliminated AI nations issue no order.

The serialized match seed is applied at match start before deployment. Logs record the seed, candidate utility lanes and final weights, roll, and selected node. The seed reproduces a run when the same sequence of player actions and random-consuming events is repeated; a dedicated AI-only random stream and automated replay harness are not part of this prototype. Enemy territory totals and comparative score ranking remain hidden. Looking one move further ahead is deferred to Phase 10 by explicit decision. The Phase 9 implementation uses one whole squad per AI nation; dividing and regrouping it belong to Phase 10. Intelligence detection and detailed encounter observations remain disconnected until their later systems are implemented. These prototype boundaries amend the broader Phase 9 evaluation and test expectations in `phases_hand_2.md`.

### Player Battle screen — Planned for Hand 3

Use a separate Battle screen with placeholders for up to four nations. The player's nation is at the bottom. With one enemy, place it at the top; with two, use top and left; with three, use top, left, and right. Every unit has a displayed name, including randomly generated names for AI warriors, and its stats and active battle effects are visible on its side.

At the center, a horizontal moving-head activation reel shows `2n + 1` cells: `n` earlier activations, the current activation, and `n` upcoming activations. The left history is initially empty; the head advances to the right as actions resolve. Each cell displays the acting unit's nation color and name. The thematic name of this reel and the displayed value of `n` are undecided. Put battle messages above it. During player activations, action, target, and direction controls sit below it; when the player defends, show the AI's chosen weapon type and the corresponding defense choices below it. Quick animations identify the attacker, target, chosen action/direction when revealed, and outcome. Melee attackers move toward their target; archers remain in their team slot while the arrow travels to its target on Fire. Defenders visibly dodge or raise a shield as appropriate. AI-versus-AI attacks remain visible in a player Battle but resolve quickly.

When a player Battle ends, show a short summary. If the encounter still has AI nations fighting, show a snapshot of its current state; if resolved, show its winner. In both cases list surviving units for each nation, if any, with names, stats, and effects, and list the units eliminated during that Battle. After an unfinished snapshot is dismissed, the AI-only remainder resolves out of view.

### Draft experience — Not locked

The playthrough sketch explores Fight, Roster, Competitive, Tips, and Exit menus; a naming gate for the initial six warriors; a unit roster view; gesture-based confirmations; and a result flow. Keep the useful interaction ideas when designing these screens, but treat their exact menus, wording, XP displays, and Competitive behavior as proposals until separately approved.

## 12. Open decisions and balance work

These open items are recorded here so they cannot be mistaken for contradictory settled rules:

1. **Prototype follow-up:** a dedicated random stream or automated replay harness beyond the Phase 9 match seed, complete four-team result record, polished match-end presentation, and higher-level match start/end management. Top-score ties and the current same-step elimination order are decided for the Phase 7 prototype.
2. **Visibility and Notes (Hand 5):** implement the decided start-of-turn scouting, independent rolls, last-known enemy ownership flags, historical badges, and editable manual Notes. Enemy territory totals remain hidden until the final result. Badge observation turns are stored internally only. The visual layout and fine-grained event integration can be reviewed during implementation without granting new hidden information.
3. **Combat balance and AI:** playtest the provisional base 15 maximum Resistance, 7 raw Sword damage, 0.4× Hammer, 0.25× Flail, 1.75× Bow, +20% Sword critical, 64% full-armor reduction, 20% stationary healing, and linear 1×–4× Agility frequency. AI combat policy is decided in principle, but its exact evaluation weights remain open. Attack/defense timer seconds, fractional damage popup precision, and the storage representation for fractional Resistance remain implementation choices.
4. **Encounter flow and presentation:** the nation-to-position assignment within the top/left/right slots when multiple enemies appear, the activation reel's name and visible half-width `n`, and final art assets remain open. AI-only fights follow encounter registration order; previously observed results can inform later same-step fights. Retreated groups may revisit buildings without a chain cap. If the player is eliminated, finish their last Battle and score without resolving later encounters. Other fine-grained capture and match-end sequencing should be checked against the established uncontested-occupation and one-time scoring rules during integration.
5. **Recovery and zone:** eligible healing and armor repair start on the second consecutive stationary turn, including on unsafe nodes; zone damage scales with maximum Resistance and resolves before recovery. Final numerical tuning and safe-region balance remain open for playtesting.
6. **Progression:** exact winner training-point award, whether a no-winner year resets an existing win streak, whether XP/account levels also exist, AI sacrifice and upgrade choices, and multi-year difficulty scaling.
7. **Other presentation and modes:** final art/audio, roster and onboarding flow, Competitive mode, Tips menu, and polished match results.

### Post-MVP inspiration — not part of the settled scope

Possession is a possible later system: an irreversible commitment of a warrior to a demon path in exchange for special battle abilities and loss of stat progression. Dealer, Entertainer, and Sinful Warrior were brainstormed as paths. Their detailed effects, limits, and interactions are **not decided** and are excluded from Hand 3 design and MVP implementation until revisited explicitly.
