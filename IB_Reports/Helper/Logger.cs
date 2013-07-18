using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace IB_Reports
{
    public static class Logger
    {

        public static void WriteToLog(string message)
        {
            using (StreamWriter writer = new StreamWriter(ConfigurationManager.AppSettings["logFileName"], true))
            {
                writer.WriteLine( DateTime.Now + " : " + message);
                Console.WriteLine(message);
            }
        }

    }
}
