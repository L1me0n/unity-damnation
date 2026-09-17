### Phase 1 Recap: Battlefield and Mobile Camera Foundation

Goal:

Create the first functional battlefield foundation for Damnation, including the procedural node board, mobile gesture input, and a bounded top-down camera.

Scripts Created / Edited:

- BattlefieldGenerator.cs
- BuildingNode.cs
- BuildingNodePreview.cs
- MobileInputReader.cs
- MobileCameraController2D.cs

Implemented:

- Added a procedurally generated 8x8 battlefield containing 64 building node tiles.
- Battlefield generation is controlled through configurable width and height values.
- Generated tiles are positioned as a centred square grid under a dedicated battlefield parent.
- Added visible spacing/outline treatment so individual node tiles remain distinguishable.
- Added a basic water background around the battlefield.
- Added BuildingNode as the data representation of a battlefield tile.
- Each BuildingNode stores:
  - Unique node ID
  - Row
  - Column
- Added BuildingNodePreview as the visual connection between a generated tile GameObject and its BuildingNode data.
- BattlefieldGenerator stores the connection from each BuildingNode to its generated GameObject.
- BuildingNodePreview exposes the connected BuildingNode, allowing future tile interactions to retrieve node data from the visual object.
- Added MobileInputReader as the central input-reading component.
- MobileInputReader supports:
  - Short one-finger taps
  - One-finger panning
  - Two-finger pinch zoom
  - Mouse click, drag, and scroll-wheel equivalents for Editor testing
- Tap recognition uses a maximum duration and a DPI-aware movement threshold.
- Input that begins over UI is ignored.
- Touches involving more than two fingers are ignored.
- After a pinch ends, the remaining finger is ignored until every finger is released, preventing accidental taps or camera movement.
- MobileInputReader publishes input through TapPerformed, PanPerformed, and ZoomPerformed events.
- Added MobileCameraController2D as a separate subscriber to mobile input events.
- Camera panning converts screen-pixel movement into world-space movement based on the current orthographic size.
- Camera movement follows mobile map-dragging behavior: the camera moves opposite the finger so the battlefield follows the drag.
- Camera zoom is focused around the midpoint of the pinch gesture instead of always using the centre of the screen.
- Camera zoom is limited to an orthographic size range of 8 to 33.
- Camera pan sensitivity is set to 4.
- Camera zoom speed is set to 75.
- Camera bounds are set from X -32 to 32 and Y -32 to 32.
- Camera clamping accounts for the current orthographic size and screen aspect ratio.
- When the visible camera area becomes larger than the battlefield on an axis, the camera is centred on that axis instead of allowing invalid movement.
- The camera starts at the maximum orthographic size of 33, presenting the complete battlefield and surrounding water.
- Final pan, zoom, gesture separation, and camera values were tested through Unity Remote on a mobile device.

Important Technical Note:

Phase 1 separates raw input, camera behavior, node data, and node visuals. MobileInputReader only recognizes gestures and publishes events; it does not move the camera or interact with battlefield nodes. MobileCameraController2D subscribes to the pan and zoom events and converts them into bounded world-space camera behavior. BuildingNode remains a non-MonoBehaviour data class, while BuildingNodePreview connects that data to its generated GameObject. This separation allows the same tap event and node connection to support start-node selection in Phase 2 without placing selection rules inside the input or camera scripts.

Scope Note:

This phase only establishes the functional greybox battlefield, node data foundation, mobile input reader, water background, and mobile camera controls. The current primitive tile and background visuals are intentional development placeholders. Final battlefield art, environmental polish, effects, and other presentation work are deferred. Start-node selection, two disabled starting lines, and their visual states are intentionally deferred to Phase 2.

Result:

Damnation now opens on a complete 64-node battlefield that can be inspected through mobile pan and pinch gestures. The camera provides both a full strategic overview and a bounded close view while preventing movement outside the playable battlefield. Every generated visual tile is connected to persistent node data, and the event-based input foundation is ready to support node selection in the next phase.

