using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public class Common
    {

        public static long ConvertToUnixEpoch (DateTime dateTime)
        {
            // Convert DateTime to Unix Epoch
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
            long unixEpoch = (dateTimeOffset.ToUnixTimeSeconds());
            return unixEpoch;
        }
    }
}
