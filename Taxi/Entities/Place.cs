﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class Place
    {
        public string Id { get; set; }

        public string Address { get; set; }

        public string Attributions { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Name { get; set; }
    }
}
