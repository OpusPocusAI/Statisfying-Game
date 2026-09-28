# Sea Glass Studio verification — September 2026

This is a playable V1 candidate, not a claim of perfection or measured 30–45-minute pacing. Scope was checked against `DEVELOPMENT_PROMPT.md`. No engine or package upgrade was made.

## Evidence

| Area | Result and limits |
| --- | --- |
| Editor compilation | Clean after importing the final assembly references. Active scene is `Assets/Scenes/SeaGlassStudio.unity`. |
| EditMode | 6/6 passed. Progress defaults, IDs, JSON round-trip, section unlocks, catalogue/economy constraints, and outward-facing glass surfaces whose triangles match the authored polygon area. |
| PlayMode | 17/17 passed in 102.75 seconds after workstation presentation polish. The full journey waits for each cleaning reveal; the new regression checks pause, next-piece selection, exit and duplicate rewards. Complete manual truck drive through Input System events; collection, loading, recycling, cleaning, sorting, all 30 placements, all five upgrades, restoration, save/load and recovery. Additional regressions cover cargo proportions, continuous grime, visible sorted inventory and malformed saves. Mouse/keyboard checks cover tool-dependent digging, collection/station entry, both truck crates, all 30 sockets through precise reticle/E input, pausing washing, restoring the entry view, all six trays with and without the magnet, and resetting driver/new-visit camera state. Screen checks cover pause/menu transitions during a mosaic reveal, completion-screen control/cursor ownership, blocked gameplay shortcuts and recovery after both Skip Drive directions. Guidance checks cover partial mixed-cargo loading, rubbish-only recycling, processing after an early upgrade, the matching tray/panel and H dismissal. Fixtures teleport between stations; most of the full journey still calls gameplay methods directly. Latest detailed output: `TestResults/PlayMode-current.json`. |
| Rendering references | Live scene scan found zero missing scripts, broken materials or shader compiler errors. |
| Live visual checks | Shoreline, water, dirty/half-clean glass, studio, UI and completed triptych inspected in Play Mode using isolated in-memory progress. Guidance fits the existing HUD in live 1918 x 879 captures: `Captures/Polish/guidance-partial-load.png` and `guidance-no-glass.png`. September 28 built-player review checked corrected concave outlines, the layered Lighthouse door and held glass; triptych comparison captures are `Captures/Polish/triptych-before-geometry-20260928.png` and `triptych-after-geometry-20260928.png`. Workstation before/after captures and the mouse-driven cleaning sequence (`workbench-final-half-clean.png`, `workbench-final-reveal-1.png`, `workbench-final-reveal-2.png`, `workbench-final-sorting.png`) verify the presentation-first pass in the same folder. Captures are ignored by Git. |
| Standalone | Explicit `--verify-seaglass` check exercises production systems and captures eight views. It verifies all 30 placements, the ending state, three reflected-light meshes, supported material shaders and no captured game exceptions. Latest result: `TestResults/Standalone.json`. |
| Build | Windows x64, Unity 6000.3.16f1. Latest report: `TestResults/WindowsBuild.json`; executable: `Builds/Windows/SeaGlassStudio.exe`. |
| Save protection | Verification uses `testMode`; regression saves use separate test filenames. The player-save directory had no `seaglass-v1.json` files before or after standalone verification. No player progress was created or replaced. |

The standalone command is `SeaGlassStudio.exe --verify-seaglass -batchmode -screen-width 1920 -screen-height 1080 -screen-fullscreen 0`. Do not add `-nographics`: this check requires real GPU rendering. It writes to `Verification` beside the executable and exits on completion or timeout. Its captures use explicit URP camera rendering and exclude IMGUI overlays; the live Editor captures provide separate UI evidence. The check accelerates progress and teleports between stations. It does not establish the natural length or feel of a playthrough.

## Performance observations

