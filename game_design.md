# Damnation — Game Design

> **Design authority:** This document is the single source of truth for Damnation's intended game rules as of 2026-11-03.  
> **Platform:** Android-first, landscape, Unity 2D.  
> **Current development state:** Hand 1 (Phases 1–5) and Hand 2 Phases 6–8 are locked. Phases 7–8 are prototype locks; Phase 9 is next.  
> **Status vocabulary:** **Decided** is a game rule; **Implemented** describes the current prototype; **Planned** is an agreed direction awaiting implementation; **Open** requires a design decision or balance pass.

## 1. How to use this document

This file defines the game. `phases_hand_1.md` and `phases_hand_2.md` remain the development plans and acceptance checklists; `feature_list.md` records what was actually locked and implemented. The playthrough is a draft of a possible player experience. The earlier READMEs and `backlog.md` are historical inputs, not parallel sources of design authority. When a phase plan or implementation differs from this file, record the difference and decide whether to change the game design or the implementation. Do not silently treat a prototype shortcut as a new rule.

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
- After movement, detection and encounters are evaluated. Capture is granted only for uncontested occupation or after an encounter winner is known. Recovery, zone damage, elimination, and match-end checks follow at their appropriate sequence boundaries.
- A pending encounter pauses later steps that depend on its outcome. No normal-turn zone damage or ownership award should be processed as though the contested node had already been resolved.

The current prototype confirms available orders, resolves registered encounters using a temporary random winner, applies zone damage if the match continues, advances the zone, and refreshes ownership, territory counts, and the HUD. If an elimination ends the match during an encounter or zone-damage callback, that resolution finishes, ownership and points refresh, and scoring runs before later turn stages. AI deployment and temporary hot-drop resolution work; AI movement orders, actual combat, and recovery remain planned.

## 4. Ownership, information, and territory

### Authoritative ownership — Decided

- Every building begins unclaimed. Its owner is either one nation or nobody.
- A nation captures a building when its units occupy it uncontested. A defeated or retreated opponent can also leave a single nation able to claim it after the encounter resolves.
- A claim persists when the units leave, including when the building later becomes unsafe. An uncontested arrival by another nation transfers ownership.
- A shared deployment building or contested destination retains its previous owner until the encounter determines an occupying winner. Merely setting a unit's current node must not change ownership.
- Ownership belongs to authoritative board state. Tile color, corner flags, HUD counters, and AI observations are views of that state, not owners themselves.

### What a nation knows — Decided direction; details planned

- Each nation knows its own units and exact owned buildings. All nations know revealed starting buildings and public zone information.
- Normal enemy movements and orders are private. Intelligence, encounters, retreat, and other legitimate events may reveal enemy observations; a remembered observation is stamped with its turn and does not silently track later hidden movement.
- Exact enemy-owned building markers shown to the player are filtered by what the player has learned. A current public territory total can coexist with hidden *locations* of enemy buildings; the final counters and reveal timing require a UI decision.
- AI nations use separate knowledge states and the same visibility limits. The authoritative board resolves actions after orders are committed; it must not supply secret decision inputs.

### Ownership presentation — Implemented foundation / Planned completion

The prototype has corner ownership flags and an Owned Buildings toggle independent of movement and zone colors. Flags refresh from current owner state while the toggle is on: player-owned nodes appear, and enemy-owned nodes appear only if the node is in `knownNodes`. The player-knowledge population was commented out during debugging, so the full intelligence flow is not yet in use. A known node still consults its live owner rather than a turn-stamped observation; a later player-knowledge UI pass must prevent hidden ownership changes from appearing through this view. Unknown enemy ownership must remain unmarked. The ordinary tile color continues to communicate movement selection and zone danger. Separate temporary colored count badges reveal enemy deployment locations after the player confirms a drop. They disappear at the next turn transition, or earlier on a node where the player wins an encounter; they do not represent live enemy positions or ownership.

### Territory points — Decided

At match end, snapshot every building's authoritative owner. **One owned building equals one territory point.** Vacant and unsafe buildings remain worth a point if owned. A building does not produce a new point each turn or each time its flag is displayed. Current ownership counts can be shown during play as territory standings; they are provisional because ownership can change before the snapshot.

## 5. Elimination, score, and yearly winner

### Match end and survival finish — Decided; prototype ordering specified

- A nation is eliminated from the current match when no units survive. It stops planning, moving, and capturing.
- Record elimination order. The match ends when the player's nation is eliminated or all three AI nations have been eliminated. The prototype completes the current encounter or zone-damage event invocation, then refreshes ownership, territory points, and HUD before calculating the result; it skips later stages of that turn. Pending encounters later in the encounter list are not resolved after the match becomes endable.
- Assign a **survival finish** from fourth to first. If the player is eliminated while AI nations remain, its finish is locked and the surviving AI nations are ordered by surviving-unit count. Equal surviving-unit counts are broken randomly; the current prototype uses Unity's unseeded random source, with a match seed planned for reproducible results.
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

