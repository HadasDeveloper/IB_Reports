using System;
using System.Net;
using IB_Reports.Model;
using Logger;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Configuration;

namespace IB_Reports
{
    public static class FlexFileManager
    {  
        public static bool GetFlexFile(IWebDriver driver, Account account)
        {
            FileLogWriter logger = new FileLogWriter();

            //initial url, response will have the code
            driver.Navigate().GoToUrl(string.Format("{0}{1}&q={2}&v=2", ConfigurationManager.AppSettings["baseUrl"], account.Token, account.Queryid));

            //find reference code
            int startIndex = driver.PageSource.IndexOf("<code>");
            int endIndex = driver.PageSource.IndexOf("</code>");
            string referenceCode = driver.PageSource.Substring(startIndex + 6, endIndex - (startIndex + 6));

            string pageUrl = string.Format("{0}{1}&t={2}&v=2", ConfigurationManager.AppSettings["baseUr2"], referenceCode, account.Token);
            driver.Navigate().GoToUrl(pageUrl);

            WebClient webClient = new WebClient();
            string text = webClient.DownloadString(driver.Url);

            //check for invalid request 
            if (text.IndexOf("Invalid request or unable to validate request") >= 0)
            {
                logger.WriteToLog(DateTime.Now, account.AccountName + ": FlexFileManager.GetFlexFile: Invalid request or unable to validate request", "IB_Log");
                return false;
            }

            
            if (!WaitForTextToNotExist("Statement generation in progress. Please try again shortly", driver))
            {
                logger.WriteToLog(DateTime.Now, account.AccountName + " : timedout - file not generated successfully", "IB_Log");
                return false;
            }

            //save the data to file
            webClient.Encoding = System.Text.Encoding.UTF8;
            webClient.DownloadFile(pageUrl, string.Format(ConfigurationManager.AppSettings["IBReportUploaderPath"], account.AccountID));

            logger.WriteToLog(DateTime.Now, "file saved", "IB_Log");

            return true;
        }

        private static bool WaitForTextToNotExist(string massege, IWebDriver driver)
        {
            FileLogWriter logger = new FileLogWriter();

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
                            logger.WriteToLog(DateTime.Now, " LogInManager: WaitForTextToNotExist: NoSuchElementException", "IB_Log");
                            return true;
                        }

                        return false;
                    }
                    catch (Exception e)
                    {
                        logger.WriteToLog(DateTime.Now, " LogInManager: WaitForTextToNotExist: " + e.Message, "IB_Log");
                        return false;
                    }
                });
            }
            catch (Exception e)
            {
                logger.WriteToLog(DateTime.Now, " LogInManager: WaitForElementToNotExist: WebDriverTimeoutException " + e.Message, "IB_Log");
                return false;
            }

            return true;
        }

    }
}