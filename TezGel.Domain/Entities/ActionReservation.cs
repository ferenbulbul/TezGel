using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Common;
using TezGel.Domain.Enums;

namespace TezGel.Domain.Entities
{
    
    public class ActionReservation : BaseEntity
    {
        public Guid UserId { get; set; }
        public CustomerUser CustomerUser { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpireAt { get; set; }

        public ActionStatus Status { get; set; } = ActionStatus.Pending;
    }
}