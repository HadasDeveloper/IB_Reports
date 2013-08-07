
namespace IB_Reports.Model
{
    public class StoredProcedures
    {
        public const string SqlInsertReportsData = "exec ups_IB_Report_update_ReportsData_table '{0}','{1}','{2: yyyy/MM/dd hh:mm:ss}',{3},{4},{5}";
       
        public const string SqlInsertActivityData = "insert into IB_Report_ActivitesData values ('{0}','{1}','{2: yyyy/MM/dd hh:mm:ss}','{3}',{4})";

        public const string SqlInsertProcessResult = "insert into IB_Report_ProcessSummary values('{0}','{1: yyyy/MM/dd hh:mm:ss}','{2}')";

        public const string SqlGetProcessSuccessAccountsNames = "select accountName from IB_Report_ProcessSummary where date = '{0: yyyy/MM/dd hh:mm:ss}'";

        public const string SqlTrancateActivitysDataRows = "delete from IB_Report_ActivitesData where date >= '{0: yyyy/MM/dd hh:mm:ss}' and AccountName = '{1}'";

        public const string SqlCalculateDailyChanges = "exec ups_IB_Report_Calculate_Daily_Change";

        public static string SqlGetGetDailyChanges = "select ds.AccountName, ds.dailyChange, CONVERT(VARCHAR(8), ds.date, 3) from IB_Report_DailySummary ds join IB_Report_ProcessSummary ps on ds.AccountName = ps.AccountName where ps.Date >= CAST(GETDATE() As date) and ps.Success = 'True' and ds.Date = (select max(date) from IB_Report_DailySummary)";

        public static string GetDailyChangesData = ""

    }

}
