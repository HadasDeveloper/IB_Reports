using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using IB_Reports.Helper;
using System.Configuration;

namespace IB_Reports
{
    public static class FlexFileManager
    {
        public static bool GetFlexFile(IWebDriver driver, Account account)
        {
            //initial url, response will have the code
            driver.Navigate().GoToUrl(string.Format("{0}{1}&q={2}&v=2", ConfigurationManager.AppSettings["baseUrl"], account.Token, account.Queryid));

            //find reference code
            int startIndex = driver.PageSource.IndexOf("<code>");
            int endIndex = driver.PageSource.IndexOf("</code>");
            string reference_code = driver.PageSource.Substring(startIndex + 6, endIndex - (startIndex + 6));

            string pageUrl = string.Format("{0}{1}&t={2}&v=2", ConfigurationManager.AppSettings["baseUr2"], reference_code, account.Token);
            driver.Navigate().GoToUrl(pageUrl);

            WebClient webClient = new WebClient();
            string text = webClient.DownloadString(driver.Url);

            //check for invalid request 
            if (text.IndexOf("Invalid request or unable to validate request") >= 0)
            {
                Logger.WriteToLog(account.AccountName + ": FlexFileManager.GetFlexFile: Invalid request or unable to validate request");
                return false;
            }

            
            if (!WaitForTextToNotExist("Statement generation in progress. Please try again shortly", driver))
            {
                Logger.WriteToLog(account.AccountName + " : timedout - file not generated successfully");
                return false;
            }

            //save the data to file
            webClient.Encoding = System.Text.Encoding.UTF8;
            webClient.DownloadFile(pageUrl, ConfigurationManager.AppSettings["IBReportUploaderPath"]);

            Logger.WriteToLog("file saved");

            return true;
        }

        private static bool WaitForTextToNotExist(string massege, IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        WebClient webClient = new WebClient();
                        string text = webClient.DownloadString(driver.Url);

                        if (text.LastIndexOf(massege) < 0) {
                            Logger.WriteToLog("LogInManager: WaitForTextToNotExist: NoSuchElementException");
                            return true;
                        }

                        return false;
                    }
                    catch (Exception e)
                    {
                        Logger.WriteToLog("LogInManager: WaitForTextToNotExist: " + e.Message);
                        return false;
                    }
                });
            }
            catch (Exception e)
            {
                Logger.WriteToLog("LogInManager: WaitForElementToNotExist: WebDriverTimeoutException");
                return false;
            }

            return true;
        }

    }
}