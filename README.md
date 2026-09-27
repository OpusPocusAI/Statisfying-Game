# Sea Glass Studio

A compact first-person coastal restoration game for Windows, built with Unity 6000.3.16f1 and URP. Open `Assets/Scenes/SeaGlassStudio.unity` and press Play. The original SampleScene is preserved.

## A visit to the studio

Start a new visit from the cream menu. Walk to a sand mound in Shell Cove, hold the left mouse button to uncover its contents, then press E to collect. Your toy shovel and seven-piece bucket are intentionally modest.

Walk to the truck beside the road. At the back, load the GLASS and RECYCLE crates separately with E. At the cab, press E to drive. Follow the marked road north; the studio's orange canopy is your destination. Brake before exiting. You can make a partial delivery whenever you like. After completing the first manual trip, the small SKIP plate on the tailgate offers optional fast travel in either direction.

Park beside the studio. Recycle the rubbish at the coral press for permanent XP and spendable bottle caps. At the enamel basin, press E and hold the mouse button to wash each piece. Leave with E. At the sorting table, press E, look at the matching broad colour tray, and click. Incorrect trays only offer a hint. At a mosaic, press E on a socket or frame to fit an available compatible piece.

Buy equipment at the pegboard catalogue. XP unlocks the two further coves and equipment tiers; it is never spent. Bottle caps pay for the sieve scoop, larger sack, taller cargo dividers, hand sprayer, and sorting magnet. Each of the three panels contains ten original polygon shards. Complete Ocean Wave, Lighthouse, and Coastal Sunset to illuminate the triptych and restore the studio.

## Controls

| Input | Action |
| --- | --- |
| WASD / mouse | Walk / look; steer and accelerate in the truck |
| Shift / Space | Sprint / jump; Space brakes in the truck |
| E | Collect, load, enter/exit truck, use/leave station, place glass |
| Hold left mouse | Dig or wash |
| Left mouse at sorting station | Snap held glass into the targeted colour tray |
| 1 / 2 / mouse wheel | Shovel or carrying container |
| F | Highlight remaining shoreline glass |
| R | Recover truck safely beside the road |
| H | Hide or restore tutorial guidance |
| Escape | Pause, save, settings, return to menu |

Music/effects volume, mouse sensitivity, invert Y, fullscreen, and 720p/1080p settings are available from both menus. No head bob is applied.

## Persistence and recovery

Progress is JSON at `Application.persistentDataPath/seaglass-v1.json`, with an atomic replacement and one backup. It includes every collected item's stable ID, container/studio stage, partial grime, sorted and installed glass, XP, currency, purchased equipment, completed panels, tutorial stage and visibility, and drive-skip unlock. Loading returns the player and truck to a safe location at the studio or Shell Cove according to the saved region; their inventory is retained. Reset requires confirmation. Damaged saves are preserved rather than silently overwritten during load.

Cargo and held pieces use constrained presentation rather than loose rigidbodies, so important objects cannot be accidentally thrown away. Out-of-world truck/player recovery and F highlighting provide additional assistance.

## Architecture

- `GameCatalog` is the authored ScriptableObject for 30 glass definitions and five upgrades. Individual meshes are shared between shore finds and mosaic sockets.
- `Progress` and `ItemRecord` store stable IDs and explicit lifecycle stages. `GameSession` owns economy, checkpoints, menus, and synchronization.
- `CoastalPlayer` handles the existing Input System and reusable `Interactable` contract. `UtilityTruck` has arcade movement and separate interaction triggers.
- `ShoreItem`, `StudioStation`, and `MosaicPanel` own their gameplay stages. Visual helpers provide per-piece material properties, equipment models, and restoration.
- `Feedback` generates original replaceable audio and uses a six-system particle pool. Water and frosted-glass shaders are original source assets.
- Editor builders under `Scripts/SeaGlass/Editor` author saved geometry, data, and scene objects. ProBuilder produces the structural geometry. Rebuilding is an explicit menu action, never a runtime requirement.

## Verification and building

Run the `SeaGlass.Tests` Edit Mode assembly and `SeaGlass.PlayModeTests` Play Mode assembly in Unity Test Runner. The latter uses a dedicated test save filename and exercises the scene's production systems. See `Documentation/IMPLEMENTATION.md` for the starting state and `Documentation/VERIFICATION.md` for final results and limitations when available.

Windows builds belong under `Builds/Windows/`. Generated builds, caches, and IDE files are excluded from Git. The game uses only original generated assets and the packages already installed in the supplied project.

The built player accepts `--verify-seaglass -batchmode` for an isolated acceptance check with GPU rendering. It exercises collection, separate loading, recycling, washing, sorting, placement and restoration; writes camera screenshots and `Verification/report.json` beside the executable; samples 300 restored-studio frames; and exits with a nonzero code on failure. It never loads or saves player progress. These offscreen captures exclude IMGUI; UI was inspected separately in the live Editor. This scripted check accelerates progress and is not a measurement of normal play duration or a substitute for human usability testing. See [verification results and remaining acceptance work](Documentation/VERIFICATION.md).
