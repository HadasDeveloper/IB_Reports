using System;
using System.Collections.Generic;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using System.Configuration;
using IB_Reports.Model;
using Logger;

namespace IB_Reports.Helper
{
    public static class GoogleSpreadSheetWriter
    {

        ////update last update date and save the daily changes
        //public static void UpdateDailyProgress(List<Account> accounts)
        //{
        //    FileLogWriter logger = new FileLogWriter();

        //    //connect to google service
        //    SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
        //    service.setUserCredentials(ConfigurationManager.AppSettings["accounts_username"], ConfigurationManager.AppSettings["accounts_password"]);

        //    //Get daily progress summary details - from sql server
        //    DataContext dbmanager = new DataContext();
        //    dbmanager.GetDailyChanges(accounts);

        //    GoogleSpreadSheethalper helper = new GoogleSpreadSheethalper();

        //    //Get all cells feeds from "Daily Change" and "xml Report" tabs 
        //    CellFeed allCellsFeeds1 = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"], ConfigurationManager.AppSettings["dailyChangeTab"]);
        //    CellFeed allCellsFeeds2 = helper.GetCellFeeds(service, ConfigurationManager.AppSettings["fileName"], ConfigurationManager.AppSettings["accountsInfoTab"]);

        //    if (allCellsFeeds1 == null || allCellsFeeds2 == null)
        //        return;

        //    //Loop through all the account names on google
        //    foreach (Account account in accounts)
        //    {
        //        if (account.Finished)
        //        {
        //            //update last update date
        //            CellEntry accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds2);
        //            if (accountNameCell != null)
        //            {
        //                uint updateDateColumn = Convert.ToUInt16(ConfigurationManager.AppSettings["updateDateColumn"]);
        //                CellEntry updateCell = allCellsFeeds2[accountNameCell.Row, updateDateColumn];
        //                updateCell.InputValue = DateTime.UtcNow.ToString();
        //                updateCell.Update();

        //                logger.WriteToLog(DateTime.Now, account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: update last change date", "IB_Log");
        //            }
        //            else
        //            {
        //                logger.WriteToLog(DateTime.Now, account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: not found this account name in \"xml report\" tab", "IB_Log");
        //            }

        //            //Find the cell's (that going to be updated) colom and row 
        //            accountNameCell = helper.GetCell(account.AccountName, allCellsFeeds1);

        //            CellEntry dateCell = helper.GetCell(account.DailyChangeDate.ToString("d", CultureInfo.CreateSpecificCulture("en-US")), allCellsFeeds1);

        //            if (accountNameCell == null || dateCell == null)
        //            {
        //                logger.WriteToLog(DateTime.Now, account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: account name could not be found in the \"Daily Change\" tab", "IB_Log");
        //                continue;
        //            }

        //            //Insert into google sheet the daily change                    
        //            CellEntry newCell = new CellEntry(dateCell.Row, accountNameCell.Column, Convert.ToString(account.DailyChange));

        //            allCellsFeeds1.Insert(newCell);

        //            logger.WriteToLog(DateTime.Now, account.AccountName + ": GoogleSpreadSheetWriter.UpdateDailyProgress: write daily change", "IB_Log");
        //        }
        //    }
        //}


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
            CellEntry startingCell = helper.GetCell("account name", allCellsFeeds);
            if (startingCell == null)
            {
                logger.WriteToLog(DateTime.Now,
                                  ": GoogleSpreadSheetWriter.WriteDailyChanges: cant find \"account name\" cell ",
                                  "IB_Log");
                return;
            }

            uint startRow = startingCell.Row + 1;
            uint startColumn = startingCell.Column;

            uint numberOfColumns = (uint)data[0].Values.Count + startColumn;

            // Check the size of the google sheet, to allow for all the data we have
            if (allCellsFeeds.RowCount.Count < data.Count ||
                allCellsFeeds.ColCount.Count < numberOfColumns)
                AddRowsToGoogleSheet();

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

        private static void AddRowsToGoogleSheet(int rows, int columns, object tabReferrence)
        {
            AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = myService.Query(listQuery);

            ListEntry row = new ListEntry();

            row.Elements.Add(new ListEntry.Custom() { LocalName = "Name", Value = "Joe" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "my-val1", Value = "Smith" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "my-val2", Value = "26" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "my-val3", Value = "176" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "Other", Value = "176" });

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
