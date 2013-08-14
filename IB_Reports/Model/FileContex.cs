using System;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace IB_Reports.Model
{
    public class FileContex
    {
        //public List<PerformanceReport> GetReportInfo(string path, string accountName)
        public PerformanceReport GetReportInfo(string path, string accountName)
        {
            TextFieldParser parser = new TextFieldParser(path)
                                         {
                                             TextFieldType = FieldType.Delimited,
                                             HasFieldsEnclosedInQuotes = true
                                         };
            parser.SetDelimiters(",");

            DataTable reportsDataTable = new DataTable();
            DataTable activityDataTable = new DataTable();


            reportsDataTable.Columns.Add("accountName");
            reportsDataTable.Columns.Add("AccountId");
            reportsDataTable.Columns.Add("Date");
            reportsDataTable.Columns.Add("Total");
            reportsDataTable.Columns.Add("TotalLong");
            reportsDataTable.Columns.Add("TotalShort");

            activityDataTable.Columns.Add("accountName");
            activityDataTable.Columns.Add("AccountId");
            activityDataTable.Columns.Add("Date");
            activityDataTable.Columns.Add("ActivityDescription");
            activityDataTable.Columns.Add("Amount");


            int reportType = 0;

            //loop while we still have more row
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields();

                if (row != null)
                {

                    if (row[1].Equals("Date") || row[1].Equals("ReportDate") || row[1].Equals("Report Date")) 
                    {
                        reportType++;
                        Console.WriteLine(accountName + " " + reportType);

                        if(reportType == 3)
                            reportType = 1;

                        continue;
                    }
             
                    if (reportType == 1)
                    {
                        reportsDataTable.Rows.Add(accountName, row[0], row[1],
                                                   Convert.ToDouble(row[2]), Convert.ToDouble(row[3]), Convert.ToDouble(row[4]));
                    }

                    if (reportType == 2)
                    {                    
                        if (row[2].Contains("'"))
                        {
                            int index = row[2].IndexOf("'");
                            row[2] = row[2].Insert(index, "'");
                        }

                        if (row[0].Equals(""))
                            continue;

                        activityDataTable.Rows.Add(accountName, row[0], row[1], row[2],
                                                  Convert.ToDouble(row[3]));
                        
                    }
                }
            }

            parser.Close();

            PerformanceReport performance = new PerformanceReport
                                                {
                                                    ActivityDataTable = activityDataTable,
                                                    ReportDataTable = reportsDataTable
                                                };
            return performance;

        }

    }
}
