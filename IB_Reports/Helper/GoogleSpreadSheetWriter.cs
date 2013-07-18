using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using Google.GData.Client;
using IB_Reports.Model;
using System.Configuration;

namespace IB_Reports.Helper
{
    public static class GoogleSpreadSheetWriter
    {

        //update last update date and save the daily changes
        public static void UpdateDailyProgress(string progress_username, string progress_pasword, List<Account> accounts)
        {
            //connect to google service
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"], ConfigurationManager.AppSettings["accounts_password"]);

            //Get daily progress summary details - from sql server
            DataContext dbmanager = new DataContext();
            dbmanager.GetDailyChanges(accounts);

            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();
            
            //Get all cells feeds from "Daily Change" and "xml Report" tabs 
            CellFeed allCellsFeeds1 = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"], ConfigurationManager.AppSettings["dailyChangeTab"]);
            CellFeed allCellsFeeds2 = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"], ConfigurationManager.AppSettings["accountsInfoTab"]);

            if (allCellsFeeds1 == null || allCellsFeeds2 == null)
                return;

            //Loop through all the account names on google
            foreach (Account account in accounts)
            { 
                if (account.Finished == true)
                {                       
                    //update last change date
                    CellEntry accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds2);
                    if (accountNameCell != null)
                    {
                        uint updateDateColumn = Convert.ToUInt16(ConfigurationManager.AppSettings["updateDateColumn"]);
                        CellEntry updateCell = allCellsFeeds2[accountNameCell.Row, updateDateColumn];
                        updateCell.InputValue = DateTime.UtcNow.ToString();
                        AtomEntry updatedCell = updateCell.Update();

                        Logger.WriteToLog(account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: update last change date");
                    }
                    else
                    {
                        Logger.WriteToLog(account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: not found this account name in \"xml report\" tab");
                    }

                    //Find the cell's (that going to be updated) colom and row 
                    accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds1);
                    CellEntry dateCell = helper.GetCell(Convert.ToString(account.DailyChangeDate.ToShortDateString()), allCellsFeeds1);

                    if (accountNameCell == null || dateCell == null)
                    {
                        Logger.WriteToLog(account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: account name could not be found in the \"Daily Change\" tab");
                        continue;
                    }

                    //Insert into google sheet the daily change                    
                    CellEntry newCell = new CellEntry(dateCell.Row, accountNameCell.Column, Convert.ToString(account.DailyChange));

                    allCellsFeeds1.Insert(newCell);

                    Logger.WriteToLog(account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: write daily change");
                }
            }
        }
    }
}
