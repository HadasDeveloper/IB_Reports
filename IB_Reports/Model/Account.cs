using System;

namespace IB_Reports.Model
{
    public class Account
    {
        public string AccountName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Link { get; set; }
        public string ProcessType { get; set; }
        public string Active { get; set; }
        public string Token { get; set; }
        public string Queryid { get; set; }
        public bool Finished  { get; set; }
        public DateTime DailyChangeDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public double DailyChange { get; set; }
    }
}
