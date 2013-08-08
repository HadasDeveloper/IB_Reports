using System.Collections.Generic;
using IB_Reports.Helper;
using IB_Reports.Model;

namespace IB_Reports
{
    public static class ProcessManager
    {

        //Start the process
        public static void Start()
        {
            //Get from google the list of account information that have not been updated for this day
            List<Account> accounts = GoogleManager.GetNotSuccessedAccounts();

            //Account account = accounts[3];

            //foreach (var account in accounts)

            //for (int i = 1; i <= 3; i++)
            //{
            //    //download and save report file 
            //    if (ReportDownloader.SaveReportToFile(accounts[i])) // 2           
            //    {
            //        //upload report data to date base
            //        ReportUploader.UploadFileToDatabase(accounts[i]); // 3           

            //        //update account status to finished
            //        accounts[i].Finished = true;
            //    }
            //}

            ReportDownloader.Driver.Quit();

            //All inserts are done, calculate the daily changes
            CalculateDailyChanges();

            //Update last change date and save the daily change into google 
            GoogleManager.WriteAccouuntsUpdate(accounts);
       
        }

        //
        private static void CalculateDailyChanges() // 4
        {
            DataContext dbmanager = new DataContext();
            dbmanager.CalcualteDailyChanges();
        } 
 

    }
}
