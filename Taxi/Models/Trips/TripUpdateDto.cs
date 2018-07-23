using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Models.Trips
{
    public class TripUpdateDto
    {
        public Place From { get; set; }
       
        public Place To { get; set; }
    }
}
