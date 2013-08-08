using System;
using System.Collections.Generic;

namespace IB_Reports.Model
{
    public class DailyChangeData
    {
        public DateTime Date { get; set; }
        public List<string> Values { get; set; }
    }
}
