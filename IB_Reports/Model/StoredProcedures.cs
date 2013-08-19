
namespace IB_Reports.Model
{
    public class StoredProcedures
    {
        public const string SqlInsertProcessResult = "insert into IB_Report_ProcessSummary values('{0}','{1: yyyy/MM/dd hh:mm:ss}','{2}')";

        public const string SqlInsertDate = "usp_IB_Report_Insert_Performance_Data";
        
        public const string SqlGetProcessSuccessAccountsNames = "select accountName from IB_Report_ProcessSummary where date = '{0: yyyy/MM/dd hh:mm:ss}'";

        public const string SqlTrancateActivitysDataRows = "delete from IB_Report_ActivitesData where date >= '{0: yyyy/MM/dd hh:mm:ss}' and AccountName = '{1}'";

        public const string SqlCalculateDailyChanges = "exec usp_IB_Report_Calculate_Daily_Change";

        public static string SqlGetGetDailyChanges = "select ds.AccountName, ds.dailyChange, CONVERT(VARCHAR(8), ds.date, 3) from IB_Report_DailySummary ds join IB_Report_ProcessSummary ps on ds.AccountName = ps.AccountName where ps.Date >= CAST(GETDATE() As date) and ps.Success = 'True' and ds.Date = (select max(date) from IB_Report_DailySummary)";

        public static string SqlGetDailyChangesData = "usp_IB_Report_get_daily_changes '{0}'";

        public static string SqlGetDailyPerformanceData = "ups_IB_Report_get_daily_performance '{0}'";

    }

}
