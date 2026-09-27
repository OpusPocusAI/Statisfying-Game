You are the lead Unity developer responsible for building a complete, polished Version 1 of a first-person casual satisfaction game called Sea Glass Studio.

Work autonomously and continue through implementation, testing, repair and final verification. Do not stop after producing a plan. Only ask me a question if progress is genuinely blocked by something requiring my authorization or an unavailable dependency.

PROJECT ENVIRONMENT

* Active project: C:\Users\Admin\UnityProjects\SeaGlassStudio
* Unity version: 6000.3.16f1 LTS
* Render pipeline: Universal Render Pipeline
* Target platform: Windows PC
* Git repository: https://github.com/OpusPocusAI/Statisfying-Game
* MCP for Unity is installed and connected.
* Unity ProBuilder is installed.
* Use the existing project rather than creating another project.
* Do not update Unity, MCP for Unity or installed packages unless absolutely required.
* Do not download paid or unlicensed assets.
* Preserve existing user files and inspect the current Git status before editing.

UNITY MCP WORKFLOW

1. Verify the active project, scene, installed packages and current Console state.
2. List the available MCP tool groups.
3. Activate additional tool groups only when required:

   * probuilder for custom geometry
   * vfx for shaders, particles and procedural textures
   * animation for animation clips
   * ui for runtime UI
   * scripting_ext for ScriptableObjects
   * testing for automated tests
   * profiling for the final performance review
4. Do not expose every tool group throughout the entire task unnecessarily.
5. Work sequentially. After every major milestone:

   * Save all affected scenes and assets.
   * Allow Unity to compile.
   * Inspect and fix every new Console error.
   * Enter Play Mode and test the implemented flow.
   * Do not continue while compilation errors remain.

GAME PITCH

Sea Glass Studio is a first-person cleanup, progression and crafting game.

The player explores a stylized shoreline, gathers rubbish and cloudy sea glass, loads everything into a small utility truck, drives back to a seaside studio, recycles the rubbish, washes and sorts the glass, and places it into glowing mosaics.

The emotional and visual transformations are:

* Polluted shoreline to clean shoreline
* Cloudy glass to colourful polished glass
* Disordered pieces to neatly sorted trays
* Empty mosaic to illuminated artwork
* Neglected studio to warm restored studio

TARGET SCOPE

Create a polished, self-contained Version 1 offering approximately 30–45 minutes of gameplay.

It must contain:

* One continuous compact coastal environment
* A shoreline with three progressively unlocked collection sections
* A short coastal road
* A seaside studio
* First-person walking and interaction
* A manually drivable utility truck
* Rubbish and sea-glass collection
* Physical truck loading and visible cargo capacity
* Recycling, cleaning, sorting and mosaic stations
* XP progression
* Spendable bottle-cap currency
* Equipment purchasing and upgrades
* Approximately 30 sea-glass pieces
* Six glass colours and roughly five general shapes
* Three complete mosaics
* Save and load
* Main menu, pause menu and settings
* Tutorial guidance
* Final completion sequence
* Sound and visual feedback
* A playable Windows build if the environment supports building

Do not build an open world. Keep every area compact, attractive and intentionally composed.

ART DIRECTION

Create an original stylized 3D coastal world inspired by colourful painterly game illustrations.

The visual language should use:

* Simplified, readable 3D geometry
* Bold colour blocking
* Painterly gradients
* Clean silhouettes
* Slightly exaggerated proportions
* Angular, stylized shadows
* Warm orange sunlight contrasted with turquoise water
* Soft atmospheric distance
* Minimal photographic texture
* No heavy outlines
* No gritty realism
* No excessive environmental clutter

Maintain a carefully controlled late-afternoon or golden-hour atmosphere. Do not implement a dynamic day-and-night system.

Suggested palette:

* Turquoise, cyan and deep teal ocean
* Pale yellow, coral and peach sky
* Warm cream and muted-orange sand
* Burnt-orange wood
* Dark-purple shadow tones
* Deep blue-green vegetation
* Aqua, emerald, cobalt, amber, clear white, purple and rare red glass

Use ProBuilder, simple custom meshes, primitives, generated materials and procedural or hand-authored gradients. Build cohesive stylized placeholders that can function as Version 1 artwork rather than leaving obvious gray-box geometry.

