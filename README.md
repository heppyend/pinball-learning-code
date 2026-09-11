# Pinball Source Learning Edition

This repository is a source-only learning snapshot of the Pinball Unity project. It retains the client architecture and implementation, while intentionally excluding art, audio, models, materials, fonts, animations, asset bundles and build payloads.

## Included

- Unity C# client source, client scenes and structural prefabs.
- Lua source and XLua-related code.
- PC TCP/gateway client, packet protocol definitions and network parsing.
- WeChat Mini Game / WebGL adapters, plugins and retained hot-update DLLs.
- Unity Packages, ProjectSettings and development documentation.

## Server boundary

No independent server project exists in the original workspace. Networking files implement the client-side connection, request and packet-parsing path; they are not server business, database or deployment code.

## Reading entry points

- `PROJECT_OVERVIEW.md` — module map.
- `CURRENT_STATE.md` — delivery checkpoint and evidence.
- `Assets/Client/` — non-battle client features.
- `Assets/Scripts/` — Unity/PC client flow and protocols.
- `Assets/Lua/` — Lua source.
- `Assets/WX-WASM-SDK-V2/` and `WebGLPlugins/` — WeChat/WebGL integration.
