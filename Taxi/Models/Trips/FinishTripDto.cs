using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Models.Trips
{
    public class FinishTripDto
    {
        [Required]
        public PlaceDto To { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
