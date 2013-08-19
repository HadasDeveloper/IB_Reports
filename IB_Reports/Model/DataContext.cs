using System;
using System.Collections.Generic;
using System.Data;
using IB_Reports.Helper;

namespace IB_Reports.Model
{
    public class DataContext
    {
        readonly DataHelper dbHelper = new DataHelper();

        public void InsertProcessResult(string accountName, DateTime date, string success)
        {
            dbHelper.InsertProcessResult(accountName, date, success);
        }

         public void InsertData(PerformanceReport data)
         {
             dbHelper.InsertData(data);
         }


        public List<string> GetProcessSuccessAccountsNames()
        {
            List<string> accountsNames = new List<string>();
 
            DataTable table = dbHelper.GetProcessSuccessAccountsNames();

            foreach (DataRow row in table.Rows)
                accountsNames.Add((string)row["accountName"]);
  
            return accountsNames;
        }


        //delete rows until the min day that going to be inserted
        public void TrancateActivitysDataRows(List<ActivityInfo> activities)
        {
            //search for min date
            DateTime date = activities[0].Date; 
            for (int i = 1; i < activities.Count; i++)
                if (date.CompareTo(activities[i].Date) > 0)
                    date = activities[i].Date;

            dbHelper.TrancateActivitysDataRows(activities[0].AccountName, date);
        }

        public void CalcualteDailyChanges()
        {
            dbHelper.CalcualteDailyChanges();
        }

        //get all the last daily changes for specifics acounts
        internal void GetDailyChanges(List<Account> accounts)
        {
           DataTable table = dbHelper.GetDailyChanges();

           for (int i = 0; i < table.Rows.Count; i++)
               for (int j = 0; j < accounts.Count; j++)
               {
                   if (table.Rows[i][0].Equals(accounts[j].AccountName))
                   {
                       accounts[j].DailyChange = Convert.ToDouble(table.Rows[i][1]);
                       accounts[j].DailyChangeDate = Convert.ToDateTime(table.Rows[i][2]).Date;
                       break;
                   }
               }
        }

        //get all the daily changes for specifics acounts
        public List<DbData> GetDailyData(List<Account> accounts, string dataSort)
        {
            string acountsNames = null;

            foreach (Account account in accounts)
                acountsNames = acountsNames + "''" + account.AccountName + "'',";

            if (acountsNames != null)
                acountsNames = acountsNames.Remove(acountsNames.LastIndexOf(','));

            DataTable table;

            switch (dataSort)
            {
                case("Changes") :   table = dbHelper.GetDailyChangesData(acountsNames);
                                break;
                case("Performance") :  table = dbHelper.GetDailyPerformanceData(acountsNames);
                                break;
                default :
                    return null;
            }

            List<DbData> dbData = new List<DbData>();

            foreach (DataRow row in table.Rows)
            {
                DbData dbDatarow = new DbData
                                            {
                                                Column1 = row["column1"].ToString(),
                                                Column2 = row["column2"].ToString(),
                                                Column3 = row["column3"].ToString()
                                            };

                dbData.Add(dbDatarow);
            }

            return dbData;
        }

     
    }
}
