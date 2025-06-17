using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.RabbitMq
{

    public class ReservationTimeoutMessage
    {
        public Guid ReservationId { get; set; } 
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }


}