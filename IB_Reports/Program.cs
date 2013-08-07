using System;
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

            logger.WriteToLog(DateTime.Now, "   start process   ","IB_Log");
            
            ProcessManager.Start();

            logger.WriteToLog(DateTime.Now, "Done", "IB_Log");
 
        }

    }
}
