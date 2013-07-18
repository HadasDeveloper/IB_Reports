using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IB_Reports.Model;
using System.Configuration;

namespace IB_Reports.Helper
{
    public static class ReportUploader
    {
        public static List<PerformanceReport> performenceData;
        public static FileContex file = new FileContex();

        public static void UploadFileToDatabase(Account account)
        {
            //Upload the report to the database
            ReadDataAndSendToDataBase(account.AccountName);
            UpdateUploadFileToDatabaseStatus(account.AccountName);
        }

        private static void UpdateUploadFileToDatabaseStatus(string accountName)
        {
            DataContext dbmanager = new DataContext();
            //Update the daily progress report with the current account name
            dbmanager.InsertProcessResult(accountName, DateTime.Today, "True");

        }

        private static void ReadDataAndSendToDataBase(string accountName)
        {
            DataContext dbmanager = new DataContext();

            performenceData = file.GetReportInfo(ConfigurationManager.AppSettings["IBReportUploaderPath"], accountName);

            for (int j = 0; j < performenceData.Count; j++)
            {
                dbmanager.InsertReportsData(performenceData[j].ReportData);

                if (performenceData[j].ActivityData.Count > 0)
                    dbmanager.TrancateActivitysDataRows(performenceData[j].ActivityData);

                dbmanager.InsertActivitiesData(performenceData[j].ActivityData);

            }
        }
    }
}
