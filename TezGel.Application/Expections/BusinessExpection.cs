using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Expection;

namespace TezGel.Application.Expections
{
    public class BusinessException : BaseException
{
    public BusinessException(string message)
        : base(message, 400) { }

    public BusinessException(string message, Exception innerException)
        : base(message, 400, innerException) { }
}
}