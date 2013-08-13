using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace IB_Reports.Model
{
    public class FileContex
    {
        public List<PerformanceReport> GetReportInfo(string path, string accountName)
        {
            TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            List<ReportInfo> reports = new List<ReportInfo>();
            List<ActivityInfo> activities = new List<ActivityInfo>();

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
                        continue;
                    }
             
                    if (reportType == 1)
                    {
                        reports.Add(new ReportInfo
                                        {
                                            AccountName = accountName,
                                            AccountId = (row[0]),
                                            Date = Convert.ToDateTime(row[1]),
                                            Total =  Convert.ToDouble(row[2]),
                                            TotalLong = Convert.ToDouble(row[3]),
                                            TotalShort = Convert.ToDouble(row[4]),
                                        });
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

                        activities.Add(new ActivityInfo
                                           {
                                               AccountName = accountName,
                                               AccountId = (row[0]),
                                               Date = Convert.ToDateTime(row[1]),
                                               ActivityDescription = row[2],
                                               Amount = Convert.ToDouble(row[3]),
                                           });
                    }
                }
            }

            parser.Close();

            List<PerformanceReport> performance = new List<PerformanceReport>();
            performance.Add(new PerformanceReport
                    {
                        ReportData = reports,
                        ActivityData = activities
                    });
            
            return performance;

        }

    }
}
