using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IB_Reports.Helper
{
    public class ReportInfo
    {
        public string AccountName { get; set; }
        public string AccountId { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public double TotalLong { get; set; }
        public double TotalShort { get; set; }
    }
}
