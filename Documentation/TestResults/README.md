# Verification artifacts

- `PlayMode-current.json`: detailed full PlayMode result from Unity MCP, September 27, 2026. Eleven tests passed in 63.79 seconds, including mouse/keyboard interactions, screen transitions and recovery after Skip Drive.
- `EditMode.xml`: Unity Test Runner's September 26 result. Five tests passed.
- `WindowsBuild.json`: latest Windows build report. This is refreshed after each delivery build.
- `PlayMode.xml`: Unity Test Runner XML for that same eleven-test run.
- `Standalone.json`: built-player rendering and gameplay check, including 300 offscreen frame samples.

The MCP test job retains detailed results as JSON. Unity's default `TestResults.xml` output is overwritten by the next test mode, so each mode's XML is copied separately. The JSON additionally preserves the MCP job identity and progress metadata.
