# Verification artifacts

- `PlayMode-current.json`: detailed full PlayMode result from Unity MCP, September 28, 2026. Seventeen tests passed in 102.75 seconds, including the timed cleaning reveal, all 30 sockets through reticle/E input and both truck crates, contextual guidance, screen transitions and recovery. Input tests temporarily route Editor events to the game independently of focused view, then restore the previous settings.
- `EditMode.xml`: Unity Test Runner's September 28 result. Six tests passed, including outward-facing and non-overlapping glass face geometry.
- `WindowsBuild.json`: latest Windows build report. This is refreshed after each delivery build.
- `PlayMode.xml`: Unity Test Runner XML for that same seventeen-test run.
- `Standalone.json`: built-player rendering and gameplay check, including 300 offscreen frame samples.

The MCP test job retains detailed results as JSON. Unity's default `TestResults.xml` output is overwritten by the next test mode, so each mode's XML is copied separately. The JSON additionally preserves the MCP job identity and progress metadata.
