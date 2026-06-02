using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Revit;
using RevitMcp.Plugin.Tools;

namespace RevitMcp.Plugin
{
    public sealed class McpHostedServer : IDisposable
    {
        private const string Prefix = "http://127.0.0.1:51235/";
        private const string ProtocolVersion = "2024-11-05";

        private readonly HttpListener _listener = new HttpListener();
        private readonly List<McpToolDefinition> _toolDefs = new List<McpToolDefinition>();
        private readonly Dictionary<string, Func<JsonElement, object>> _handlers =
            new Dictionary<string, Func<JsonElement, object>>();
        private Thread? _thread;
        private volatile bool _running;

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public McpHostedServer()
        {
            _listener.Prefixes.Add(Prefix);
            RegisterTools();
        }

        // ── Tool registration ────────────────────────────────────────────────

        private void RegisterTools()
        {
            var facade = new RevitModelFacade();

            // Existing tools
            var beamTool           = new PlaceBeamTool(facade);
            var columnTool         = new PlaceColumnTool(facade);
            var getColFamiliesTool = new GetColumnFamiliesTool(facade);
            var modelSummaryTool   = new ModelSummaryTool(facade);

            // New tools
            var listLevelsTool           = new ListLevelsTool(facade);
            var listViewsTool            = new ListViewsTool(facade);
            var listFamiliesTool         = new ListFamiliesTool(facade);
            var getElementsTool          = new GetElementsTool(facade);
            var getElementParametersTool = new GetElementParametersTool(facade);
            var setParameterValueTool    = new SetParameterValueTool(facade);
            var batchSetParametersTool   = new BatchSetParametersTool(facade);
            var getWarningsTool          = new GetWarningsTool(facade);
            var createViewSnapshotTool   = new CreateViewSnapshotTool(facade);

            _toolDefs.Add(beamTool.GetDefinition());
            _toolDefs.Add(columnTool.GetDefinition());
            _toolDefs.Add(getColFamiliesTool.GetDefinition());
            _toolDefs.Add(modelSummaryTool.GetDefinition());
            _toolDefs.Add(listLevelsTool.GetDefinition());
            _toolDefs.Add(listViewsTool.GetDefinition());
            _toolDefs.Add(listFamiliesTool.GetDefinition());
            _toolDefs.Add(getElementsTool.GetDefinition());
            _toolDefs.Add(getElementParametersTool.GetDefinition());
            _toolDefs.Add(setParameterValueTool.GetDefinition());
            _toolDefs.Add(batchSetParametersTool.GetDefinition());
            _toolDefs.Add(getWarningsTool.GetDefinition());
            _toolDefs.Add(createViewSnapshotTool.GetDefinition());

            _handlers["PlaceBeam"]            = body => beamTool.Invoke(body);
            _handlers["PlaceColumn"]          = body => columnTool.Invoke(body);
            _handlers["GetColumnFamilies"]    = body => getColFamiliesTool.Invoke(body);
            _handlers["ModelSummary"]         = body => modelSummaryTool.Invoke(body);
            _handlers["ListLevels"]           = body => listLevelsTool.Invoke(body);
            _handlers["ListViews"]            = body => listViewsTool.Invoke(body);
            _handlers["ListFamilies"]         = body => listFamiliesTool.Invoke(body);
            _handlers["GetElements"]          = body => getElementsTool.Invoke(body);
            _handlers["GetElementParameters"] = body => getElementParametersTool.Invoke(body);
            _handlers["SetParameterValue"]    = body => setParameterValueTool.Invoke(body);
            _handlers["BatchSetParameters"]   = body => batchSetParametersTool.Invoke(body);
            _handlers["GetWarnings"]          = body => getWarningsTool.Invoke(body);
            _handlers["CreateViewSnapshot"]   = body => createViewSnapshotTool.Invoke(body);
        }

        // ── Lifecycle ────────────────────────────────────────────────────────

        public void Start()
        {
            _listener.Start();
            _running = true;
            _thread = new Thread(ListenLoop) { IsBackground = true, Name = "McpHttpServer" };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
            _thread?.Join(TimeSpan.FromSeconds(5));
        }

        public void Dispose() => Stop();

        // ── Request loop ─────────────────────────────────────────────────────

        private void ListenLoop()
        {
            while (_running)
            {
                HttpListenerContext? ctx = null;
                try { ctx = _listener.GetContext(); }
                catch { break; }
                ThreadPool.QueueUserWorkItem(_ => Handle(ctx));
            }
        }

        private void Handle(HttpListenerContext ctx)
        {
            var req  = ctx.Request;
            var resp = ctx.Response;
            resp.ContentType = "application/json; charset=utf-8";

            try
            {
                var path   = req.Url?.AbsolutePath ?? "";
                var method = req.HttpMethod;

                if (method == "GET" && path == "/health")
                {
                    Send(resp, 200, "{\"status\":\"ok\"}");
                    return;
                }

                if (method == "POST" && path == "/")
                {
                    HandleJsonRpc(req, resp);
                    return;
                }

                Send(resp, 404, "{\"error\":\"Not found\"}");
            }
            catch (Exception ex)
            {
                try { Send(resp, 500, JsonSerializer.Serialize(new { error = ex.Message })); }
                catch { /* best-effort */ }
            }
        }

        // ── JSON-RPC dispatcher ───────────────────────────────────────────────

