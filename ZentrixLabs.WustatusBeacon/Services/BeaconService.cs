using System;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Text;
using Newtonsoft.Json;

namespace ZentrixLabs.WustatusBeacon
{
    public class BeaconService : ServiceBase
    {
        private HttpListener _listener;
        private EventLog _eventLog;

        protected override void OnStart(string[] args)
        {
            // Setup event log
            _eventLog = new EventLog();
            if (!EventLog.SourceExists("WustatusBeacon"))
            {
                EventLog.CreateEventSource("WustatusBeacon", "Application");
            }
            _eventLog.Source = "WustatusBeacon";
            _eventLog.Log = "Application";

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:7341/wustatus/");
                _listener.Prefixes.Add("http://localhost:7341/health/");
                _listener.Start();
                _listener.BeginGetContext(OnRequest, null);

                _eventLog.WriteEntry("Wustatus Beacon service started successfully.", EventLogEntryType.Information, 1000);
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry($"Failed to start HTTP listener: {ex.Message}", EventLogEntryType.Error, 3000);
                throw;
            }
        }

        protected override void OnStop()
        {
            _listener?.Stop();
            _listener?.Close();

            _eventLog?.WriteEntry("Wustatus Beacon service stopped.", EventLogEntryType.Information, 1001);
        }

        private void OnRequest(IAsyncResult ar)
        {
            if (_listener == null || !_listener.IsListening) return;

            HttpListenerContext context = null;
            try
            {
                context = _listener.EndGetContext(ar);
                _listener.BeginGetContext(OnRequest, null); // Continue listening

                var response = context.Response;
                var path = context.Request.Url.AbsolutePath.ToLowerInvariant();

                string responseJson;
                if (path.StartsWith("/wustatus"))
                {
                    var status = PatchInfoService.GetPatchStatus();
                    responseJson = JsonConvert.SerializeObject(status, Formatting.Indented);
                    _eventLog.WriteEntry("Wustatus endpoint requested.", EventLogEntryType.Information, 4000);
                }
                else if (path.StartsWith("/health"))
                {
                    responseJson = JsonConvert.SerializeObject(new
                    {
                        Status = "OK",
                        Service = "WustatusBeacon",
                        Timestamp = DateTime.UtcNow
                    }, Formatting.Indented);
                }
                else
                {
                    response.StatusCode = 404;
                    responseJson = "{\"error\": \"Not found\"}";
                    _eventLog.WriteEntry($"Received unknown path: {path}", EventLogEntryType.Warning, 2000);
                }

                var buffer = Encoding.UTF8.GetBytes(responseJson);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry($"Unhandled exception in request handler: {ex.Message}", EventLogEntryType.Error, 3001);
                if (context?.Response != null)
                {
                    var error = Encoding.UTF8.GetBytes("{\"error\": \"" + ex.Message + "\"}");
                    context.Response.StatusCode = 500;
                    context.Response.OutputStream.Write(error, 0, error.Length);
                    context.Response.OutputStream.Close();
                }
            }
        }
    }
}
