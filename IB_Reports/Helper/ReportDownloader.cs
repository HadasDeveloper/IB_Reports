using System;
using System.IO;
using IB_Reports.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Configuration;

namespace IB_Reports.Helper
{
    public static class ReportDownloader
    {
        public static IWebDriver Driver = new FirefoxDriver(GetFfProfile());
        //private static readonly string path = ConfigurationManager.AppSettings["IBReportDownloadsPath"];

        public static bool SaveReportToFile(Account account)
        {
            //Clean up the folder where we store the report 
            if (!CleanUp(ConfigurationManager.AppSettings["IBReportDownloadsPath"]))
                return false;

            
            return account.ProcessType.Equals("Automatic")
                       ? LogInManager.GetFileWithLogin(Driver, account)
                       : FlexFileManager.GetFlexFile(Driver, account);
        }

        private static bool CleanUp(string path)
        {
            DirectoryInfo downloadedMessageInfo = new DirectoryInfo(path);

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                try
                {
                    file.Delete();
                }
                catch(Exception)
                {
                    return false;
                }

            foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                dir.Delete(true);

            return true;
        }

        private static FirefoxProfile GetFfProfile()
        {
            FirefoxProfileManager profileManager = new FirefoxProfileManager();
            FirefoxProfile profile = profileManager.GetProfile("WebDriver");
            return profile;
        }

    }
}
