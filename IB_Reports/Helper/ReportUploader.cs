using System;
using System.Configuration;
using IB_Reports.Model;

namespace IB_Reports.Helper
{
    public static class ReportUploader
    {
        //public static List<PerformanceReport> PerformenceData;
        public static PerformanceReport PerformenceData;
        public static FileContex File = new FileContex();

        public static void UploadFileToDatabase(Account account)
        {
            //Upload the report to the database
            ReadDataAndSendToDataBase(account.AccountName, account.AccountID);
            UpdateUploadFileToDatabaseStatus(account.AccountName);
        }

        private static void UpdateUploadFileToDatabaseStatus(string accountName)
        {
            DataContext dbmanager = new DataContext();
            //Update the daily progress report with the current account name
            dbmanager.InsertProcessResult(accountName, DateTime.Today, "True");

        }

        private static void ReadDataAndSendToDataBase(string accountName, string accountId)
        {
            DataContext dbmanager = new DataContext();

            PerformenceData = File.GetReportInfo(string.Format(ConfigurationManager.AppSettings["IBReportUploaderPath"], accountId), accountName);

            dbmanager.InsertData(PerformenceData);

        }
    }
}
