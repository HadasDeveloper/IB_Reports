using System;
using System.Configuration;
using System.Threading;
using IB_Reports.Model;
using Logger;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace IB_Reports
{
    public static class LogInManager
    {
        public static bool GetFileWithLogin(IWebDriver driver, Account account)
        {
            FileLogWriter logger = new FileLogWriter();

            driver.Navigate().GoToUrl(ConfigurationManager.AppSettings["IBUrl"]);
            driver.FindElement(By.Name("user_name")).SendKeys(account.Login);
            driver.FindElement(By.Name("password")).SendKeys(account.Password);
            Thread.Sleep(100);
            driver.FindElement(By.Id("submitForm")).Click();

            //login into IB site
            if (!WaitForElementToNotExist("submitForm", driver))
            {
                logger.WriteToLog(DateTime.Now, account.AccountName + " : timedout - sbumitForm was not found for the first time", ConfigurationManager.AppSettings["logFileName"]);
                //try to login in the second time
                driver.FindElement(By.Id("submitForm")).Click();

                if (!WaitForElementToNotExist("submitForm", driver))
                {
                    logger.WriteToLog(DateTime.Now, account.AccountName + " : timedout - sbumitForm was not found for the secound time", ConfigurationManager.AppSettings["logFileName"]);    
                    return false;
                }
            }
            logger.WriteToLog(DateTime.Now, account.AccountName + " : logged in", ConfigurationManager.AppSettings["logFileName"]);

            //navigate to repory file downlod page
            driver.Navigate().GoToUrl(account.Link);


            //check for error messages
            if (GetErrorMessage(driver))
            {
                logger.WriteToLog(DateTime.Now, account.AccountName + " : error massage in generating the file", ConfigurationManager.AppSettings["logFileName"]);
                return false;
            }

            if (WaitForElementTextToShowUp("msg", "There was a problem while generating report for the account", driver,30))
            {
                logger.WriteToLog(DateTime.Now, account.AccountName + " : There was a problem while generating report for the account", ConfigurationManager.AppSettings["logFileName"]);
                account.Error = true;
                return false;
            }

            if (!WaitForElementTextToShowUp("msg", "generated successfully", driver,600)) 
            {
                logger.WriteToLog(DateTime.Now, account.AccountName + " : timedout - file not generated successfully", ConfigurationManager.AppSettings["logFileName"]);
                return false;
            }
            logger.WriteToLog(DateTime.Now, account.AccountName + "file generated successfully", ConfigurationManager.AppSettings["logFileName"]);
            return true;
        }

        private static bool WaitForElementToNotExist(string id, IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            FileLogWriter logger = new FileLogWriter();
            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        d.FindElement(By.Id(id));
                        return false;
                    }
                    catch (NoSuchElementException)
                    {
                        logger.WriteToLog(DateTime.Now, "LogInManager: WaitForElementToNotExist: NoSuchElementException", ConfigurationManager.AppSettings["logFileName"]);
                        return true;
                        
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                logger.WriteToLog(DateTime.Now, "LogInManager: WaitForElementToNotExist: WebDriverTimeoutException", ConfigurationManager.AppSettings["logFileName"]);
                return false;
            }
            return true;
        }

        private static bool WaitForElementTextToShowUp(string id, string text, IWebDriver driver, int secounds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(secounds));
            FileLogWriter logger = new FileLogWriter();
            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        IWebElement element = d.FindElement(By.Id(id));

                        if (element.Text.IndexOf(text) < 0)
                            return false;
                        
                        return true;

                    }
                    catch (Exception)
                    {
                        return false;   
                    }
                });
            }
            catch (Exception e)
            {
                logger.WriteToLog(DateTime.Now, string.Format("LogInManager: WaitForElementTextToShowUp: Waiting for {0} - {1}" , text , e.Message), ConfigurationManager.AppSettings["logFileName"]);
                return false;     
            }

            return true;
        }

        private static bool GetErrorMessage(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            FileLogWriter logger = new FileLogWriter();
            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        IWebElement element2 = d.FindElement(By.ClassName("attention"));

                        if ((element2.Text.IndexOf(ConfigurationManager.AppSettings["message1"]) < 0)
                            && element2.Text.IndexOf(ConfigurationManager.AppSettings["message2"]) < 0
                            && element2.Text.IndexOf(ConfigurationManager.AppSettings["message3"]) < 0
                            && element2.Text.IndexOf(ConfigurationManager.AppSettings["message4"]) < 0 
                            )
                        {
                            logger.WriteToLog(DateTime.Now, "LogInManager: GetErrorMessage: " + ConfigurationManager.AppSettings["message1"], ConfigurationManager.AppSettings["logFileName"]);
                            return false; 
                        }     
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });
            }
            catch (Exception e)
            {
                logger.WriteToLog(DateTime.Now, "LogInManager: GetErrorMessage: " + e.Message, ConfigurationManager.AppSettings["logFileName"]);
                return false;
            }
            return true;
        }

    }
}