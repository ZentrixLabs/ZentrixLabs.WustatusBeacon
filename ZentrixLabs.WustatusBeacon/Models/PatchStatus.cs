using System;
using System.Collections.Generic;

namespace ZentrixLabs.WustatusBeacon
{
    public class PatchStatus
    {
        public string Hostname { get; set; }
        public string OSVersion { get; set; }
        public string KernelVersion { get; set; }
        public int UBR { get; set; }
        public DateTime? LastPatchDate { get; set; }
        public List<string> InstalledKBs { get; set; }
        public bool PendingReboot { get; set; }
    }
}