        private void HandleJsonRpc(HttpListenerRequest req, HttpListenerResponse resp)
        {
            string rawBody;
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8))
                rawBody = reader.ReadToEnd();

            JsonDocument doc;
            try { doc = JsonDocument.Parse(rawBody); }
            catch
            {
                SendError(resp, null, -32700, "Parse error");
                return;
            }

            using (doc)
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    SendError(resp, null, -32600, "Invalid Request");
                    return;
                }

                bool hasId     = root.TryGetProperty("id", out var idEl);
                string mcpMethod = root.TryGetProperty("method", out var m) ? m.GetString() ?? "" : "";
                JsonElement paramsEl = root.TryGetProperty("params", out var p) ? p : default;

                if (!hasId)
                {
                    resp.StatusCode = 202;
                    resp.Close();
                    return;
                }

                switch (mcpMethod)
                {
                    case "initialize":
                        HandleInitialize(resp, idEl);
                        break;
                    case "ping":
                        SendResult(resp, idEl, new JsonObject());
                        break;
                    case "tools/list":
                        HandleToolsList(resp, idEl);
                        break;
                    case "tools/call":
                        HandleToolsCall(resp, idEl, paramsEl);
                        break;
                    default:
                        SendError(resp, idEl, -32601, $"Method not found: {mcpMethod}");
                        break;
                }
            }
        }

        // ── MCP method handlers ───────────────────────────────────────────────

        private void HandleInitialize(HttpListenerResponse resp, JsonElement id)
        {
            var result = new JsonObject
            {
                ["protocolVersion"] = ProtocolVersion,
                ["capabilities"]    = new JsonObject { ["tools"] = new JsonObject() },
                ["serverInfo"]      = new JsonObject { ["name"] = "revit-mcp-server", ["version"] = "1.0.0" }
            };
            SendResult(resp, id, result);
        }

        private void HandleToolsList(HttpListenerResponse resp, JsonElement id)
        {
            var toolsArray = new JsonArray();
            foreach (var def in _toolDefs)
            {
                var schemaJson = JsonSerializer.Serialize(def.InputSchema, JsonOpts);
                var schemaNode = JsonNode.Parse(schemaJson);
                toolsArray.Add(new JsonObject
                {
                    ["name"]        = def.Name,
                    ["description"] = def.Description,
                    ["inputSchema"] = schemaNode
                });
            }
            SendResult(resp, id, new JsonObject { ["tools"] = toolsArray });
        }

        private void HandleToolsCall(HttpListenerResponse resp, JsonElement id, JsonElement paramsEl)
        {
            string toolName = "";
            JsonElement args = default;

            if (paramsEl.ValueKind == JsonValueKind.Object)
            {
                if (paramsEl.TryGetProperty("name", out var n))
                    toolName = n.GetString() ?? "";
                if (paramsEl.TryGetProperty("arguments", out var a))
                    args = a;
            }

            if (!_handlers.TryGetValue(toolName, out var handler))
            {
                SendError(resp, id, -32602, $"Unknown tool: {toolName}");
                return;
            }

            if (args.ValueKind == JsonValueKind.Undefined)
                args = JsonDocument.Parse("{}").RootElement;

            object toolResult;
            try
            {
                toolResult = handler(args);
            }
            catch (ArgumentException ex)
            {
                SendError(resp, id, -32602, ex.Message);
                return;
            }
            catch (Exception ex)
            {
                SendError(resp, id, -32603, ex.Message);
                return;
            }

            // Return image content for view snapshots
            if (toolName == "CreateViewSnapshot" && toolResult is CreateViewSnapshotResult snapshot && snapshot.Success && snapshot.Base64Image != null)
            {
                var content = new JsonArray
                {
                    new JsonObject { ["type"] = "image", ["data"] = snapshot.Base64Image, ["mimeType"] = snapshot.MimeType }
                };
                SendResult(resp, id, new JsonObject { ["content"] = content });
                return;
            }

            var resultText = JsonSerializer.Serialize(toolResult, JsonOpts);
            var textContent = new JsonArray
            {
                new JsonObject { ["type"] = "text", ["text"] = resultText }
            };
            SendResult(resp, id, new JsonObject { ["content"] = textContent });
        }

        // ── JSON-RPC envelope helpers ─────────────────────────────────────────

        private void SendResult(HttpListenerResponse resp, JsonElement id, JsonNode result)
        {
            var envelope = BuildEnvelope(id);
            envelope["result"] = result;
            Send(resp, 200, envelope.ToJsonString());
        }

        private void SendError(HttpListenerResponse resp, JsonElement? id, int code, string message)
        {
            JsonObject envelope = id.HasValue
                ? BuildEnvelope(id.Value)
                : new JsonObject { ["jsonrpc"] = "2.0", ["id"] = JsonValue.Create((object?)null) };

            envelope["error"] = new JsonObject { ["code"] = code, ["message"] = message };
            Send(resp, 200, envelope.ToJsonString());
        }

        private static JsonObject BuildEnvelope(JsonElement id)
        {
            JsonNode idNode = JsonSerializer.SerializeToNode(id) ?? JsonValue.Create((object?)null)!;
            return new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode };
        }

        private static void Send(HttpListenerResponse resp, int statusCode, string json)
        {
            resp.StatusCode = statusCode;
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.OutputStream.Close();
        }
    }
}
