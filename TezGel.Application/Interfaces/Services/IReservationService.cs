using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Entities;

namespace TezGel.Application.Interfaces.Services
{
    public interface IReservationService
    {
        Task<ActionReservation> ReserveProductAsync(Guid userId, Guid productId);
        Task CompleteReservationAsync(Guid reservationId);
    }
}