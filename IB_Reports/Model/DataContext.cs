using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

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
    }
}