Phase 1 Verdict:

**Locked.** ✅ - 08.25.26

---

### Phase 2 Recap: Deployment-Line Rules and Starting-Node Selection

Goal:

Implement the first deployment interaction for Damnation: roll two unavailable battlefield lines, prevent deployment on their nodes, allow the player to choose a valid starting node, and confirm that choice through reusable UI.

Scripts Created / Edited:

- DeploymentSelection.cs
- NodeInteraction.cs
- BuildingNode.cs
- BuildingNodePreview.cs
- ConfirmationMenu.cs
- Notification.cs
- NotificationManager.cs
- BattlefieldGenerator.cs

Implemented:

- Added DeploymentSelection as the controller for the player deployment sequence.
- Deployment begins once and is protected from duplicate initialization through a deploymentStarted state.
- Added two independent random deployment-line rolls.
- Each roll first chooses between row and column and then selects a valid index for that line type.
- Both rolls are intentionally allowed to select the same line.
- DeploymentSelection evaluates the generated battlefield data and identifies every node belonging to either rolled line.
- Added deployment-blocked state to BuildingNode through BlockDeployment and UnblockDeployment operations.
- Blocked deployment nodes receive a distinct dark visual state through BuildingNodePreview.
- Blocked nodes remain interactive so tapping one can explain why it cannot be selected.
- Added NodeInteraction as the bridge between the mobile tap event and battlefield-node interaction.
- NodeInteraction converts the screen-space tap position into world space and uses a node LayerMask with Physics2D.OverlapPoint to identify the tapped tile.
- The tapped BuildingNodePreview is published through OnNodeTapped instead of placing deployment rules inside the input reader.
- DeploymentSelection rejects blocked nodes and publishes OnDeploymentBlocked when one is tapped.
- Valid nodes can be selected and receive a distinct highlighted visual state.
- Added a reusable ConfirmationMenu with configurable text and confirm/cancel callbacks.
- Cancelling selection restores the node's normal visual state and allows another node to be selected.
- Confirming selection stores the chosen BuildingNode as stable board data rather than storing only its visual GameObject.
- Confirmation clears the deployment-only blocked states and visual treatment so those nodes remain normal playable buildings after deployment.
- Confirmation-menu button listeners are removed after each decision so callbacks do not accumulate between uses.
- Added a reusable notification prefab controlled through Notification.
- Notifications use a CanvasGroup coroutine to fade in, remain visible, fade out, and report completion through OnNotificationFinished.
- Reusing a notification stops its previous coroutine and resets its text and alpha before starting a new lifetime.
- Added NotificationManager to subscribe to deployment-blocked events and display the corresponding player message.
- NotificationManager limits the display to three notification objects.
- Available notification objects are stored in a Queue for reuse instead of being repeatedly instantiated and destroyed.
- Active notifications are stored in an ordered List so the oldest notification can be found while any specific completed notification can be removed safely.
- When the three-notification limit is exceeded, the oldest active notification is immediately reused for the newest message.
- Reused notifications are moved to the first sibling position so the Vertical Layout Group presents them in the intended newest-first order.
- Completed notifications become transparent and return to the available pool while retaining their reserved layout space.
- The complete disabled-line, blocked-tap notification, valid selection, cancellation, confirmation, overflow, fade, and pooling flow was tested successfully.

Important Technical Note:

Phase 2 keeps input recognition, physical node detection, deployment rules, node state, presentation, confirmation UI, and notification lifetime in separate components. MobileInputReader continues to publish a generic tap position; NodeInteraction determines which visual node was tapped; DeploymentSelection decides whether the connected BuildingNode is legal; and the UI components only present the resulting request or warning. The notification system also separates active-object tracking from the available-object pool: the active List supports removal by exact object identity, while the available Queue only needs first-in/first-out reuse. Unity sibling order is updated separately because logical notification order does not automatically change the order used by a Vertical Layout Group.

Scope Note:

