using System;

namespace Djohnnie.HomeAutomation.Utils
{
    public static class EpochExtensions
    {
        public static DateTime ConvertEpochToDateTime(this Int64 timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddMilliseconds(timestamp);
        }

        public static Int64 ConvertDateTimeToEpoch(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (Int64)diff.TotalMilliseconds;
        }
    }
}