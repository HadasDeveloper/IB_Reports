using System;
using System.Collections.Generic;
using System.Data;
using IB_Reports.Helper;
using Logger;

namespace IB_Reports.Model
{
    public class DataContext
    {
        readonly DataHelper dbHelper = new DataHelper();

        public void InsertReportsData(List<ReportInfo> reports)
        {
            foreach (var report in reports)
            {
                dbHelper.InsertReportsData(report.AccountName, report.AccountId, report.Date, report.Total, report.TotalLong, report.TotalShort);
            }
        }

        public void InsertActivitiesData(List<ActivityInfo> activities)
        {
            foreach (var activity in activities)
            {
                dbHelper.InsertActivitiesData(activity.AccountName, activity.AccountId, activity.Date, activity.ActivityDescription, activity.Amount);
            }
        }


        public void InsertProcessResult(string accountName, DateTime date, string success)
        {
            dbHelper.InsertProcessResult(accountName, date, success);
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
        public List<DailyChangeData> GetDailyChangesData(List<Account> accounts)
        {
            FileLogWriter logger = new FileLogWriter();

            string acountsNames = null;
            foreach (Account account in accounts)
            {
                acountsNames = acountsNames + "'" + account.AccountName + "',";
            }

            acountsNames = acountsNames.Remove(acountsNames.LastIndexOf(','));

            DataTable table = dbHelper.GetDailyChangesData(acountsNames);

            List<DailyChangeData> dailyChangeRows = new List<DailyChangeData>();

            foreach (DataRow row in table.Rows)
            {
                DailyChangeData dailyChangeRow = new DailyChangeData();

                try
                {
                    dailyChangeRow.Date = DateTime.Parse(row["date"].ToString());
                }
                catch (Exception e)
                {
                    logger.WriteToLog(DateTime.Now, string.Format("DataContext.GetDailyChangesData: {0}", e.Message), "IB_Log");
                }

                dailyChangeRow.AccountName = row["AccountName"].ToString();
                dailyChangeRow.Value = row["dailyChange"].ToString();

                dailyChangeRows.Add(dailyChangeRow);
            }

            return dailyChangeRows;
        }


        //get all the daily changes for specifics acounts
        //public List<DailyChangeData> GetDailyChangesData(List<Account> accounts)
        //{
        //    DataTable table = dbHelper.GetDailyChangesData(accounts);
        //    FileLogWriter logger = new FileLogWriter();

        //    List<DailyChangeData> dailyChangeData = new List<DailyChangeData>();

        //    foreach (DataRow row in table.Rows)
        //    {
        //        //get the date
        //        DailyChangeData data = new DailyChangeData();

        //        try
        //        {
        //            data.Date = DateTime.Parse(row["date"].ToString());
        //        }
        //        catch (Exception e  )
        //        {
        //            logger.WriteToLog(DateTime.Now, string.Format("DataContext.GetDailyChangesData: {0}", e.Message), "IB_Log");
        //        }

        //        //get the values
        //        data.Values = new List<string>();

        //        for(int i=1 ; i<= accounts.Count ; i++)
        //        {
        //            string value = row["account" + i].ToString();
        //            data.Values.Add(value);
        //        }
                    
        //        dailyChangeData.Add(data);
                
        //    }

        //    return dailyChangeData;
        //}
    }
}