Most surfaces should be matte or low-smoothness. Sea glass is the main visual exception and should have attractive translucency, frosted surfaces, edge highlights and subtle emission.

FIRST-PERSON PLAYER

Implement responsive mouse-and-keyboard controls:

* WASD movement
* Mouse look
* Shift to sprint
* Space to jump if appropriate
* E to interact, operate stations and enter or exit the truck
* Left mouse button for the primary equipped-tool action
* Right mouse button for an alternate action when applicable
* Mouse wheel or number keys to switch available tools
* Escape for pause

Use Unity’s current Input System if it is already available and stable.

Include:

* Adjustable mouse sensitivity
* Optional invert-Y
* Sensible collision and slope handling
* A small interaction reticle
* Contextual interaction prompts
* Subtle first-person hands or tool models
* No mandatory head bob
* No complex character body simulation

SHORELINE GAMEPLAY

The player collects rubbish and sea glass directly from the shoreline.

Create three compact beach sections unlocked progressively. Populate them with readable combinations of:

* Plastic bottles
* Cans
* Small metal rubbish
* Bottle caps
* Driftwood
* Other simple coastal debris
* Cloudy sea-glass pieces
* Small sand or debris piles

Do not simulate deformable sand. Partially bury objects by positioning them within static sand meshes.

Starting gathering equipment:

* A deliberately weak toy-like hand shovel
* A small bucket with limited capacity

The player should be able to:

* Dig or scoop within a small targeted area.
* Reveal partially buried objects.
* Pick up visible objects.
* Collect groups of nearby objects with upgraded tools.
* Carry a filled bucket or container back to the truck.
* Place rubbish and glass into separate truck compartments.
* See and hear objects enter the truck.
* See the cargo load increase physically and through a simple capacity indicator.
* Return before the truck is full.
* Lose the ability to collect additional cargo when full, with a clear suggestion to return to the studio.

Prevent objects from becoming permanently lost. Add recovery logic for objects that fall outside the playable area.

TRUCK AND TRANSPORT

Create a charming, slightly exaggerated utility pickup or beach-cleanup truck.

Implement:

* Entering and exiting using E
* First-person driver-seat camera
* Simple arcade steering and acceleration
* Forward, reverse and braking
* Reliable road collision
* A short 30–60-second route between shoreline and studio
* No traffic, pedestrians, fuel or realistic damage
* Natural scenery and barriers keeping the player on the intended route
* Visible cargo in separate rubbish and glass containers
* Cargo-rattle audio and small visual movement
* A small suspension or body response as cargo increases
* Reset-vehicle functionality if overturned or stuck

After the route has been driven successfully once, unlock an optional Skip Drive interaction. Manual driving must remain available.

STUDIO WORKFLOW

The seaside studio contains four physical stations.

1. Recycling station

* Accept collected rubbish.
* Play a short satisfying processing animation.
* Award XP and bottle caps.
* Remove processed rubbish from the truck.
* Give clear audiovisual confirmation.

2. Cleaning station

* Place cloudy glass into a basin or working tray.
* Begin with a brush and water bowl.
* Use hold-to-clean interaction.
* Gradually change a grime value from dirty to clean.
* Reveal stronger colour and translucency continuously.
* Add foam, water particles, wiping feedback and layered sounds.
* Prevent cleaning from becoming pixel-perfect or frustrating.

3. Sorting station

* Allow cleaned pieces to be picked up and moved in first person.
* Provide broad colour trays with generous trigger areas.
* Correct sorting should produce a soft snap, small bounce and pitched glass note.
* Incorrect sorting should give gentle feedback without punishment.
* Later upgrades may attract compatible pieces or increase snapping assistance.

4. Mosaic station

* Let the player select cleaned pieces and place them into authored physical sockets.
* Compatible nearby sockets should highlight gently.
* Use forgiving placement.
* Retain the actual colour and form of every placed glass piece.
* Add dark grout or framing around installed pieces.
* A placed piece should remain part of the persistent mosaic.

The workbench interaction may temporarily constrain player movement and bring the camera closer, but it must remain visually first person and return cleanly to free movement.

MOSAICS

Build three authored panels forming one large seaside triptych:

1. Ocean Wave
2. Lighthouse
3. Coastal Sunset

Use approximately 30 total sea-glass pieces, divided sensibly across the three panels.

