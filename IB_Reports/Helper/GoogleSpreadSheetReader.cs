using System;
using System.Configuration;
using System.Collections.Generic;
using IB_Reports;
using Google.GData.Client;
using Google.GData.Spreadsheets;


namespace IB_Reports.Helper
{
    public class GoogleSpreadsSheetReader
    {
        private const uint titlesRow = 3;

        public static List<Account> ReadAcountsInfo()
        {
            SpreadsheetsService service = new SpreadsheetsService(ConfigurationManager.AppSettings["service"]);
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"], ConfigurationManager.AppSettings["accounts_password"]);

            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();
            CellFeed allCellsFeeds = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"], ConfigurationManager.AppSettings["accountsInfoTab"]);

            if(allCellsFeeds == null)
                return new List<Account>();

            List<Account> accounts = new List<Account>();
            uint currentRow = 0;

            Account account = new Account();
            foreach (CellEntry entry in allCellsFeeds.Entries)
            {
                if (entry.Row <= titlesRow)
                    continue;

                if (!entry.Row.Equals(currentRow))
                {
                    if(account.AccountName!=null)
                        accounts.Add(account);
                    account = new Account();
                    currentRow = entry.Row;
                }

                switch (entry.Column)
                {
                    case 3:
                        account.AccountName = entry.Value;
                        break;
                    case 4:
                        account.Login = entry.Value;
                        break;
                    case 5:
                        account.Password = entry.Value;
                        break;
                    case 6:
                        account.Token = entry.Value;
                        break;
                    case 7:
                        account.Queryid = entry.Value;
                        break;
                    case 8:
                        account.Link = entry.Value;
                        break;
                    case 12:
                        account.ProcessType = entry.Value;
                        break;
                    case 13:
                        account.Active = entry.Value;
                        break;
                    case 14:
                        if (!entry.Value.Equals("0"))
                            account.DailyChangeDate = Convert.ToDateTime(entry.Value);
                        break;
                    default:
                        break;
                }

            }

            if (account.AccountName != null)
                accounts.Add(account);

            return accounts;
        }      

    }

}

