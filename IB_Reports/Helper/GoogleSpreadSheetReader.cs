using System;
using System.Configuration;
using System.Collections.Generic;
using Google.GData.Spreadsheets;
using IB_Reports.Model;
using Logger;


namespace IB_Reports.Helper
{
    public class GoogleSpreadsSheetReader
    {
        private const uint TitlesRow = 3;
      

        public static List<Account> ReadAcountsInfo()
        {  
            FileLogWriter logger = new FileLogWriter();

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
                if (entry.Row <= TitlesRow)
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
                        account.AccountID = entry.Value;
                        break;
                    case 5:
                        account.Login = entry.Value;
                        break;
                    case 6:
                        account.Password = entry.Value;
                        break;
                    case 7:
                        account.Token = entry.Value;
                        break;
                    case 8:
                        account.Queryid = entry.Value;
                        break;
                    case 9:
                        account.Link = entry.Value;
                        break;
                    case 13:
                        account.ProcessType = entry.Value;
                        break;
                    case 14:
                        account.Active = entry.Value;
                        break;
                    case 16:
                        if (!entry.Value.Equals("0"))
                            try
                            {
                                //CultureInfo culture = new CultureInfo("en-US");
                                account.LastUpdate = Convert.ToDateTime(entry.Value);//, culture);

                            }
                            catch (Exception e)
                            {
                                logger.WriteToLog(DateTime.Now, string.Format("ReadAcountsInfo: {0}" , e.Message), ConfigurationManager.AppSettings["logFileName"]);

                            }
                            
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

