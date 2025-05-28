using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ZentrixLabs.WustatusBeacon
{
    public static class PatchInfoService
    {
        public static PatchStatus GetPatchStatus()
        {
            try
            {
                LogEvent("Starting GetPatchStatus()", EventLogEntryType.Information, 9001);

                var hostname = Environment.MachineName;
                var osVersion = GetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
                var ubrString = GetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "UBR");
                var ubr = string.IsNullOrWhiteSpace(ubrString) ? 0 : Convert.ToInt32(ubrString);
                var kernelVersion = "10.0." + GetRegistryValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber") + "." + ubr;

                var patchStatus = new PatchStatus
                {
                    Hostname = hostname,
                    OSVersion = osVersion,
                    KernelVersion = kernelVersion,
                    UBR = ubr,
                    LastPatchDate = GetLastPatchDate(),
                    InstalledKBs = GetInstalledKBs(),
                    PendingReboot = IsRebootPending()
                };

                LogEvent($"Successfully generated patch status for {hostname}", EventLogEntryType.Information, 9002);
                return patchStatus;
            }
            catch (Exception ex)
            {
                LogEvent($"Exception in GetPatchStatus: {ex}", EventLogEntryType.Error, 9900);
                throw;
            }
        }


        private static string GetRegistryValue(string path, string key)
        {
            using (var regKey = Registry.LocalMachine.OpenSubKey(path))
            {
                return regKey?.GetValue(key)?.ToString() ?? "";
            }
        }

        private static DateTime? GetLastPatchDate()
        {
            try
            {
                using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Install"))
                {
                    var val = regKey?.GetValue("LastSuccessTime")?.ToString();
                    if (DateTime.TryParse(val, out DateTime result))
                        return result;
                }
            }
            catch { }
            return null;
        }

        private static List<string> GetInstalledKBs()
        {
            var results = new HashSet<string>();

            // 1. Try registry-based scan (Component Based Servicing)
            try
            {
                using (var baseKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\Packages"))
                {
                    if (baseKey != null)
                    {
                        foreach (var name in baseKey.GetSubKeyNames())
                        {
                            if (name.Contains("KB"))
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(name, @"KB\d{6,7}");
                                if (match.Success)
                                {
                                    results.Add(match.Value);
                                }
                            }
                        }
                    }
                }
            }
            catch { /* ignore registry access issues */ }

            // 2. If no results, fall back to WMI (Win32_QuickFixEngineering)
            if (results.Count == 0)
            {
                try
                {
                    var searcher = new System.Management.ManagementObjectSearcher("SELECT HotFixID FROM Win32_QuickFixEngineering");
                    foreach (var obj in searcher.Get())
                    {
                        var hotFixId = obj["HotFixID"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(hotFixId) && hotFixId.StartsWith("KB"))
                        {
                            results.Add(hotFixId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"WMI query failed: {ex}", EventLogEntryType.Warning, 9901);
                }

            }

            return results.OrderByDescending(x => x).ToList();
        }




        private static bool IsRebootPending()
        {
            return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired") != null;
        }

        private static void LogEvent(string message, EventLogEntryType type = EventLogEntryType.Information, int eventId = 9000)
        {
            const string source = "WustatusBeacon";

            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }

                EventLog.WriteEntry(source, message, type, eventId);
            }
            catch
            {
                // Silent fail: avoid throwing from logging
            }
        }

    }
}
