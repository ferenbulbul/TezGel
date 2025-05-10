using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Interfaces.Services;

namespace TezGel.Application.Services
{
   public class TimeZoneService : ITimeZoneService
{
    private const string TurkeyTimeZoneId = "Turkey Standard Time";

     public DateTime ConvertToTimeZone(DateTime utcDateTime, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch
        {
            return utcDateTime;
        }
    }

    public DateTime ConvertToTurkeyTime(DateTime utcDateTime)
    {
        return ConvertToTimeZone(utcDateTime, TurkeyTimeZoneId);
    }

    public DateTime FromTurkeyToUtc(DateTime localTurkeyTime)
    {
        var trZone = TimeZoneInfo.FindSystemTimeZoneById(TurkeyTimeZoneId);
        return TimeZoneInfo.ConvertTimeToUtc(localTurkeyTime, trZone);
    }
}
}