using System.ServiceProcess;

namespace ZentrixLabs.WustatusBeacon
{
    static class Program
    {
        static void Main()
        {
            ServiceBase.Run(new BeaconService());
        }
    }
}