Complete the Ocean Wave system first and verify its entire flow before using the reusable mosaic system to construct the other two.

Every placed piece should make the image more recognizable. Completing a panel must trigger:

* Final piece snap
* Harmonic audio resolution
* A short pause for anticipation
* Grout or connecting lines completing
* Increasing glass emission
* Warm light shining through the panel
* Coloured light appearing across the studio
* Camera emphasis on the completed artwork
* A substantial XP reward
* Progress towards restoring the studio

Completing all three panels triggers the final sequence in which the full triptych illuminates and the studio visibly changes from neglected to restored.

PROGRESSION AND ECONOMY

Use two separate progression systems.

Experience:

* Permanent and never spent
* Earned from collecting rubbish
* Earned from discovering glass
* Earned from cleaning, sorting and placing glass
* Large bonus for completing each mosaic
* Levels unlock equipment tiers and new shoreline sections

Bottle caps:

* Spendable currency
* Earned primarily by recycling rubbish
* Some found directly on the shoreline
* Bonus awarded for mosaic completion
* Used to purchase equipment after its tier is unlocked through XP

Create a readable upgrade station, pegboard or catalogue inside the studio.

Equipment progression should include a selection from:

Gathering:

* Toy hand shovel
* Metal trowel
* Sieve scoop
* Wide beach rake
* Powered sand sifter

Carrying:

* Small bucket
* Larger bucket
* Collection sack
* Handcart

Truck:

* Larger cargo crates
* Separate cargo dividers
* Increased capacity
* Improved vehicle stability

Cleaning:

* Brush and water bowl
* Hand sprayer
* Cone rinse nozzle
* Pressure washer
* Polishing tumbler

Sorting and mosaic assistance:

* Larger sorting trays
* Increased snap radius
* Sorting magnet
* Light table
* Shape hints

Keep the total Version 1 upgrade selection manageable. Prioritize upgrades that visibly change reach, speed, capacity or interaction rather than invisible percentage increases.

Balance the opening so the first meaningful upgrade can normally be obtained within approximately five to eight minutes.

ARCHITECTURE

Use clean, maintainable C# architecture without unnecessary frameworks.

Prefer:

* ScriptableObjects for item, tool, upgrade, glass and mosaic definitions
* Stable unique identifiers for saved objects
* Event-driven feedback where useful
* Separate gameplay logic from audiovisual feedback
* Reusable interaction interfaces
* Explicit game states for walking, driving and workstation interaction
* Object pooling for frequently repeated particles
* MaterialPropertyBlocks for per-piece glass and grime variation
* Data-driven socket compatibility
* JSON save data stored through Application.persistentDataPath

Create organized folders under Assets for:

* Art
* Audio
* Materials
* Prefabs
* Scenes
* Scripts
* ScriptableObjects or Data
* UI
* VFX
* Tests

Avoid giant manager scripts and hidden scene dependencies. Add helpful comments where design intent is not obvious.

SAVE SYSTEM

Save at appropriate checkpoints and when quitting:

* XP and level
* Bottle caps
* Purchased upgrades
* Unlocked beach sections
* Collected and processed object IDs
* Cleaned glass state
* Sorted glass state
* Mosaic socket contents
* Completed panels
* Truck capacity upgrades
* Tutorial progress
* Whether skip-drive is unlocked
* Player location or a safe return location

Provide New Game, Continue and Reset Progress options. Reset Progress must require confirmation.

UI AND ACCESSIBILITY

Create a minimal illustrated interface matching the art style:

* Warm cream panels
* Dark teal text
* Coral or orange highlights
* Rounded shapes
* Simple equipment icons
* Minimal permanent HUD

Include:

* Interaction reticle
* Current XP and level
* Bottle-cap count
* Equipped tool
* Bucket or truck capacity
* Current objective
* Mosaic progress
* Contextual tutorial prompts
* Pause menu
* Separate music and effects volume
* Mouse sensitivity
* Invert-Y option
* Fullscreen and resolution controls where practical

Do not cover the satisfying interactions with oversized interface elements.

AUDIO AND FEEDBACK

Use replaceable original placeholder audio or procedurally generated simple sounds when external assets are unavailable.

Differentiate:

