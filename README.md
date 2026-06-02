# RevitMCP

RevitMCP is a Revit 2025 add-in that hosts a local Model Context Protocol (MCP) server inside Autodesk Revit. It lets MCP-capable AI clients such as Claude Code, Gemini CLI, and Codex inspect and modify the currently open Revit model through tool calls.

The server runs at:

```text
http://127.0.0.1:51235/
```

Because the MCP server is hosted by the Revit add-in, Revit must be open, a model must be loaded, and the server window must be started before an AI client can use the tools.

## What This Project Contains

- `RevitMcp.Plugin`: the Revit add-in, WPF control window, HTTP JSON-RPC MCP host, and Revit tool implementations.
- `RevitMcp.Contracts`: shared DTOs and MCP tool definitions.
- `Develoh.BIM.RVT25`: Revit 2025 helper library used by the add-in.

The add-in creates a `Develoh Tools` ribbon tab in Revit with an `AI` panel. From there, users can open the Revit MCP Server window and start or stop the local MCP endpoint.

## Available MCP Tools

- `ModelSummary`: returns project metadata, level count, and structural element counts.
- `ListLevels`: lists levels with id, name, and elevation in millimeters.
- `ListViews`: lists non-template views with id, name, type, and level.
- `ListFamilies`: lists loaded families with category and type count.
- `GetColumnFamilies`: lists available column families.
- `GetElements`: queries elements by category, level name, or family name.
- `GetElementParameters`: returns parameters for a selected element.
- `SetParameterValue`: updates one parameter on one element.
- `BatchSetParameters`: updates multiple parameters in a single transaction.
- `GetWarnings`: lists current model warnings and affected element ids.
- `CreateViewSnapshot`: exports a Revit view as a PNG image.
- `PlaceColumn`: places a column at a base point with a height.
- `PlaceBeam`: places a beam between two coordinates.

Some tools write to the Revit model. Use them on a copy of a model until you trust the workflow.

## Requirements

- Windows
- Autodesk Revit 2025 installed
- .NET SDK 8 or newer
- Access to `C:\ProgramData\Autodesk\Revit\Addins\2025\`

The project references:

```text
C:\Program Files\Autodesk\Revit 2025\RevitAPI.dll
C:\Program Files\Autodesk\Revit 2025\RevitAPIUI.dll
```

If Revit is installed somewhere else, update the `HintPath` values in `RevitMcp.Plugin\RevitMcp.Plugin.csproj`.

## Build

From the repository root:

```powershell
dotnet restore .\RevitMcp.Plugin.sln
dotnet build .\RevitMcp.Plugin.sln -c Debug
```

The `RevitMcp.Plugin` post-build target copies these files to the Revit 2025 add-ins folder:

```text
C:\ProgramData\Autodesk\Revit\Addins\2025\RevitMcp.Plugin.dll
C:\ProgramData\Autodesk\Revit\Addins\2025\RevitMcp.Contracts.dll
C:\ProgramData\Autodesk\Revit\Addins\2025\Develoh.BIM.RVT25.dll
```

If the copy fails, run the build from an elevated terminal or copy the files manually from:

```text
RevitMcp.Plugin\bin\Debug\net8.0-windows\
```

## Install the Revit Add-in Manifest

Copy the manifest:

```powershell
Copy-Item .\RevitMcp.Plugin\Addin\DevelohTools.addin "C:\ProgramData\Autodesk\Revit\Addins\2025\DevelohTools.addin"
```

The manifest expects `RevitMcp.Plugin.dll` to be in the same Revit add-ins folder.

Restart Revit after installing or rebuilding the add-in.

## Start the MCP Server

1. Open Revit 2025.
2. Open a project model.
3. Go to `Develoh Tools` > `AI` > `Start MCP server`.
4. In the `Revit MCP Server` window, click `Start`.
5. Confirm the server is running:

```powershell
Invoke-RestMethod http://127.0.0.1:51235/health
```

Expected response:

```json
{ "status": "ok" }
```

## Configure AI Clients

Use the root URL exactly as shown unless the client requires a different field name:

```text
http://127.0.0.1:51235/
```

### Claude Code

Add the server:

```bash
claude mcp add --transport http revit http://127.0.0.1:51235/
```

Verify it:

```bash
claude mcp list
```

Inside Claude Code, run:

```text
/mcp
```

Then ask questions such as:

```text
Use the Revit MCP server to summarize the current model.
```

### Gemini CLI

Add this to your Gemini CLI `settings.json`:

```json
{
  "mcpServers": {
    "revit": {
      "httpUrl": "http://127.0.0.1:51235/",
      "trust": false
    }
  }
}
```

Gemini CLI uses `httpUrl` for Streamable HTTP MCP servers. After restarting Gemini CLI, run:

```text
/mcp
```

### Codex

Add the server with the Codex CLI:

```bash
codex mcp add revit --url http://127.0.0.1:51235/
codex mcp list
```

Or add it directly to `~/.codex/config.toml`:

```toml
[mcp_servers.revit]
url = "http://127.0.0.1:51235/"
```

Then start Codex from a project where you want the Revit tools available.

## Manual Protocol Test

List tools with a raw JSON-RPC request:

```powershell
$body = @{
  jsonrpc = "2.0"
  id = 1
  method = "tools/list"
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri http://127.0.0.1:51235/ -ContentType "application/json" -Body $body
```

Call a tool:

```powershell
$body = @{
  jsonrpc = "2.0"
  id = 2
  method = "tools/call"
  params = @{
    name = "ModelSummary"
    arguments = @{}
  }
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Method Post -Uri http://127.0.0.1:51235/ -ContentType "application/json" -Body $body
```

## Development Notes

- The HTTP listener is implemented in `RevitMcp.Plugin\McpHostedServer.cs`.
- Tools are registered in `McpHostedServer.RegisterTools()`.
- Revit API work is routed through external events so model operations run on the Revit UI thread.
- The server currently binds only to `127.0.0.1` and does not implement authentication. Do not expose it directly to a network.
- The MCP protocol version returned by the server is `2024-11-05`.

## Troubleshooting

- `Connection refused`: start Revit, open the MCP Server window, and click `Start`.
- `404 Not found`: use `http://127.0.0.1:51235/`, including the trailing `/`, for MCP JSON-RPC calls.
- Revit does not show the ribbon tab: confirm `DevelohTools.addin` and the DLLs are in `C:\ProgramData\Autodesk\Revit\Addins\2025\`, then restart Revit.
- Build cannot copy files to `ProgramData`: run the build as Administrator or copy the files manually.
- Tool calls fail or return empty data: make sure a Revit model is open and the requested elements, views, levels, or families exist in that model.
