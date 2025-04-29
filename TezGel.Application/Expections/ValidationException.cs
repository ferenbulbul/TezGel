using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Expection;

namespace TezGel.Application.Expections
{
    public class ValidationException : BaseException
    {
        public ValidationException(string message) : base(message, 400)
        {
        }
    }
}