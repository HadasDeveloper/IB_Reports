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
            int attemptCounter = 0;

            List<Account> accounts = new List<Account>();

            //loop through the accounts up to five times to finish them all
            while (attemptCounter < 5)    
            {
                //Get the list of account information that have not been updated for this day

               accounts = GoogleSpreadsSheetReader.ReadAcountsInfo();
               List<Account> notSuccessedAccounts = GoogleManager.GetNotSuccessedAccounts(accounts);

                if(notSuccessedAccounts.Count == 0) 
                    break;

                foreach (var account in notSuccessedAccounts)
                {
                    //download and save report file 
//                    if (ReportDownloader.SaveReportToFile(account)) // 2           
                    {
                        //upload report data to date base
                        ReportUploader.UploadFileToDatabase(account); // 3           

                        //update account status to finished
                        account.Finished = true;
                    }
                }

                //set the last update date of the successed accounts to the current date
                GoogleManager.UpdateDailyProgress(notSuccessedAccounts);
                attemptCounter++;
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
