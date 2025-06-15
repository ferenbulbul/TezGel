using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.ActionReservation
{
    public class CompleteReservationRequest
    {
        public Guid reservationId { get; set; }
        public Guid businessQrid { get; set; }
    }
}