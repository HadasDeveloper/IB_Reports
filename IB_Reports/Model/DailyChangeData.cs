using System;
using System.Collections.Generic;

namespace IB_Reports.Model
{
    public class DailyChangeData
    {
        public DateTime Date { get; set; }
        public string AccountName { get; set; }
        public string Value { get; set; }
    }
}
