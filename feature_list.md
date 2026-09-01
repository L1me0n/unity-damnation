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
