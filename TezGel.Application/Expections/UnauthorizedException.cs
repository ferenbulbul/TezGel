using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Expection;

namespace TezGel.Application.Expections
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base(message, 401)
        {
        }
    }
}