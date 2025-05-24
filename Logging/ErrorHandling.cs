//using Pushover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class ErrorHandling
    {
        public static List<string> ErrorItems = new List<string>();
        public static string MachineName = Environment.MachineName;


        // Temp
        public void LogError(string errorMessage)
        {
            string formattedError = $"{DateTime.Now}: {errorMessage}";
            ErrorItems.Add(formattedError);
            Console.WriteLine(formattedError);
        }

        //// Send Fatal Error to Pushover
        //public void SendToPushover(string errorToSend)
        //{
        //    PushoverClient pushoverClient = new PushoverClient("token");

        //    PushoverMessage message = new PushoverMessage()
        //    {
        //        Title = "Spacefarmers Data Collection Fatal Error.",
        //        Message = "The Spacefarmers.io Data Collection Service on {MachineName} has thrown a fatal error. \n\n<b>Error:</b>\n{errorToSend}",
        //        Priority = Priority.Normal,
        //        Sound = "magic",
        //        Url = "",
        //        UrlTitle = "",
        //    };

        //    bool result = pushoverClient.Send("token", message);
        //}
    }
}
