# Sea Glass Studio verification — September 2026

This is a playable V1 candidate, not a claim of perfection or measured 30–45-minute pacing. Scope was checked against `DEVELOPMENT_PROMPT.md`. No engine or package upgrade was made.

## Evidence

| Area | Result and limits |
| --- | --- |
| Editor compilation | Clean after importing the final assembly references. Active scene is `Assets/Scenes/SeaGlassStudio.unity`. |
| EditMode | 5/5 passed. Progress defaults, IDs, JSON round-trip, section unlocks, catalogue and economy constraints. |
| PlayMode | 4/4 passed after the coastal art pass. Complete manual truck drive through Input System events; collection, loading, recycling, cleaning, sorting, all 30 placements, all five upgrades, restoration, save/load and recovery. Additional regressions cover cargo proportions, continuous grime, visible sorted inventory and malformed saves. Latest detailed output: `TestResults/PlayMode-current.json`. |
| Rendering references | Live scene scan found zero missing scripts, broken materials or shader compiler errors. |
| Live visual checks | Shoreline, water, dirty/half-clean glass, studio, UI and completed triptych inspected in Play Mode using isolated in-memory progress. Live UI captures are under ignored `Captures/Polish`. |
| Standalone | Explicit `--verify-seaglass` check exercises production systems and captures eight views. It verifies all 30 placements, the ending state, three reflected-light meshes, supported material shaders and no captured game exceptions. Latest result: `TestResults/Standalone.json`. |
| Build | Windows x64, Unity 6000.3.16f1. Latest report: `TestResults/WindowsBuild.json`; executable: `Builds/Windows/SeaGlassStudio.exe`. |
| Save protection | Verification uses `testMode`; regression saves use separate test filenames. The player-save directory had no `seaglass-v1.json` files before or after standalone verification. No player progress was created or replaced. |

The standalone command is `SeaGlassStudio.exe --verify-seaglass -batchmode -screen-width 1920 -screen-height 1080 -screen-fullscreen 0`. Do not add `-nographics`: this check requires real GPU rendering. It writes to `Verification` beside the executable and exits on completion or timeout. Its captures use explicit URP camera rendering and exclude IMGUI overlays; the live Editor captures provide separate UI evidence. The check accelerates progress and teleports between stations. It does not establish the natural length or feel of a playthrough.

## Performance observations

On the supplied AMD Ryzen 5 7500F / Radeon RX 7600 XT machine, the 1080p standalone offscreen sample averaged approximately 11.1 ms across 300 restored-studio frames. This is capped by the game's frame target and is not a presented-window FPS benchmark, worst-case profile, or low-spec guarantee. A live Editor sample reported approximately 17 ms CPU and 4.2 ms GPU; another sample was slower. The studio view reported about 18,333 triangles, 260 batches and 526 draw calls. These observations do not justify switching engines or aggressive geometry reduction. A broader foreground performance pass remains useful.

## Verification issues encountered

- Unity Test Framework interrupted one PlayMode run with an internal `PlayModeRunTask` null-reference exception. The orphan job was cleared after the Editor was confirmed idle; the complete rerun passed. No package source was edited.
- The original screenshot helper caused recursive PlayerLoop errors. Reading the existing Game-view buffer avoided that route.
- A hidden standalone window returned black swapchain captures and then stalled on `WaitForEndOfFrame`. The explicit URP render-request path avoids that dependency and has a 120-second timeout.
- The first standalone harness selected rubbish from a locked cove. Restricting its fixture to Shell Cove corrected the test setup.
- An intermediate build was started before all imports settled. The final delivery build was started only after compilation and domain reload finished.
- Unity emits `1 URP assets included in build` as a Console warning. This is an inventory message, not a missing renderer. Restricted-network player logs also contain Unity telemetry connection failures; no online service is part of the game loop.

## Remaining acceptance work and limitations

- A normal human playthrough must measure the 30–45-minute target and the first-upgrade 5–8-minute target. Automated accelerated journeys cannot validate them.
- No comprehensive human usability or boundary sweep has been completed. Recovery paths have automated coverage, but that is not proof against every possible trapping position.
- Original procedural audio remains deliberately simple. Listening, mixing and perceived satisfaction need a human pass.
- Geometry remains visibly economical: the truck, parasols, workbench tools and shoreline prop arrangement are simple. The coastal palette, glass transformation and illuminated triptych are implemented; richer silhouettes and less repetitive composition remain worthwhile.
- Lighting uses a fixed golden-hour setup, modest point lights and authored floor-light shapes. The floor shapes are a stylized effect, not simulated optical projection.

The highest-value next work is a timed usability playthrough, stronger prop/shoreline composition, and richer layered audio. Keep that work inside the compact scope; do not add open-world systems, weather, NPCs or complex vehicle simulation.