This phase only implements the player's initial deployment-line restrictions and confirmed starting-node choice. The two line rolls may produce the same line by design. Player-unit spawning, AI deployment, revealing other teams, hot-drop detection, combat, and normal movement turns are deferred to later phases. The current confirmation and notification visuals remain reusable development UI rather than final presentation art.

Result:

Damnation can now begin a deployment sequence, independently roll two unavailable rows or columns, communicate those restrictions visually, and reject blocked selections with pooled corner notifications. The player can select a valid node, cancel and choose again, or confirm it through a reusable confirmation panel. The resulting BuildingNode is preserved for Phase 3, while all deployment-only restrictions are removed from the battlefield after confirmation.

Phase 2 Verdict:

**Locked.** ✅ - 09.01.26

---

### Phase 3 Recap: Unit Foundation and Movement Orders

Goal:

Create the six player units, implement mobile group and individual unit selection, allow adjacent movement orders to be previewed and edited, and resolve the first player-only simultaneous movement turn.

Scripts Created / Edited:

* DeploymentSelection.cs
* BattlefieldGenerator.cs
* BuildingNodePreview.cs
* MatchManager.cs
* UnitManager.cs
* UnitCard.cs
* VerticalSwipeReader.cs
* CommandMenu.cs
* DecisionManager.cs
* DecisionMenu.cs
* TurnManager.cs

Implemented:

* Added six distinct player units that spawn on the starting building confirmed during Phase 2.
* Added MatchManager as the central registry for created units.
* Every registered unit receives:

  * A stable unique unit ID
  * A team ID
* Added UnitManager as the authoritative runtime component for each unit.
* Each unit stores:

  * Unit name
  * Unit ID
  * Team ID
  * Alive state
  * Current building
  * Previous building
  * Planned target building
  * Current and maximum Resistance
  * Current and maximum Strength
  * Current and maximum Agility
  * Current and maximum Intelligence
* Added clamped operations for changing Resistance, Strength, Agility, and Intelligence without exceeding their valid ranges.
* Added Agility as an additional unit stat for later combat turn-order logic.
* Added team-colour presentation for unit heads and bodies.
* Added separate Unit Slots and Preview Slots to every battlefield node.
* BuildingNodePreview separately tracks:

  * Units currently occupying the node
  * Units previewed as moving toward the node
  * Whether the node is occupied
  * Whether the node contains movement previews
  * Which team currently occupies it
  * Whether it is the original node of an active command
* Node occupant queries are encapsulated through BuildingNodePreview rather than exposing the slot hierarchy to the menus.
* Added a two-dimensional battlefield-node collection indexed directly by row and column.
* Added BattlefieldGenerator.GetNode to retrieve a BuildingNode through its board coordinates.
* Added UnitCard as a reusable visual representation of one unit inside selection menus.
* Unit cards display:

  * Unit name
  * Team-coloured icon
  * Resistance
  * Agility
  * Strength
  * Intelligence
  * Selected or unselected background state
* Added CommandMenu for selecting units from a friendly occupied node.
* CommandMenu opens only when a player-controlled occupied node is tapped.
* Between one and six active unit cards are populated from the units occupying that node.
* Every displayed unit begins selected.
* Individual cards can be tapped to include or exclude units from the command.
* Hidden card slots are ignored so stale units cannot enter later selections.
* Confirmed selections are cleared and rebuilt for every command.
* Empty unit selections cannot be confirmed.
* Card event subscriptions are removed whenever the menu closes.
* Added VerticalSwipeReader as a reusable UI gesture component.
* The swipe reader:

  * Tracks one active pointer
  * Moves the panel vertically with the pointer
  * Converts screen-pixel movement through the Canvas scale factor
  * Limits maximum panel travel
  * Rejects horizontally dominant gestures
  * Uses a panel-height percentage as its confirmation threshold
  * Resets the panel before publishing an event
  * Recovers safely if the panel is disabled during a drag
