using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class Contract
    {
        [Key]
        public ulong Id { get; set; }

        public ulong TokenValue { get; set; }

        public double FromLatitude { get; set; }

        public double FromLongitude { get; set; }

        public double ToLatitude { get; set; }

        public double ToLongitude { get; set; }
        
    }
}