On the supplied AMD Ryzen 5 7500F / Radeon RX 7600 XT machine, the latest 1080p standalone offscreen sample averaged 11.11 ms across 300 restored-studio frames, with an 11.11 ms 95th percentile. Earlier samples averaged roughly 11.1 to 19.3 ms. These runs were not controlled comparisons and do not establish the cause of the variation or prove a performance improvement from gameplay fixes. The game targets 90 frames per second; this hidden-player check is not a presented-window FPS benchmark, worst-case profile, or low-spec guarantee. A live Editor sample reported approximately 17 ms CPU and 4.2 ms GPU; another sample was slower. The studio view reported about 18,333 triangles, 260 batches and 526 draw calls. A foreground performance pass is still needed before claiming consistent smoothness.

## Verification issues encountered

- Editor focus routed test keyboard/mouse events to Scene view despite background-input handling. Tests now temporarily use explicit game input routing and restore the original setting; all 17 tests pass. The new reveal regression also stopped assuming catalogue order equals inventory order.

- Live targeting exposed overlapping rectangular mosaic colliders. Shape-matched colliders then exposed inward-facing sunset geometry and the Lighthouse door sharing the tower's plane. Concave glass-06/glass-21 also contained excess triangle area. Correct face triangulation/winding, mesh colliders and an explicit door depth offset now pass all 30 precise socket interactions. The broader panel placement fallback is retained.
- Live inspection briefly timed out around a Play Mode/MCP connection transition. Returning to Edit Mode and reconnecting restored inspection; no package changes were made.
- The fixed tutorial sequence requested driving before both crates were loaded and washing after a rubbish-only haul. Guidance now follows actual inventory and workstation state; it retains existing save milestones and dismissal without changing the economy.
- Live Escape events reproduced pause/resume releasing controls behind the completion screen. Screen and mosaic-presentation locks are now combined; the regression also checks returning to the menu during a reveal and starting a fresh visit.
- Skip Drive left the player's recovery point in the previous region. Both directions now record the destination; actual below-world recovery passes in PlayMode.
- Live interaction testing found that stations failed to restore the entry pitch and truck entry retained stored pitch. Both are fixed. Beginning a new visit also resets its recovery point.
- The first input-based sorting regression failed for Emerald: the wide assist cast hit Cobalt first despite precise aim at Emerald. Exact reticle hits now take priority; all twelve normal/magnet tray checks pass.
- Test cleanup removed the original preloaded input-action asset from Player Settings. Its original reference was restored after testing; this is not an intended configuration change.
- Unity Test Framework interrupted one PlayMode run with an internal `PlayModeRunTask` null-reference exception. The orphan job was cleared after the Editor was confirmed idle; the complete rerun passed. No package source was edited.
- The original screenshot helper caused recursive PlayerLoop errors. Reading the existing Game-view buffer avoided that route.
- A hidden standalone window returned black swapchain captures and then stalled on `WaitForEndOfFrame`. The explicit URP render-request path avoids that dependency and has a 120-second timeout.
- The first standalone harness selected rubbish from a locked cove. Restricting its fixture to Shell Cove corrected the test setup.
- An intermediate build was started before all imports settled. The final delivery build was started only after compilation and domain reload finished.
- The interaction-fix rebuild logged a build-worker pipe interruption, then recovered without a retry and succeeded in 177.43 seconds with zero build errors or warnings.
- Unity emits `1 URP assets included in build` as a Console warning. This is an inventory message, not a missing renderer. Restricted-network player logs also contain Unity telemetry connection failures; no online service is part of the game loop.

## Remaining acceptance work and limitations

- A normal human playthrough must measure the 30–45-minute target and the first-upgrade 5–8-minute target. Automated accelerated journeys cannot validate them.
- No comprehensive human usability or boundary sweep has been completed. Recovery paths have automated coverage, but that is not proof against every possible trapping position.
- Original procedural audio remains deliberately simple. Listening, mixing and perceived satisfaction need a human pass.
- Geometry remains visibly economical: the truck, parasols, workbench tools and shoreline prop arrangement are simple. The coastal palette, glass transformation and illuminated triptych are implemented; richer silhouettes and less repetitive composition remain worthwhile.
- Lighting uses a fixed golden-hour setup, modest point lights and authored floor-light shapes. The floor shapes are a stylized effect, not simulated optical projection.

The highest-value next work is a timed usability playthrough, stronger prop/shoreline composition, and richer layered audio. Keep that work inside the compact scope; do not add open-world systems, weather, NPCs or complex vehicle simulation.
