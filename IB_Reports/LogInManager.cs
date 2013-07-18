using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using IB_Reports;

namespace IB_Reports
{
    public static class LogInManager
    {
        public static bool GetFileWithLogin(IWebDriver driver, Account account)
        {
            driver.Navigate().GoToUrl(ConfigurationManager.AppSettings["IBUrl"]);
            driver.FindElement(By.Name("user_name")).SendKeys(account.Login);
            driver.FindElement(By.Name("password")).SendKeys(account.Password);
            driver.FindElement(By.Id("submitForm")).Click();

            //login into IB site
            if (!WaitForElementToNotExist("submitForm", driver))
            {
                Logger.WriteToLog(account.AccountName + " : timedout - sbumitForm was not found for the first time");
                //try to login in the second time
                driver.FindElement(By.Id("submitForm")).Click();

                if (!WaitForElementToNotExist("submitForm", driver))
                {
                    Logger.WriteToLog(account.AccountName + " : timedout - sbumitForm was not found for the secound time");
                    return false;
                }
            }
            
            Logger.WriteToLog(account.AccountName + " : logged in");

            //navigate to repory file downlod page
            driver.Navigate().GoToUrl(account.Link);


            //check for error messages
            if (GetErrorMessage(driver))
            {
                Logger.WriteToLog(account.AccountName + " : error massage in generating the file");
                return false;
            }
           
            if (!WaitForElementTextToShowUp("msg", "generated successfully", driver)) 
            {
                Logger.WriteToLog(account.AccountName + " : timedout - file not generated successfully");
                return false;
            }

            Logger.WriteToLog(account.AccountName + "file generated successfully");
            return true;
        }

        private static bool WaitForElementToNotExist(string ID, IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        IWebElement element = d.FindElement(By.Id(ID));
                        return false;

                    }
                    catch (NoSuchElementException)
                    {
                        Logger.WriteToLog("LogInManager: WaitForElementToNotExist: NoSuchElementException");
                        return true;
                        
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                Logger.WriteToLog("LogInManager: WaitForElementToNotExist: WebDriverTimeoutException");
                return false;
            }
            return true;
        }

        private static bool WaitForElementTextToShowUp(string ID, string text, IWebDriver driver)
        { 
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(900));

            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        IWebElement element = d.FindElement(By.Id(ID));

                        if (element.Text.IndexOf(text) < 0)
                            return false;
                        else
                            return true;

                    }
                    catch (Exception e)
                    {
                        return false;   
                    }
                });
            }
            catch (Exception e)
            {
                Logger.WriteToLog("LogInManager: WaitForElementTextToShowUp: " + e.Message);
                return false;     
            }

            return true;
        }

        private static bool GetErrorMessage(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

            try
            {
                wait.Until((d) =>
                {
                    try
                    {
                        IWebElement element2 = d.FindElement(By.ClassName("attention"));

                        if ((element2.Text.IndexOf(ConfigurationManager.AppSettings["message1"]) < 0)
                            && element2.Text.IndexOf(ConfigurationManager.AppSettings["message2"]) < 0)
                            return false;
                        else
                            return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                });
            }
            catch (Exception e)
            {
                Logger.WriteToLog("LogInManager: GetErrorMessage: " + e.Message);
                return false;
            }
            return true;
        }

    }
}