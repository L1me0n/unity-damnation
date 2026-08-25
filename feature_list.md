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
