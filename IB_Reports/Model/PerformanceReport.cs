using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IB_Reports.Model
{
    public class PerformanceReport
    {
        public List<ReportInfo> ReportData { get; set; }
        public List<ActivityInfo> ActivityData { get; set; }
    }
}
