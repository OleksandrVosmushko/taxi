using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class DriverLicense
    {
        public string Id { get; set; }

        public DateTime UpdateTime { get; set; }

        public Driver Driver { get; set; }

        public Guid DriverId { get; set; }
    }
}