The prototype updates the player's current ownership count on the HUD and shows a basic winner or tie message at match end. Do not label live ownership counts as banked match points: final placement bonuses and the ownership snapshot arrive when the match ends. A later result screen should show each nation's survival finish, placement bonus, territory, total score, and sole winner or top-score tie. A provisional live score can be shown only if labeled as a projection with its assumptions.

## 6. Units, stats, and recovery

Every warrior retains identity, team, current and maximum stats, current and previous building, planned destination, and match alive state. All units start the first year with the same base stats; later upgrades make individuals different. Eliminated units leave the current battle. Sacrifice, not ordinary match elimination, permanently removes a warrior from the persistent roster under the present long-term design.

| Stat or resource | Design role |
|---|---|
| Resistance | Health. Damage persists between encounters during a match; zero removes the unit from that match. |
| Strength | Direct combat damage. |
| Intelligence | Future adjacent scouting uses separate chances to reveal an enemy team's identity and its unit count. Either can succeed without the other; it does not reveal exact stats or health. |
| Agility | Shared turn order for all participants in an encounter; exact initiative formula and action frequency are open. |
| Armor | Persistent defensive integrity separate from Resistance. It changes in quarter-pip steps from 0 to 1; the damage-reduction curve is open. |

The intended intelligence check is once per distinct building occupied by friendly units each turn. Each such group randomly selects one occupied neighboring building among the eight surrounding positions, if any. Identity and exact count are independent rolls; two groups scouting the same enemy building both roll, and a success by either reveals that part of the information. Observations retain their turn and can hold an unknown identity or unknown count. Example increases of one percentage point per Intelligence level for identity and 0.3 percentage points for count were proposed, but the final probabilities and how a group's unit Intelligence contributes remain open. These detection rolls are not yet implemented.

Remaining safely on the same building for consecutive turns can restore Resistance or repair armor. The first idle turn gives no healing; recovery starts on a later eligible idle turn. Moving, retreating, or fighting resets that unit's rest progress. Healing is the default priority; if Resistance is full, armor repair is selected. The player can choose armor repair instead. Each eligible armor-repair turn restores one quarter-pip. Healing amount, exact first repair turn, and zone-damage interaction are open.

## 7. Encounters and weapons — Designed, not yet implemented

Different nations sharing a building after movement or a hot drop enter an encounter. Two-, three-, and four-nation encounters are possible. Every surviving unit has an Agility-ordered combat turn. The acting attacker selects an opposing nation, a unit target when required, and one weapon. The defender sees the weapon before responding; the hidden attack choice is revealed during resolution. The earlier backlog's all-attacks-simultaneous combat is superseded by this design.

| Weapon | Committed interaction | Current result |
|---|---|---|
| Sword | Attacker secretly chooses Up, Down, Left, or Right; defender chooses one of those or Center/Guard. | Same direction: slightly stronger critical and another Sword exchange, up to three hits total in the unit turn. Opposite: complete dodge. Perpendicular or Center/Guard: normal hit, with no critical for Guard. |
| Hammer | Both sides choose Left, Center, or Right. | Match hits; mismatch misses. No critical. Working damage is 25% of normal Sword damage; a hit removes one armor pip (0.25). |
| Flail | Randomly selects about half the target nation's surviving units, rounding up for odd counts; defender shields about half, also rounding up. | Selected unshielded units take low area damage. With one target, attack has a 50% miss chance. The one-target shielding interaction is open. |
| Bow | Aim locks a target on one archer turn; Fire releases on the next. | Fire hits with certainty for 1.75× normal Sword damage, no critical. Each hit has a 50% chance to add one Bleeding stack. |

Bow preparation is visible, but the locked target is visible only to the aiming nation until the shot fires. Each Bleeding stack removes 1% of maximum Resistance per combat turn and lasts until the encounter ends. Tick timing, armor interaction, and rounding are open. A unit does not switch weapons during a Sword critical chain.

### Retreat and encounter chaining — Designed

- Retreat becomes available after one full combat round. It moves all of the nation's surviving participants together to any neighboring building, including an enemy-occupied one.
- Prepared hostile Bow shots aimed at that nation fire at their locked targets before the retreat completes. This is the only defined free retreat attack.
- Damage and armor state persist; retreat resets rest progress. Entering another enemy-occupied building immediately starts a chained encounter and requires another full round before another retreat.
- An encounter ends for a nation when its participants die or escape; a multi-nation encounter continues until only one nation remains or the others retreat.

