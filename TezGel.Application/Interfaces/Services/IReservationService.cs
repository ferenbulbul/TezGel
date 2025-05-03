using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.Interfaces.Services
{
    public interface IReservationService
    {
        Task ReserveProductAsync(Guid userId, Guid productId);
        Task CompleteReservationAsync(Guid reservationId);
    }
}