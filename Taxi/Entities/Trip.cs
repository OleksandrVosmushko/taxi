using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class Trip
    {
        public Guid Id { set; get; }

        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; }

        public Guid? DriverId { get; set; }

        public Driver Driver{ get; set; }
        
        public List<Place> Places { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime DriverTakeTripTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }
    }
}
 