using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using Google.GData.Client;
using IB_Reports;

namespace IB_Reports.Helper
{
    public class GoogleSpreadSheethalper
    {

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
                   Logger.WriteToLog("GoogleSpreadSheethalper.GetCell: " + e.Message);
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

            for (int i = 0; i < allSpreadsheet.Entries.Count; i++)
                if (allSpreadsheet.Entries[i].Title.Text.Equals(fileName))
                    spreadsheetFile = (SpreadsheetEntry)allSpreadsheet.Entries[i];

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

            for (int i = 0; i < allTabs.Entries.Count; i++)
                if (allTabs.Entries[i].Title.Text.Equals(tabName))
                    tab = (WorksheetEntry)allTabs.Entries[i];

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
    }
}
