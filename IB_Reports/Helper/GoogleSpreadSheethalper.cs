using System;
using Google.GData.Spreadsheets;
using Google.GData.Client;
using Logger;

namespace IB_Reports.Helper
{
    public class GoogleSpreadSheethalper
    {
        readonly FileLogWriter logger = new FileLogWriter();

        //find specific cell by value
        public CellEntry GetCell(string value, CellFeed cells)
        {
            foreach (CellEntry entry in cells.Entries)
            {
                try
                {
                    if (entry.Value.Contains(value))
                        return entry;
                }
                catch (Exception e)
                {
                   logger.WriteToLog(DateTime.Now, "GoogleSpreadSheethalper.GetCell: " + e.Message,"IB_Log");
                   continue;
                }
            }

            return null;
        }

        //get all the celles in the specific tab
        public CellFeed GetCellFeeds(SpreadsheetsService service, string fileName, string tabName)
        {
            //retrieve available spreadsheets
            SpreadsheetFeed allSpreadsheet = service.Query(new SpreadsheetQuery());

            if (allSpreadsheet.Entries.Count == 0)
            {
                Console.WriteLine(" the drive is empty - 0 spreadsheets files");
                return null;
            }

            SpreadsheetEntry spreadsheetFile = null;

            foreach (AtomEntry t in allSpreadsheet.Entries)
                if (t.Title.Text.Equals(fileName))
                    spreadsheetFile = (SpreadsheetEntry)t;

            if (spreadsheetFile == null)
            {
                Console.WriteLine(" requested file not found in the drive");
                return null;
            }

            //retrieve all the sheets
            AtomLink spreadSheetLink = spreadsheetFile.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);

            WorksheetQuery getAllTabs = new WorksheetQuery(spreadSheetLink.AbsoluteUri);
            WorksheetFeed allTabs = service.Query(getAllTabs);

            WorksheetEntry tab = null; 

            foreach (AtomEntry t in allTabs.Entries)
                if (t.Title.Text.Equals(tabName))
                    tab = (WorksheetEntry)t;

            if (tab == null)
            {
                Console.WriteLine(" tab was not found in the file");
                return null;
            }

            //retrieve the particular sheet
            AtomLink tabLink = tab.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null);

            CellQuery getAllCellsQuery = new CellQuery(tabLink.AbsoluteUri);
            CellFeed allCellsFeeds = service.Query(getAllCellsQuery);

            return allCellsFeeds;
        }
        //get List fedd in the specific tab
        public ListFeed GetListFeeds(SpreadsheetsService service, string fileName, string tabName)
        {
            //retrieve available spreadsheets
            SpreadsheetFeed allSpreadsheet = service.Query(new SpreadsheetQuery());

            if (allSpreadsheet.Entries.Count == 0)
            {
                Console.WriteLine(" the drive is empty - 0 spreadsheets files");
                return null;
            }

            SpreadsheetEntry spreadsheetFile = null;

            foreach (AtomEntry t in allSpreadsheet.Entries)
                if (t.Title.Text.Equals(fileName))
                    spreadsheetFile = (SpreadsheetEntry)t;

            if (spreadsheetFile == null)
            {
                Console.WriteLine(" requested file not found in the drive");
                return null;
            }

            //retrieve all the sheets
            AtomLink spreadSheetLink = spreadsheetFile.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);

            WorksheetQuery getAllTabs = new WorksheetQuery(spreadSheetLink.AbsoluteUri);
            WorksheetFeed allTabs = service.Query(getAllTabs);

            WorksheetEntry tab = null;

            foreach (AtomEntry t in allTabs.Entries)
                if (t.Title.Text.Equals(tabName))
                    tab = (WorksheetEntry)t;

            if (tab == null)
            {
                Console.WriteLine(" tab was not found in the file");
                return null;
            }

            AtomLink listFeedLink = tab.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);


            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);

            return listFeed;

        }

    }
}
