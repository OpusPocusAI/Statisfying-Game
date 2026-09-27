# Sea Glass Studio implementation record

Baseline: Unity 6000.3.16f1, URP 17.3.0, Input System 1.19.0, ProBuilder 6.0.9, Test Framework 1.6.0. Active scene was the unchanged SampleScene (camera, sunlight, volume). Console had no warnings or errors. Starting commit f9dd324.

Pre-existing user changes preserved: Assets/Settings/PC_RPAsset.asset, Packages/manifest.json, Packages/packages-lock.json, ProjectSettings/ShaderGraphSettings.asset, ProjectSettings/URPProjectSettings.asset, ProjectSettings/Packages/.

Implementation sequence: data and input foundation; authored continuous coast and collection/transport; recycling/cleaning/sorting/Ocean Wave; economy and remaining panels; persistence and presentation; automated and live verification; performance review and Windows build.

No package upgrades, external assets, or external services are required. Original geometric artwork and synthesized audio are produced within the project.

## Continued V1 work — September 2026

Source of scope and acceptance criteria: [DEVELOPMENT_PROMPT.md](DEVELOPMENT_PROMPT.md). Recheck its implementation order, art direction, scope exclusions, and definition of done at each milestone. The original prompt is not a record of completed features.

Baseline verification before this continuation: 5 EditMode tests and the complete PlayMode journey passed on September 25. Live review confirmed an early coastal art pass, not final visual quality. The existing MCP composited-screenshot helper produced recursive PlayerLoop errors; reading the already-rendered Game-view buffer avoided those errors. Do not use that failing capture route to assess game stability.

Current milestone follows the prompt's gathering, transport, cleaning, sorting, mosaic feedback, UI, and save-system requirements:

- Preserve the original cargo-rail proportions when applying the taller-divider upgrade.
- Make the cleaning shader consume continuous grime values and reveal translucency and highlights.
- Normalize working-glass size across authored shapes; show sorted inventory physically and animate transfers/placement.
- Show cleaning tools only at the cleaning station, with restrained brushing motion.
- Require the gathering tool for player-directed digging; retain forgiving interaction.
- Allow tutorial guidance to be hidden/restored, disable unavailable Continue, and soften UI panel corners.
- Reject invalid progress values/unknown collectible IDs without overwriting the save; resume studio sessions at a safe studio location.

Status on September 26: Unity was reopened by the user and the live MCP connection recovered. The gameplay changes compiled cleanly; `StudioPresentation` was connected and the scene saved. All four PlayMode tests passed, including the full triptych journey and three presentation/save regressions. The MCP server uses the cached 10.0.0 installation; no package was updated. Initial test failures exposed an unreliable fixed-duration braking assertion and a property-block initialization issue during reload; both were corrected before the successful run.

The following authored art changes are now saved: sloping wet sand, animated turquoise shallows, a fixed peach-to-blue sky, rounded coastal stones and tree canopies, actual studio window openings, floorboards, basin/tray details, smaller depth-tested lettering, and illuminated mosaic colours on the floor. Recycling has a short physical compression animation. The additive `CoastalFinishPass` does not regenerate gameplay or change collectible IDs. Live Game-view captures under the ignored `Captures/Polish` folder verified glass cleaning and the complete triptych/restoration with isolated in-memory progress. Player save data was not used for these views.

September 27 delivery verification: the art-pass regression completed with 4/4 PlayMode and 5/5 EditMode tests passing. The saved scene has no missing scripts, broken material references or shader errors. A Windows x64 build and isolated standalone journey passed, including eight inspected 1080p camera captures and all three illuminated panels. The offscreen performance sample averaged about 11.1 ms on the supplied Radeon RX 7600 XT; this is not a presented-window benchmark. The latest build, test and standalone reports are in `Documentation/TestResults`. Final image review reduced held sorting-glass size and replaced square feedback particles with soft discs; the final four-test PlayMode regression also passed, in 48.36 seconds.

See [VERIFICATION.md](VERIFICATION.md) for exact evidence, recovered test/build-tool failures, controls/build instructions and limits. This is a playable V1 candidate. The timed human playthrough, first-upgrade pacing, audio evaluation and broader usability/boundary sweep remain acceptance work. Do not mark the prompt fully satisfied or call the game perfect based on the automated checks. Unrelated pre-existing settings and MCP installation changes remain outside the scoped game commits; ProBuilder's already-installed dependency is included because the authored editor tools require it.

September 27 interaction verification: checked the prompt's requirement for stations to return cleanly to free movement and for forgiving sorting. Live inspection reproduced two camera issues: station exit retained the downward working angle, and truck entry's camera reset was overwritten by the controller's stored pitch. Both now synchronize stored look state; beginning a visit also resets the recovery point to its actual spawn. Mouse-event testing exposed the broad sorting cast selecting Cobalt while aimed directly at Emerald. Exact reticle hits now take priority, with nearby assistance ranked by alignment. All eight PlayMode regressions passed in 54.32 seconds, including all six trays with and without the magnet, tool-dependent digging/collection, washing while paused, camera restoration and the full journey. These tests use isolated progress and do not establish human pacing or subjective satisfaction.
