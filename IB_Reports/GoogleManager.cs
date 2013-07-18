using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using IB_Reports.Helper;

namespace IB_Reports
{
    public static class GoogleManager
    {
        public static List<Account> GetNotSuccessedAccounts()
        {
            List<Account> accounts = GoogleSpreadsSheetReader.ReadAcountsInfo();
            List<Account> notSuccessedAccounts = new List<Account>();

            foreach (Account account in accounts)
            {
                if (!account.DailyChangeDate.Date.Equals(DateTime.Today.Date))
                    notSuccessedAccounts.Add(account);
            }

            return notSuccessedAccounts;
        }
        
        public static void WriteAccouuntsUpdate(List<Account> accounts) 
        { 
            //GoogleSpreadsSheetWriter.UpdateDailyProgress(progress_username, progress_pasword, accounts);
            GoogleSpreadSheetWriter.UpdateDailyProgress("", "", accounts);
        }
        
    }
}