* Sand scraping
* Plastic impacts
* Metal impacts
* Glass clicks
* Water spraying
* Brushing
* Sorting snaps
* Truck engine
* Cargo rattling
* Recycling
* Upgrade purchase
* Mosaic completion

Glass placement should use gently pitched notes. Completing a row or panel should resolve those notes into a pleasant chord.

Use restrained particles, scale punches, object bounce, easing, light changes and subtle camera impulse. Feedback should feel tactile without causing motion sickness.

TUTORIAL

Teach through short contextual prompts rather than a long explanation.

The opening should guide the player through:

1. Looking and moving
2. Using the toy shovel
3. Collecting rubbish and glass
4. Filling the bucket
5. Loading the truck
6. Driving to the studio
7. Recycling rubbish
8. Cleaning one glass piece
9. Sorting it
10. Placing it into the first mosaic
11. Purchasing the first upgrade

Allow tutorial prompts to be dismissed and avoid repeatedly showing completed instructions.

ROBUSTNESS

Account for:

* Dropped objects
* Objects leaving the world
* Truck overturning
* Entering and exiting near obstacles
* Saving during different gameplay states
* Loading incomplete mosaic progress
* Missing references
* Duplicate object IDs
* Full containers
* Incorrect sorting attempts
* Pausing while using a workstation
* Returning to the shoreline after processing a haul

Add forgiving completion thresholds. Never require the player to find one invisible object to finish an area; provide a highlight or recovery ability for remaining important objects.

SCOPE EXCLUSIONS

Do not implement:

* Open-world exploration
* NPCs or dialogue systems
* Multiplayer
* Traffic
* Complex vehicle simulation
* Real fluid simulation
* Deformable sand
* Procedural glass fracture
* Freeform glass cutting
* Weather systems
* Dynamic day and night
* Skill trees
* Voice acting
* Cinematic story scenes
* Console support
* Online services
* Hundreds of collectibles
* Paid external assets

IMPLEMENTATION ORDER

Use this order and verify each stage before continuing:

1. Inspect and document the existing project.
2. Establish folders, architecture and data definitions.
3. Build first-person movement and interaction.
4. Block out the continuous shoreline, road and studio using ProBuilder.
5. Implement shoreline items, digging, gathering and containers.
6. Implement truck loading, capacity, driving and reset behavior.
7. Implement studio recycling.
8. Implement glass cleaning and visual transformation.
9. Implement sorting and snapping.
10. Implement the Ocean Wave mosaic completely.
11. Implement XP, bottle caps and upgrades.
12. Add the Lighthouse and Sunset mosaics.
13. Add save/load and menus.
14. Apply the full painterly coastal art pass.
15. Add audio, animation, VFX and final restoration.
16. Add tutorial and accessibility settings.
17. Run automated and manual tests.
18. Profile and optimize.
19. Produce a Windows build if supported.
20. Review the entire game against the definition of done.

DEFINITION OF DONE

Do not describe the project as complete unless:

* The project compiles with no errors.
* The main gameplay loop is playable from beginning to end.
* The player can walk, gather, load, drive, recycle, clean, sort and place glass.
* XP and bottle caps work.
* At least one meaningful upgrade exists in every major equipment category.
* All three mosaics can be completed.
* The final restoration sequence runs.
* Saving and loading preserve meaningful progress.
* The player cannot become permanently trapped through normal play.
* The game has a main menu and pause menu.
* Audio and visual feedback are present.
* The environment follows the specified art direction.
* Automated tests pass.
* A complete Play Mode smoke test has been performed.
* No new Console errors remain.
* Relevant scenes and assets are saved.
* Documentation explains controls, architecture, how to play and known limitations.

GIT AND DELIVERY

* Inspect Git status before beginning.
* Preserve unrelated existing changes.
* Make logical local commits at stable milestones.
* Do not commit Library, Temp, Logs, generated builds or IDE files.
* After final verification, commit the completed Version 1 work.
* Push to origin/main only if authentication is already available and all verification has passed.
* If pushing is unavailable, leave the verified commits locally and report the exact command I should run.

At completion, provide:

* A concise summary of what was built
* Controls
* How to play from start to finish
* Test results
* Build location, if created
* Git commit hashes
* Any remaining limitations
* The three highest-value improvements for a later version

Begin by inspecting the active Unity project and current Git status. Then show a concise implementation plan and immediately proceed with the work without waiting for additional confirmation.
