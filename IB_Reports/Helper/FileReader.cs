using System.IO;
using OpenQA.Selenium.Firefox;

namespace IB_Reports.Helper
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
            text = ReadFile(path);
            string[] textRows = text.Split('\n');

            return textRows;
        }

        public string[] GetTextRows(FirefoxDriver driver)
        {
            int startIndex = driver.PageSource.IndexOf("<pre>");
            int endIndex = driver.PageSource.IndexOf("</pre>");
            string text = driver.PageSource.Substring(startIndex + 5, endIndex - (startIndex + 5));

            string[] textRows = text.Split('\n');

            return textRows;
        }
  
    }
}
