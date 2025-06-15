using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.ActionReservation;
using TezGel.Domain.Entities;

namespace TezGel.Application.Interfaces.Repositories
{
    public interface IActionRepository : IGenericRepository<ActionReservation>
    {
        Task<List<RezervationResponseList>> GetReservationsAsync(Guid userId);
    }
}