using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IB_Reports.Model;
using IB_Reports;
using IB_Reports.Helper;

namespace IB_Reports
{
    public static class ProcessManager
    {

        //Start the process
        public static void Start()
        {
            
            DataContext dbmanager = new DataContext();

            //Get from google the list of account information that have not been updated for this day
            List<Account> accounts = GoogleManager.GetNotSuccessedAccounts();

            foreach (var account in accounts)
            {
                //download and save report file 
                if (ReportDownloader.SaveReportToFile(account)) // 2           
                {   
                    //upload report data to date base
                    ReportUploader.UploadFileToDatabase(account); // 3           

                    //update account status to finished
                    account.Finished = true;
                }
            }

            ReportDownloader.Driver.Quit();

            //All inserts are done, calculate the daily changes
            CalculateDailyChanges();

            //Update last change date and save the daily change into google 
            GoogleManager.WriteAccouuntsUpdate(accounts);
        //    
        //    UpdateDailyChange();
        }

        //
        private static void CalculateDailyChanges() // 4
        {
            DataContext dbmanager = new DataContext();
            dbmanager.CalcualteDailyChanges();
        } 
        
        //
        private static void UpdateDailyChange() { } // 5

    }
}
