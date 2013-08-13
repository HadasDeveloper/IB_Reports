using System;
using System.Collections.Generic;
using IB_Reports.Helper;
using IB_Reports.Model;

namespace IB_Reports
{
    public static class GoogleManager
    {
        public static List<Account> GetNotSuccessedAccounts(List<Account> accounts)
        {
            List<Account> notSuccessedAccounts = new List<Account>();

            foreach (Account account in accounts)
            {
                if (!account.LastUpdate.Date.Equals(DateTime.Today.Date))
                    notSuccessedAccounts.Add(account);
            }

            return notSuccessedAccounts;
        }
        
        public static void WriteAccouuntsUpdate(List<Account> accounts) 
        {
            GoogleSpreadSheetWriter.WriteDailyChanges(accounts);
        }
        
        public static void UpdateDailyProgress(List<Account> accounts)
        {
            GoogleSpreadSheetWriter.WriteLastUpdateDate(accounts);
        }
    }
}