Exact initiative math, Sword and Flail damage, armor reduction, Bow bleeding edge cases, retreat-volley timing, and AI combat choices remain open. Combat is planned for a later hand after strategic AI foundations.

## 8. Shrinking zone — Implemented foundation

The zone compresses the board and penalizes indefinite hiding. The current Unity prototype uses nested, axis-aligned safe regions with four moving damaging edges. It does not use the graph-distance focus rings in the older backlog. A node made unsafe stays unsafe. Borders warn of a future region before damaging edges advance, and unsafe nodes receive a persistent danger color beneath temporary interaction highlights.

| Stage | Border introduced at turn | Four-turn shrink window |
|---|---:|---|
| 1 | 1 | 5–8 |
| 2 | 9 | 12–15 |
| 3 | 16 | 18–21 |
| 4 | 22 | 24–27 |
| 5 | 28 | 30–33 |

The stage's current damage rises from one in Stage 1 to five in Stage 5. Movement resolves before zone damage in the current turn pipeline; future combat resolution must occur before damage is assessed. AI units take the same node-safety-based zone damage as player units. `ZoneBuilder` exposes whether a future safe area has been revealed and whether a building lies completely inside its current borders. The HUD shows turn, stage, countdown, and shrink progress; state transitions have tap-to-dismiss messages. Exact final safe-region dimensions and balance values can be revised through a later explicit balance decision.

## 9. AI strategy — Hand 2 design

All three AI nations use the same rules as the player and independent knowledge states. They know their own unit positions and condition, revealed public information, and their own prior observations. They cannot see hidden player movement, private stats, or unconfirmed orders. Random deployment is independent and uniform among valid buildings; strategic weighting begins with turn decisions.

Hand 2 develops this in stages:

1. **Phase 6, locked:** create three six-unit AI rosters and register them. At that phase boundary, the prototype also assigned preliminary random starting nodes but had not yet integrated AI battlefield occupancy or zone damage.
2. **Phase 7, prototype locked:** capture ownership from resolved occupancy, count territory, track elimination and survival finishes, compute scores, and announce a sole highest-score winner or top-score tie. AI units can enter the encounter/occupancy flow, with a temporary random encounter winner. A complete immutable result record and detailed result UI are deferred. Its older winner-by-survival wording is amended by Section 5 here.
3. **Phase 8, prototype locked:** AI nations independently choose uniformly among legal deployment buildings, including buildings another nation has selected. All confirmed positions are revealed after the player's confirmation. AI deployment and player hot drops enter the encounter registry; the prototype's temporary random resolver settles them before normal player orders. All teams are then affected by authoritative node safety. Each AI keeps its own territory and turn-stamped, source-tagged enemy position, territory, and unit-snapshot observation structures. Public deployment sightings record team, building, and starting count without claiming ownership. Detailed intelligence and encounter observations have prepared update methods but are not called until those systems exist. This lock does not supply a full decision-time information context or relative utility helpers.
4. **Phase 9, planned:** retrieve the latest permitted observations and public zone and score information when decisions are made; derive candidate-relative information without reading hidden enemy positions or orders. Then move whole AI squads by probabilistically weighting hold and legal neighboring moves using zone, territory, remembered threats, and future options. All AIs commit before any movement resolves. A separate read-only context object is optional if the decision code remains within `AITeam` and preserves the same information boundary.
5. **Phase 10, planned:** evaluate complete team plans, choose whether to group, split, persist in several groups, or regroup, then assign each unit one order. Candidate patterns may include 6, 5+1, 4+2, 3+3, 4+1+1, or 2+2+2. Weights respond to territory, safety, exposure, and support.

The first-match AI may use unit counts without comparing individual stat values because all starting units have equal stats. Later years need stat-aware decisions without replacing the nation/knowledge separation. Strategic choices should favor stronger options probabilistically, retain controlled variation, and support a seeded replay with inspectable candidate weights. Difficulty must not come from privileged hidden information; progression-aware difficulty scaling remains open.

## 10. Years, sacrifice, and national history — Designed for later work

The same four nations persist between years and rebuild six-unit rosters for the next match. A **sole highest-scoring** nation wins the year, increases its victory total and current win streak, and receives at least one training point for unit progression; the exact award formula is open. When the highest final score is tied, there is no yearly winner or winner reward. Each nation **below** the highest final score sacrifices one active warrior, permanently removes that warrior from the roster, and adds a blank replacement with base stats, no allocated training points, and no personal history. Nations tied at the top do not sacrifice. AI nations follow equivalent progression and sacrifice rules; their nomination logic is open. This multi-year progression is not part of Phase 7.

The Classical Republic uses a three-contender lottery when its final score is below the year's highest. Three of its six active roster members are drawn uniformly, including blank replacements; the player chooses one of those three to sacrifice. A unit can receive at most 20 allocated training points across its stats. The stat gains per point, additional sources of points, and difficulty scaling still require decisions.

