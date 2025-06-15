using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Enums;

namespace TezGel.Application.DTOs.ActionReservation
{
    public class RezervationResponseList
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public string CompanyName { get; set; }
        public TimeSpan ClosingTime { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ExpireAt { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }

    }
}