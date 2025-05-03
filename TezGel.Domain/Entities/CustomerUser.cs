using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Common;

namespace TezGel.Domain.Entities
{

    public class CustomerUser : BaseEntity
    {
        public Guid Id { get; set; }  // PK ve FK (AppUser.Id)
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }

        public AppUser AppUser { get; set; }
        public ICollection<ActionReservation> ActionReservations { get; set; }
    }

}