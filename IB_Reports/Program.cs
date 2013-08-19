using System;
using System.Configuration;
using Logger;

namespace IB_Reports
{
    // This is the main entry to the program
    class Program
    {

        [STAThread]
        static void Main()
        {
            FileLogWriter logger = new FileLogWriter();

            logger.WriteToLog(DateTime.Now, "   start process   ",ConfigurationManager.AppSettings["logFileName"]);
            
            ProcessManager.Start();

            logger.WriteToLog(DateTime.Now, "Done", ConfigurationManager.AppSettings["logFileName"]);
 
        }

    }
}
