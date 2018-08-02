using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Models.Drivers
{
    public class FileDto
    {
        public MemoryStream Stream { get; set; }

        public string ContentType { get; set; }
    }
}
