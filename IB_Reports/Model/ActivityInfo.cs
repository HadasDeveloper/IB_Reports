using System;

namespace IB_Reports.Model
{
    public class ActivityInfo
    {
        public string AccountName { get; set; }
        public string AccountId { get; set; }
        public DateTime Date { get; set; }
        public string ActivityDescription { get; set; }
        public double Amount { get; set; }
    }
}
