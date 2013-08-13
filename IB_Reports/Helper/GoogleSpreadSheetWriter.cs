using System;
using System.Collections.Generic;
using Google.GData.Spreadsheets;
using System.Configuration;
using IB_Reports.Model;
using Logger;

namespace IB_Reports.Helper
{
    public static class GoogleSpreadSheetWriter
    {

        public static void WriteDailyChanges(List<Account> accounts)
        {
            DataContext dbmanager = new DataContext();
        
            //connect to google service
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"],
                                       ConfigurationManager.AppSettings["accounts_password"]);

            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();

            ListFeed listFeed = helper.GetListFeeds(service, ConfigurationManager.AppSettings["fileName"],
                                            ConfigurationManager.AppSettings["dailyChangeTab"]);

            int startingIndex = listFeed.Entries.Count > 500 ? listFeed.Entries.Count - 500 : listFeed.Entries.Count;

            for (int i = startingIndex-1; i >= 0; i--)
                listFeed.Entries[i].Delete();

            //Get all cells feeds from "Daily Change" tab
            CellFeed allCellsFeeds = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"],
                                                         ConfigurationManager.AppSettings["dailyChangeTab"]);

           
            if (allCellsFeeds == null)
                return;

            List<DailyChangeData> data = dbmanager.GetDailyChangesData(accounts);

            WriteRowsToGoogleSheet(service, data, allCellsFeeds, listFeed);

        }

        private static void WriteRowsToGoogleSheet(SpreadsheetsService service, List<DailyChangeData> data, CellFeed allCellsFeeds, ListFeed listFeed)
        {
            DateTime previousdate = data[0].Date;
            const uint headRow = 1;
            uint currentRow= 1;
            uint currentColumn = 2;

            //update accounts names in the header row
            foreach (DailyChangeData rowValue in data)
            {
                CellEntry updateCell;

                if (rowValue.Date == previousdate)
                {
                    updateCell = allCellsFeeds[currentRow, currentColumn];
                    updateCell.InputValue = rowValue.AccountName;
                    updateCell.Update();
                    currentColumn++;
                }
            }

            previousdate = data[0].Date;

            //insert data rows
            foreach (DailyChangeData rowValue in data)
            {
                CellEntry updateCell;

                if (rowValue.Date != previousdate || currentRow == 1)
                {
                    previousdate = rowValue.Date;
                    currentColumn = 1;
                    currentRow++;

                    if (allCellsFeeds.RowCount.Count < currentRow)
                    {
                        //add new row
                        ListEntry row = new ListEntry();
                        row.Elements.Add(new ListEntry.Custom { LocalName = allCellsFeeds[headRow, currentColumn].Value.Replace(" ", "") , Value = rowValue.Date.ToShortDateString() });
                        service.Insert(listFeed, row);
                    }
                    else
                    {
                        //update existing  row
                        updateCell = allCellsFeeds[currentRow, currentColumn];
                        updateCell.InputValue = rowValue.Date.ToShortDateString();
                        updateCell.Update();
                    }
                }  
              
                currentColumn++;
                updateCell = allCellsFeeds[currentRow, currentColumn];

                if (updateCell != null)
                {
                    updateCell.InputValue = rowValue.Value;//confirm the account name before writting????
                    updateCell.Update();
                }
                else
                {
                    CellEntry newCell = new CellEntry(currentRow, currentColumn, rowValue.Value);
                    allCellsFeeds.Insert(newCell);
                }             
            }            
            
        }

        public static void WriteLastUpdateDate(List<Account> accounts)
        {
            FileLogWriter logger = new FileLogWriter();

            //connect to google service
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"],
                                       ConfigurationManager.AppSettings["accounts_password"]);

            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();

            //Get all cells feeds from  "xml Report" tab  
            CellFeed allCellsFeeds = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"],ConfigurationManager.AppSettings["accountsInfoTab"]);
         

            if (allCellsFeeds == null)
                return;

            CellEntry lastUpdateCell = helper.GetCell("last update", allCellsFeeds);

            //Loop through all the account names on google
            foreach (var account in accounts)
            {
                if (account.Finished)
                {
                    //update last update date
                    CellEntry accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds);
                    if (accountNameCell != null)
                    { 
                        CellEntry updateCell = allCellsFeeds[accountNameCell.Row, lastUpdateCell.Column];
                        updateCell.InputValue = DateTime.UtcNow.ToShortDateString();
                        updateCell.Update();

                        logger.WriteToLog(DateTime.Now,account.AccountName +": GoogleSpreadSheetWriter.UpdateDailyProgress: update last change date","IB_Log");
                    }
                    else
                    {
                        logger.WriteToLog(DateTime.Now, account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: not found this account name in \"xml report\" tab","IB_Log");
                    }
                }
            }
            
            
        }

    }
}
