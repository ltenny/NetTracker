using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found!");
                return;
            }

            for(int i = 0; i < allDevices.Count; i++)
            {
                LivePacketDevice device = allDevices[i];
                string description = device.Description == null ? string.Empty : device.Description;
                if (i == 0)
                {
                    description = $"{description} (default)";
                }
                Console.WriteLine($"{i} - {description}");
            }

            Console.Write("Enter device number ==> ");
            var response = Console.ReadLine();
            int ndx = 0;
            int.TryParse(response, out ndx);
            if (ndx >= allDevices.Count)
            {
                Console.WriteLine("Need to pick on of the devices listed above.");
                return;
            }
            PacketDevice selectedDevice = allDevices[ndx];

            // this blocks until the user cancels
            var hostname = System.Environment.MachineName;
            using (PacketCommunicator communicator = selectedDevice.Open(6556, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                var tracker = new Tracker();
                Console.WriteLine($"Listening on {selectedDevice.Description}, filtering just for IP packets from {hostname}");
                communicator.SetFilter($"ip and src host {hostname}");
                communicator.ReceivePackets(0, tracker.PacketHandler);
            }
        }
    }
}
