# Sea Glass Studio verification — September 2026

This is a playable V1 candidate, not a claim of perfection or measured 30–45-minute pacing. Scope was checked against `DEVELOPMENT_PROMPT.md`. No engine or package upgrade was made.

## Evidence

| Area | Result and limits |
| --- | --- |
| Editor compilation | Clean after importing the final assembly references. Active scene is `Assets/Scenes/SeaGlassStudio.unity`. |
| EditMode | 5/5 passed. Progress defaults, IDs, JSON round-trip, section unlocks, catalogue and economy constraints. |
| PlayMode | 11/11 passed in 63.79 seconds after screen-transition and recovery fixes. Complete manual truck drive through Input System events; collection, loading, recycling, cleaning, sorting, all 30 placements, all five upgrades, restoration, save/load and recovery. Additional regressions cover cargo proportions, continuous grime, visible sorted inventory and malformed saves. Mouse/keyboard checks cover tool-dependent digging, ray-based collection/station entry, pausing washing, restoring the entry view, all six trays with and without the magnet, and resetting driver/new-visit camera state. New checks cover pause/menu transitions during a mosaic reveal, completion-screen control/cursor ownership, blocked gameplay shortcuts and recovery after both Skip Drive directions. Fixtures teleport between stations; most of the full journey still calls gameplay methods directly. Latest detailed output: `TestResults/PlayMode-current.json`. |
| Rendering references | Live scene scan found zero missing scripts, broken materials or shader compiler errors. |
| Live visual checks | Shoreline, water, dirty/half-clean glass, studio, UI and completed triptych inspected in Play Mode using isolated in-memory progress. Live UI captures are under ignored `Captures/Polish`. |
| Standalone | Explicit `--verify-seaglass` check exercises production systems and captures eight views. It verifies all 30 placements, the ending state, three reflected-light meshes, supported material shaders and no captured game exceptions. Latest result: `TestResults/Standalone.json`. |
| Build | Windows x64, Unity 6000.3.16f1. Latest report: `TestResults/WindowsBuild.json`; executable: `Builds/Windows/SeaGlassStudio.exe`. |
| Save protection | Verification uses `testMode`; regression saves use separate test filenames. The player-save directory had no `seaglass-v1.json` files before or after standalone verification. No player progress was created or replaced. |

The standalone command is `SeaGlassStudio.exe --verify-seaglass -batchmode -screen-width 1920 -screen-height 1080 -screen-fullscreen 0`. Do not add `-nographics`: this check requires real GPU rendering. It writes to `Verification` beside the executable and exits on completion or timeout. Its captures use explicit URP camera rendering and exclude IMGUI overlays; the live Editor captures provide separate UI evidence. The check accelerates progress and teleports between stations. It does not establish the natural length or feel of a playthrough.

## Performance observations

On the supplied AMD Ryzen 5 7500F / Radeon RX 7600 XT machine, the latest 1080p standalone offscreen sample averaged 11.12 ms across 300 restored-studio frames, with an 11.11 ms 95th percentile. The previous sample averaged 19.32 ms with a 40.98 ms 95th percentile; an earlier sample was also around 11.1 ms. These runs were not controlled comparisons and do not establish the cause of the variation or prove a performance improvement from the control-state fixes. The game targets 90 frames per second; this hidden-player check is not a presented-window FPS benchmark, worst-case profile, or low-spec guarantee. A live Editor sample reported approximately 17 ms CPU and 4.2 ms GPU; another sample was slower. The studio view reported about 18,333 triangles, 260 batches and 526 draw calls. A foreground performance pass is still needed before claiming consistent smoothness.

## Verification issues encountered

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
