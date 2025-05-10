using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.Interfaces.Services
{
    public interface ITimeZoneService
    {
        DateTime ConvertToTimeZone(DateTime utcDateTime, string timeZoneId);
        DateTime ConvertToTurkeyTime(DateTime utcDateTime);
        DateTime FromTurkeyToUtc(DateTime localTurkeyTime);
    }
}