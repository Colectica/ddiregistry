using System;
using System.Collections.Generic;
using System.Text;

namespace Ddi.Registry.Data
{
    public class ExportAction
    {
        public string Id { get; set; }

        public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;

        public long? LastSoa { get; set; }
    }
}
