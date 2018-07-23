using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Models.Trips
{
    public class TripCreationDto
    {
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Place From { get; set; }
        [Required]
        public Place To { get; set; }
  }
}
