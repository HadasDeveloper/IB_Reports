using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenQA.Selenium.Firefox;

namespace IB_Reports.Model
{
    class FileReader
    {
        private string text;

        public static string ReadFile(string filePath)
        { 
            StreamReader streamReader = new StreamReader(filePath);
            string text = streamReader.ReadToEnd();
            streamReader.Close();

            return text;
        }

        public string[] GetTextRows(string path)
        {
            string[] textRows;

            text = ReadFile(path);
            textRows = text.Split('\n');

            return textRows;
        }

        public string[] GetTextRows(FirefoxDriver driver)
        {
            string[] textRows;

            int startIndex = driver.PageSource.IndexOf("<pre>");
            int endIndex = driver.PageSource.IndexOf("</pre>");
            string text = driver.PageSource.Substring(startIndex + 5, endIndex - (startIndex + 5));

            textRows = text.Split('\n');

            return textRows;
        }
  
    }
}
