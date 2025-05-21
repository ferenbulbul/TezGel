using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.RabbitMq;

namespace TezGel.Application.Interfaces.Services
{
    public interface IMessagePublisher
    {
       void PublishReservationTimeoutMessage(ReservationTimeoutMessage message);
    }

}