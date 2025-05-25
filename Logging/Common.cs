using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class Common
    {
        public static List<LogItem> LogItems = new List<LogItem>();

        public static void AddLogItem(string message, string level = "Info", string source = "General")
        {
            LogItems.Add(new LogItem(DateTime.Now, message, level, source));

            int i = 0;
        }



    }
}
