using System;

namespace IB_Reports.Model
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
