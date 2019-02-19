using System;
using System.Collections.Generic;
using System.Text;

namespace Ddi.Registry.ZoneWriter
{
    public class ZoneWriterOptions
    {     
        public string BaseZone { get; set; }
        public string MasterNameserver { get; set; }
        public List<string> Nameservers { get; set; }
        public string DefaultARecord { get; set; }
        public string ZoneFileLocation { get; set; }
        public string ExtraRecords { get; set; }
    }
}
