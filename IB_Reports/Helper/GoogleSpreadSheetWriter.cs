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

            string fileName =  ConfigurationManager.AppSettings["fileName"];
            string tabName;
            int startingRow;

            //connect to google service
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"],
                                       ConfigurationManager.AppSettings["accounts_password"]);


            //List<DbData> dailyChangeData = dbmanager.GetDailyData(accounts, "Changes");
            //tabName = ConfigurationManager.AppSettings["dailyChangeTab"];
            ///startingRow = 0;
            //WriteRowsToGoogleSheet(dailyChangeData, service, fileName, tabName);


            List<DbData> performence = dbmanager.GetDailyData(accounts, "Performance");
            tabName = ConfigurationManager.AppSettings["performanceTab"];
            //startingRow = 1;
            WriteRowsToGoogleSheet(performence, service, fileName, tabName);


        }

        private static void WriteRowsToGoogleSheet(List<DbData> data, SpreadsheetsService service, string fileName, string tabName)
        {
            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();
            FileLogWriter logger = new FileLogWriter();

            ListFeed listFeed = helper.GetListFeeds(service , fileName , tabName);
            CellFeed allCellsFeeds = helper.GetCellFeeds(service , fileName , tabName);                                         

            if (allCellsFeeds == null || listFeed == null)
                return;

            int startingIndex = listFeed.Entries.Count > 500 ? listFeed.Entries.Count - 500 : listFeed.Entries.Count;

            try
            {
                for (int i = startingIndex - 1 ; i >= 0; i--)
                    listFeed.Entries[i].Delete();
            }
            catch (Exception e)
            {
                logger.WriteToLog(DateTime.Now, "WriteDailyChanges.WriteRowsToGoogleSheet(): cant delete thith row google server rturnd an error: " + e.Message, ConfigurationManager.AppSettings["logFileName"]);
            }

            listFeed = helper.GetListFeeds(service, fileName, tabName);
            allCellsFeeds = helper.GetCellFeeds(service, fileName, tabName);         

            string previousdate = data[0].Column1;  //column1 = date or acountName , column2 = accountName or column
            const uint headRow = 1;
            uint currentRow = 1;
            uint currentColumn = 2;

            //update the header row
            foreach (DbData rowValue in data)
            {
                CellEntry updateCell;

                if (rowValue.Column1 == previousdate)
                {
                    updateCell = allCellsFeeds[currentRow, currentColumn];
                    updateCell.InputValue = rowValue.Column2;
                    updateCell.Update();
                    currentColumn++;
                }
            }

            previousdate = data[0].Column1;

            //insert data rows
            foreach (DbData rowValue in data)
            {
                CellEntry updateCell;

                if (rowValue.Column1 != previousdate || currentRow == 1)
                {
                    previousdate = rowValue.Column1;
                    currentColumn = 1;
                    currentRow++;

                    

                    if (allCellsFeeds.RowCount.Count < currentRow)
                    {
                        //add new row
                        ListEntry row = new ListEntry();

                        row.Elements.Add(new ListEntry.Custom { LocalName = allCellsFeeds[headRow, currentColumn].Value, Value = rowValue.Column1 });
                        service.Insert(listFeed, row);
                    }
                    else
                    {
                        //update existing  row
                        updateCell = allCellsFeeds[currentRow, currentColumn];
                        updateCell.InputValue = rowValue.Column1;
                        updateCell.Update();
                    }
                    

                }  
              
                currentColumn++;

                updateCell = allCellsFeeds[currentRow, currentColumn];

                if (updateCell != null)
                {
                    updateCell.InputValue = rowValue.Column3;
                    updateCell.Update();
                }
                else
                {
                    CellEntry newCell = new CellEntry(currentRow, currentColumn, rowValue.Column3);
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
            CellEntry errorCell = helper.GetCell("error", allCellsFeeds);

            //Loop through all the account names on google
            foreach (var account in accounts)
            {
                if (account.Error)
                {
                    
                    CellEntry accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds);
                    if (accountNameCell != null)
                    {
                        CellEntry updateCell = allCellsFeeds[accountNameCell.Row, errorCell.Column];
                        updateCell.InputValue = "problem whith generating the report";
                        updateCell.Update();

                    }
                    else
                    {
                        logger.WriteToLog(DateTime.Now, account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: not found this account name in \"xml report\" tab", "IB_Log");
                    }
                }

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
