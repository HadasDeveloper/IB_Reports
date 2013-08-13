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
            
            List<Account> accounts = GoogleSpreadsSheetReader.ReadAcountsInfo();
            //Get the list of account information that have not been updated for this day
            List<Account> notSuccessedAccounts = GoogleManager.GetNotSuccessedAccounts(accounts);

            foreach (var account in notSuccessedAccounts)
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
        }
       
        private static void CalculateDailyChanges() // 4
        {
            DataContext dbmanager = new DataContext();
            dbmanager.CalcualteDailyChanges();
        } 
 

    }
}
