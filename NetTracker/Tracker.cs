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

        private Dictionary<string, bool> localAddresses;

        private DatabaseReader reader;

        private List<string> possibleDBLocations = new List<string> { "c:\\GeoLite2-Country.mmdb", "GeoLite2-Country.mmdb" };
        private Dictionary<string, bool> badActors;

        #region Public Methods

        public Tracker()
        {
            IgnoreLocalAddress = true;  // default is to ignore local addresses
            LogFrequency = 5;           // default is 5 minutes
            LogPath = string.Empty;     // default log directory is the current directory


            InitLocalAddressPrefixes();
            InitLocalAddresses();
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
            badActors = new Dictionary<string, bool>
            {
                { "AF", true },
                { "AL", true },
                { "AO", true },
                { "AR", true },
                { "AZ", true },
                { "BY", true },
                { "BJ", true },
                { "BG", true },
                { "KH", true },
                { "HR", true },
                { "EE", true },
                { "GA", true },
                { "GM", true },
                { "GN", true },
                { "GW", true },
                { "IR", true },
                { "CI", true },
                { "KZ", true },
                { "KE", true },
                { "KI", true },
                { "KG", true },
                { "LA", true },
                { "LV", true },
                { "LS", true },
                { "LR", true },
                { "LT", true },
                { "MW", true },
                { "MR", true },
                { "MU", true },
                { "MN", true },
                { "MZ", true },
                { "NR", true },
                { "NP", true },
                { "NI", true },
                { "KP", true },
                { "OM", true },
                { "PK", true },
                { "PS", true },
                { "RU", true },
                { "RO", true },
                { "RS", true },
                { "SC", true },
                { "SL", true },
                { "SK", true },
                { "SY", true },
                { "TJ", true },
                { "TH", true },
                { "TN", true },
                { "UA", true },
                { "UZ", true },
                { "VN", true },
                { "YE", true },
                { "ZM", true },
                { "ZW", true }
            };
        }

        private void InitLocalAddressPrefixes()
        {
            localAddressPrefixes = new Dictionary<string, bool>
            {
                {"224", true },
                {"10", true },
                {"172", true },
                {"192", true },
                {"0", true },
                {"255", true }
            };
        }

        private void InitLocalAddresses()
        {
            localAddresses = new Dictionary<string, bool>()
            {
                {"239.255.255.250", true }
            };
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
