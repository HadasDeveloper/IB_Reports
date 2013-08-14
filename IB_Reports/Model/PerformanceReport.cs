using System.Data;

namespace IB_Reports.Model
{
    public class PerformanceReport
    {
        public DataTable ReportDataTable { get; set; }
        public DataTable ActivityDataTable { get; set; }
    }
}
