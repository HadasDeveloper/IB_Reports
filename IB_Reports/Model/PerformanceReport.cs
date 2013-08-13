using System.Collections.Generic;
using IB_Reports.Helper;

namespace IB_Reports.Model
{
    public class PerformanceReport
    {
        public List<ReportInfo> ReportData { get; set; }
        public List<ActivityInfo> ActivityData { get; set; }
    }
}
