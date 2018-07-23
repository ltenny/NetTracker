using System;
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

        private Dictionary<string, DateTime> table;

        private DateTime lastWrite;

        private Dictionary<string, bool> localAddressPrefixes;

        private DatabaseReader reader;

        private List<string> possibleDBLocations = new List<string> { "c:\\GeoLite2-Country.mmdb", "GeoLite2-Country.mmdb" };
        private Dictionary<string, bool> badActors;

        #region Public Methods

        public Tracker()
        {
            IgnoreLocalAddress = true;  // default is to ignore local addresses
            LogFrequency = 5;           // default is 5 minutes
            LogPath = string.Empty;     // default log directory is the current directory

            localAddressPrefixes = new Dictionary<string, bool>();
            AddLocalAddressPrefixes();
            table = new Dictionary<string, DateTime>();
            lastWrite = DateTime.Now;
            InitBadActorsTable();

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

        public bool IsBadActor(string addr)
        {
            try
            { 
                var cc = reader.Country(addr).Country.IsoCode;
                if (string.IsNullOrEmpty(cc)) return false;
                return (badActors.ContainsKey(cc));
            }
            catch(Exception)
            {
                return false; ;
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

        private void InitBadActorsTable()
        {
            // might be easier to to whitelist the good guys -;)
            badActors = new Dictionary<string, bool>();
            badActors.Add("AF", true);
            badActors.Add("AL", true);
            badActors.Add("AO", true);
            badActors.Add("AR", true);
            badActors.Add("AZ", true);
            badActors.Add("BY", true);
            badActors.Add("BJ", true);
            badActors.Add("BG", true);
            badActors.Add("KH", true);
            badActors.Add("HR", true);
            badActors.Add("EE", true);
            badActors.Add("GA", true);
            badActors.Add("GM", true);
            badActors.Add("GN", true);
            badActors.Add("GW", true);
            badActors.Add("IR", true);
            badActors.Add("CI", true);
            badActors.Add("KZ", true);
            badActors.Add("KE", true);
            badActors.Add("KI", true);
            badActors.Add("KG", true);
            badActors.Add("LA", true);
            badActors.Add("LV", true);
            badActors.Add("LS", true);
            badActors.Add("LR", true);
            badActors.Add("LT", true);
            badActors.Add("MW", true);
            badActors.Add("MR", true);
            badActors.Add("MU", true);
            badActors.Add("MN", true);
            badActors.Add("MZ", true);
            badActors.Add("NR", true);
            badActors.Add("NP", true);
            badActors.Add("NI", true);
            badActors.Add("KP", true);
            badActors.Add("OM", true);
            badActors.Add("PK", true);
            badActors.Add("PS", true);
            badActors.Add("RU", true);
            badActors.Add("RO", true);
            badActors.Add("RS", true);
            badActors.Add("SC", true);
            badActors.Add("SL", true);
            badActors.Add("SK", true);
            badActors.Add("SY", true);
            badActors.Add("TJ", true);
            badActors.Add("TH", true);
            badActors.Add("TN", true);
            badActors.Add("UA", true);
            badActors.Add("UZ", true);
            badActors.Add("VN", true);
            badActors.Add("YE", true);
            badActors.Add("ZM", true);
            badActors.Add("ZW", true);
        }

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
            string fmt = "yyyy-MM-dd-hh-mm-ss-fff";
            string fmt_file = "yyyy-MM-dd hh:mm:ss";

            if (!IgnoreAddressFilter(addr))
            {
                if (!table.ContainsKey(addr))
                {
                    table.Add(addr, ts);
                    if (IsBadActor(addr))
                    {
                        var save = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{ts.ToString(fmt_file)}: New address: {addr} in country {GetCountry(addr)}");
                        Console.ForegroundColor = save;
                    }
                    else
                    {
                        if (IsNotInUSA(addr))
                        {
                            Console.WriteLine($"{ts.ToString(fmt_file)}: New address: {addr} in country {GetCountry(addr)}");
                        }
                    }
                }
            }
            if ((ts - lastWrite) > TimeSpan.FromMinutes(LogFrequency))
            {
                string fullPath = Path.Combine(LogPath, $"{ts.ToString(fmt)}.txt");
                using (StreamWriter writer = new StreamWriter(fullPath))
                {
                    foreach(var key in table.Keys)
                    {
                        writer.WriteLine($"{table[key].ToString(fmt_file)}\t{key}\t{GetCountry(key)}");
                    }
                }
                lastWrite = DateTime.Now;
                table = new Dictionary<string, DateTime>();
            }
        }

        #endregion // Private Methods
    }
}
