﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Models.Trips
{
    public class TripHistoryDto
    {
        public Guid Id { get; set; }

        public PlaceDto From { get; set; }

        public PlaceDto To { get; set; }

        public Guid CustomerId { get; set; }

        public Guid DriverId { get; set; }

        public DateTime FinishTime { get; set; }

        public decimal Price { get; set; }
    }
}
