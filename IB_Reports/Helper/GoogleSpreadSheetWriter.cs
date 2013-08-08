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

            List<DailyChangeData> data = dbmanager.GetDailyChangesData(accounts);


            FileLogWriter logger = new FileLogWriter();

            //connect to google service
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"],
                                       ConfigurationManager.AppSettings["accounts_password"]);

            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();

            //Get all cells feeds from "Daily Change" tab
            CellFeed allCellsFeeds = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"],
                                                         ConfigurationManager.AppSettings["dailyChangeTab"]);
            if (allCellsFeeds == null)
                return;

            //Find the cell which is a starting point for writing the data
//           CellEntry startingCell = helper.GetCell("accountname", allCellsFeeds);
            CellEntry startingCell = allCellsFeeds[1, 1];

            if (startingCell == null)
            {
                logger.WriteToLog(DateTime.Now,
                                  ": GoogleSpreadSheetWriter.WriteDailyChanges: cant find string cell: \"account name\" ",
                                  "IB_Log");
                return;
            }

            uint startRow = startingCell.Row + 1;
            uint startColumn = startingCell.Column;

            uint numberOfColumns = (uint)data[0].Values.Count + startColumn;

            // Check the size of the google sheet, to allow for all the data we have
            if (allCellsFeeds.RowCount.Count < data.Count ||
                allCellsFeeds.ColCount.Count < numberOfColumns)
                AddRowsToGoogleSheet(startingCell.Value);

            foreach (DailyChangeData row in data)
            {
                    CellEntry updateCell = allCellsFeeds[startRow,startColumn]; //write the date
                    
                    if(updateCell!=null)
                    {
                        updateCell.InputValue = row.Date.ToShortDateString();
                        updateCell.Update();

                        for (int i = 2; i <= row.Values.Count ; i++ )
                        {
                            updateCell = allCellsFeeds[startRow, startColumn + (uint)i];//write the Value1
                            updateCell.InputValue = row.Values[i];
                            updateCell.Update();
                        }

                    }
                    else
                    {
                        CellEntry newCell = new CellEntry(startRow, startColumn, Convert.ToString(row.Date.ToShortDateString()));               
                        allCellsFeeds.Insert(newCell);

                        for (int i = 2; i < row.Values.Count; i++)
                        {   
                            newCell = new CellEntry(startRow, startColumn + (uint)i, row.Values[i]);
                            allCellsFeeds.Insert(newCell);
                        }
                    }

                    startRow++;
  
            }

            UpdateDailyProgress(accounts);

        }

        //private static void AddRowsToGoogleSheet(int rows, int columns, object tabReferrence)
        private static void AddRowsToGoogleSheet(string startingCell)
        {
            //connect to google service
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"],
                                       ConfigurationManager.AppSettings["accounts_password"]);

            GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();
            ListFeed listFeed = helper.GetListFeeds(service, ConfigurationManager.AppSettings["fileName"],
                                             ConfigurationManager.AppSettings["dailyChangeTab"]);

            ListEntry row = new ListEntry();


            row.Elements.Add(new ListEntry.Custom() { LocalName = startingCell, Value = "a" });
            //row.Elements.Add(new ListEntry.Custom() { LocalName = "davidbush", Value = "3" });
            //row.Elements.Add(new ListEntry.Custom() { LocalName = "graemesmith", Value = "4" });
            //row.Elements.Add(new ListEntry.Custom() { LocalName = "markangil", Value = "5" });
            //row.Elements.Add(new ListEntry.Custom() { LocalName = "timfligg", Value = "" });
            //row.Elements.Add(new ListEntry.Custom() { LocalName = "ab", Value = "" });
             
            service.Insert(listFeed, row);
        }

        public static void UpdateDailyProgress(List<Account> accounts)
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

            //Loop through all the account names on google
            foreach (var account in accounts)
            {
                if (account.Finished)
                {
                    //update last update date
                    CellEntry accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds);
                    if (accountNameCell != null)
                    {
                        uint updateDateColumn = Convert.ToUInt16(ConfigurationManager.AppSettings["updateDateColumn"]);
                        CellEntry updateCell = allCellsFeeds[accountNameCell.Row, updateDateColumn];
                        updateCell.InputValue = DateTime.UtcNow.ToString();
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
