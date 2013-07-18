using System;
using System.IO;
using System.Collections.Generic;
using Google.GData.Spreadsheets;
using IB_Reports;
using IB_Reports.Model;
using IB_Reports.Helper;

namespace IB_Reports
{
    // This is the main entry to the program
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            Logger.WriteToLog("start process");
            
            ProcessManager.Start();

            Logger.WriteToLog("Done");     
        }

    }
}