Nation History preserves each year's winner or top-score tie, victory totals and streaks, nominees, sacrifice, replacement, active roster, and departed warriors. A historical warrior keeps identity, nation, arrival/departure years, service length, allocated build, combat record, nominations, and final fate.

The older backlog and short README describe account XP and levels as the primary source of training points. The draft playthrough displays XP and a Competitive mode. These are historical or exploratory concepts; they do not override the newer winner-training-point and four-nation history design. Whether XP/account levels also exist is **open**.

## 11. Player experience and presentation

### Current prototype — Implemented through Phase 8

- A generated 8×8 board with mobile tap, pan, zoom, and bounded camera movement.
- Two disabled deployment lines, player drop confirmation, six player units, a central registry, and three initialized AI rosters.
- Player command and decision menus with per-unit selection, adjacent legal highlights, ghost previews, cancellation, and turn confirmation.
- A five-stage shrinking-zone backend with warning borders, danger nodes, damage, stage HUD, and blocking transition messages.
- Independent legal AI deployment, shared-drop encounter registration, and temporary random encounter results. The player sees colored count badges for all enemy deployment buildings, with multiple badges on shared buildings. Badges hide on the next turn or when the player wins on that building. Normal AI movement decisions remain for Phases 9–10.
- AI-specific historical deployment sightings and own-territory tracking, with observation structures prepared for intelligence and encounter information. AI units receive node-safety-based zone damage. Selection of decision inputs from those records remains for Phase 9.
- Persistent building ownership, current territory counts, an ownership flag toggle, elimination placements, survival bonuses, final-score calculation, and a basic winner/tie message.

### Phase 7 lock — Prototype scope

`OwnershipChecker.CheckOwners` scans actual node occupants after encounter resolution. Pending attackers do not join the occupant list until `MoveWinner` settles the encounter, so the first occupant represents the sole occupying nation at the point of this check. A vacant node keeps its previous owner. Phase 8 hot drops preserve this invariant; future movement changes must continue to preserve it or update the capture logic. The flag toggle refreshes while active and gates enemy markers through `knownNodes`; collection of observations was disabled for debugging, and its live-owner lookup must later become turn-stamped knowledge. Match end uses a one-time guard and shows a basic message. Seeded random outcomes, an immutable four-team result record, polished result UI, multi-year progression, and full match start/end management are deferred.

### Phase 8 lock — Prototype scope

AI drop locations are stored before the player confirms deployment but revealed only after all four choices are committed. Shared buildings are registered as encounters without adding pending attackers to settled occupancy. The existing random resolver determines their temporary winners as deployment advances to Turn 1; actual combat and a pre-turn hot-drop boundary remain future work. Player hot-drop units avoid duplicate encounter registration, and winners are placed into node occupancy before ownership and scoring refresh. Each AI's public deployment observation stores a building ID and observation turn, so later hidden movement cannot change that record. Its owned-building list follows authoritative ownership, including losses; enemy ownership is not inferred merely from a deployment. Intelligence rolls, detailed encounter reporting, decision-time selection of the latest observations, relative information helpers, and the player-facing historical ownership view remain deferred. This deliberately revises the broader Phase 8 context and hot-drop exit conditions in `phases_hand_2.md` without changing the intended final combat and fair-information rules.

### Draft experience — Not locked

The playthrough sketch explores Fight, Roster, Competitive, Tips, and Exit menus; a naming gate for the initial six warriors; a unit roster view; gesture-based confirmations; and a result flow. Keep the useful interaction ideas when designing these screens, but treat their exact menus, wording, XP displays, and Competitive behavior as proposals until separately approved.

## 12. Open decisions and balance work

These open items are recorded here so they cannot be mistaken for contradictory settled rules:

1. **Prototype follow-up:** seeded random results and replay, complete four-team result record, polished match-end presentation, and higher-level match start/end management. Top-score ties and the current same-step elimination order are decided for the Phase 7 prototype.
2. **Visibility:** which enemy ownership changes become a public event, how long learned tile ownership remains reliable, and the presentation of public territory totals alongside hidden exact locations.
3. **Combat:** Agility initiative and action frequency; Sword values and target changes in a critical chain; armor reduction; Flail single-unit edge; Bow bleeding/armor/rounding and retreat volley timing.
4. **Recovery and zone:** healing amount, armor repair eligibility, zone/rest interaction, final damage and safe-region balance.
5. **Progression:** exact winner training-point award, whether a no-winner year resets an existing win streak, whether XP/account levels also exist, stat increments, AI sacrifice and upgrade choices, and multi-year difficulty scaling.
6. **Presentation and modes:** final art/audio, roster and onboarding flow, Competitive mode, Tips menu, and polished results.
