using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.Expection
{

    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, 404)
        {
        }
    }


}