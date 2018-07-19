﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;

namespace NetTracker
{
    public class Tracker
    {
        public bool IgnoreLocalAddress { get; set; }

        public int LogFrequency { get; set; }

        public string LogPath { get; set; }

        private Dictionary<string, bool> table;

        private DateTime lastWrite;

        private Dictionary<string, bool> localAddressPrefixes;

        private DatabaseReader reader;

        private List<string> possibleDBLocations = new List<string> { "c:\\GeoLite2-Country.mmdb", "GeoLite2-Country.mmdb" };
        #region Public Methods

        public Tracker()
        {
            IgnoreLocalAddress = true;  // default is to ignore local addresses
            LogFrequency = 5;           // default is 5 minutes
            LogPath = string.Empty;     // default log directory is the current directory

            localAddressPrefixes = new Dictionary<string, bool>();
            AddLocalAddressPrefixes();
            table = new Dictionary<string, bool>();
            lastWrite = DateTime.Now;

            foreach(var possible in possibleDBLocations)
            {
                if (File.Exists(possible))
                {
                    reader = new DatabaseReader(possible);
                    break;
                }
            }
            if (reader == null)
            {
                throw new FileNotFoundException("Can't find the GeoLite2-Country.mmdb database!");
            }
        }

        public string GetCountry(string addr)
        {
            try
            {
                return reader.Country(addr).Country.ToString();
            }
            catch(Exception)
            {
                return "<Unknown>";
            }
        }

        public bool IsNotInUSA(string addr)
        {
            try
            {
                return ("US" != reader.Country(addr).Country.IsoCode);
            }
            catch(Exception)
            {
                return true;
            }
        }

        public void PacketHandler(Packet packet)
        {
            IpV4Datagram ip = packet.Ethernet.IpV4;
            var src = $"{ip.Source}";
            var dest = $"{ip.Destination}";
            AddToTable(packet.Timestamp, dest);
        }

        #endregion // Public Methods

        #region Private Methods

        private void AddLocalAddressPrefixes()
        {
            localAddressPrefixes.Add("224", true);
            localAddressPrefixes.Add("10", true);
            localAddressPrefixes.Add("172", true);
            localAddressPrefixes.Add("192", true);
            localAddressPrefixes.Add("0", true);
            localAddressPrefixes.Add("255", true);
        }

        private bool IgnoreAddressFilter(string addr)
        {
            
            if (IgnoreLocalAddress)
            {
                var prefix = addr.Substring(0, addr.IndexOf('.'));
                return localAddressPrefixes.ContainsKey(prefix);
            }

            return false;
        }
        private void AddToTable(DateTime ts, string addr)
        {
            if (!IgnoreAddressFilter(addr))
            {
                if (!table.ContainsKey(addr))
                {
                    table.Add(addr, true);
                    if (IsNotInUSA(addr))
                    {
                        Console.WriteLine($"New address: {addr} in country {GetCountry(addr)}");
                    }
                }
            }
            if (table.Count > 0 && (ts - lastWrite) > TimeSpan.FromMinutes(LogFrequency))
            {
                string fmt = "yyyy-MM-dd-hh-mm-ss-fff";
                string fullPath = Path.Combine(LogPath, $"{ts.ToString(fmt)}.txt");
                using (StreamWriter writer = new StreamWriter(fullPath))
                {
                    foreach(var key in table.Keys)
                    {
                        writer.WriteLine($"{key} in {GetCountry(key)}");
                    }
                }
                lastWrite = DateTime.Now;
                table = new Dictionary<string, bool>();
            }
        }

        #endregion // Private Methods
    }
}