* Swiping CommandMenu upward confirms the selected unit group.
* Swiping CommandMenu downward closes it without creating a command.
* Added DecisionManager to control destination selection after CommandMenu confirmation.
* DecisionManager copies the confirmed unit group into its own active selection.
* Orthogonally adjacent nodes are found through their row and column coordinates.
* Board-edge checks prevent coordinates outside the battlefield from being requested.
* Legal adjacent destinations are highlighted green.
* Only highlighted adjacent nodes can receive the active movement decision.
* A red cross allows the active destination-selection attempt to be cancelled.
* Cancelling destination selection restores neighbour colours and clears the active decision state.
* Confirming a destination assigns that BuildingNode as the selected units' planned target.
* Neighbour collections, selected-unit collections, highlights, and the red-cross state are reset after every completed or cancelled decision.
* Assigning a movement order does not immediately change the unit's authoritative current building.
* The ordered unit is temporarily removed from the source node's commandable occupant collection.
* Its source representation becomes white to communicate that it has already received an order.
* A separate ghost representation appears in the corresponding Preview Slot on the target node.
* Target nodes retain references to the units represented by their ghosts.
* Added DecisionMenu for inspecting and editing units ordered toward a previewed target node.
* DecisionMenu displays the units whose planned target is the tapped node.
* Units remain selected by default, representing orders that will be kept.
* Deselecting a card marks that unit's order for cancellation.
* Swiping upward applies the cancellations to the deselected units.
* Swiping downward closes DecisionMenu without applying its temporary card changes.
* Cancelling a unit's order:

  * Clears its target building
  * Removes its target ghost
  * Returns it to its original node's occupant collection and visual slots
  * Restores its team colour
* Added TurnManager as the initial turn-resolution spine.
* Ending the turn advances the turn counter and publishes the turn-ended event.
* Every UnitManager subscribes to the turn-ended event.
* Units without movement orders remain at their current buildings.
* Units with movement orders:

  * Remove their target ghosts
  * Move their actual visual objects into the destination Unit Slots
  * Store their old current building as their previous building
  * Promote the planned target to their current building
  * Clear the resolved target reference
* Original-node command markers are cleared during turn resolution so they do not persist into later turns.
* All player movement orders are planned before the turn ends and resolve from the same turn-ended signal.
* The complete spawning, card selection, swipe confirmation, adjacent highlighting, ghost preview, order cancellation, red-cross cancellation, and turn-resolution flow was tested successfully.

Important Technical Note:

Phase 3 maintains a distinction between actual unit location and planned movement. UnitManager.currentNode remains the authoritative location throughout planning, while targetNode stores the proposed destination and BuildingNodePreview presents that proposal through separate Preview Slots and preview-unit collections. The actual unit only changes buildings when TurnManager publishes the turn-ended event. CommandMenu handles source-unit selection, DecisionManager handles legal destination selection, and DecisionMenu handles editing orders already represented on target nodes. This prevents UI selection and ghost visuals from becoming the authoritative gameplay state.

Scope Note:

This phase implements the first player-only movement loop. AI teams, hidden information, building ownership, capture, encounters, combat, hot-drops, retreat, regeneration, zone damage, permanent progression, and final presentation remain deferred. The current slot-pointer implementation is accepted for the tested Phase 3 command flow. Validation that prevents unsupported future `MoveUnitHere` call patterns, generalized slot compaction, and more complex occupied-destination behavior will be added when those movement and encounter cases enter the playthrough. The current unit, ghost, card, and menu visuals remain functional development placeholders.

Result:

Damnation now supports a complete player movement turn. Six registered units spawn at the confirmed deployment node, can be selected together or divided into subsets through a mobile swipe menu, can receive legal adjacent movement orders, and display those orders as target-node ghosts without prematurely changing their actual locations. Planned orders can be inspected and cancelled before the turn ends, while turn confirmation resolves the remaining orders, updates unit locations, removes their previews, clears temporary decision state, and prepares the battlefield for the next planning turn.

Phase 3 Verdict:

**Locked.** ✅ - 09.17.26

---


