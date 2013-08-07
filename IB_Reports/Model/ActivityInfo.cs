using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IB_Reports.Helper
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
