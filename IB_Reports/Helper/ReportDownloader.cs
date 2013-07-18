using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Configuration;


namespace IB_Reports
{
    public static class ReportDownloader
    {
        public static IWebDriver Driver = new FirefoxDriver(GetFFProfile());
        
        //private static readonly string path = ConfigurationManager.AppSettings["IBReportDownloadsPath"];

        public static bool SaveReportToFile(Account account) 
        {
            //Clean up the folder where we store the report 
            if (!CleanUp(ConfigurationManager.AppSettings["IBReportDownloadsPath"]))
                return false;

            if (account.ProcessType.Equals("Automatic"))
                return LogInManager.GetFileWithLogin(Driver, account);
            else
                return FlexFileManager.GetFlexFile(Driver, account);

        }

        private static bool CleanUp(string path)
        {
            DirectoryInfo downloadedMessageInfo = new DirectoryInfo(path);

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                try
                {
                    file.Delete();
                }
                catch(Exception e)
                {
                    return false;
                }

            foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                dir.Delete(true);

            return true;
        }

        private static FirefoxProfile GetFFProfile()
        {
            FirefoxProfileManager profileManager = new FirefoxProfileManager();
            FirefoxProfile profile = profileManager.GetProfile("WebDriver");
            return profile;
        }

    }
}
