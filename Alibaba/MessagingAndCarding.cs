/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.07.09
 *   Description: 
 *       Company: Qway Inc.
 *  Project Name: 
 *     Reference: 
 * Code Reviewer: 
 *   Review Date: 
 *        Remark: 
 * ****************************************************************************
 * Revision History:                                                          *
 * ========================================================================== *
 *          Name: 
 * Modified Date: 
 *   Description: 
 * --------------------------------------------------------------------------
 * ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using HtmlAgilityPack;
using System.Drawing;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using System.Reflection;
using System.IO;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

using Utilities;
using EAGetMail;

namespace Alibaba
{
    public static partial class BusinessLogic
    {
        public static void Test()
        {
            IWebElement element = null;
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: false, hideCommand: false, isMaximized: true);
            //String url = "https://message.alibaba.com/msgsend/contact.htm?action=contact_action&domain=2&id=234204272&id_f=IDX181aKJtvuRZLKh8naf5MGS3QN69sZNW3y2Mc4mdq06T4qvQHoHjGz1R3DJKHxQCOn";
            String url = @"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\HTMLPage1.html";
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(5);
            driver.Navigate().GoToUrl(url);
            element = driver.FindElementById("inquiry-content");
            element = driver.FindElement(By.XPath("//textarea[@id='inquiry-content']"));
            element.SendKeys("AAAAAAA");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value=arguments[1];", element, "BBBBB");
            //if (driver.GetElement("//body[@id='tinymce']", ref element, "tinymce")
            //    || driver.GetElement("//textarea[@id='inquiry-content']", ref element, "inquiry-content"))
            driver.Quit();


        }
        private static Boolean _Stop = false;
        public static void SendingAndPolling(String connString, string[] args)
        {
            WebDriverSettingInfo webDriverSetting = new WebDriverSettingInfo(hideCommand: false, hideBrowser: false);
            Int32 totalAccount = 1;
            Boolean isAll = true;
            if (args.Length > 0)
            {
                totalAccount = Convert.ToInt32(args[0]);
                if (args.Length == 2)
                    isAll = args[1].ToBoolean();
            }
            String status = Utilities.Utilities.GetCurrentMethodName();
            Boolean success = false;
            Random random = new Random();
            display(message: $"[{status}] Sending And Polling Start ... [{totalAccount}]", isWriteLine: true, addTime: true, feedLine: true);
            Int32 count = 1;
            while (count <= totalAccount)
            {
                try
                {
                    ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: webDriverSetting.HideBrowser, hideCommand: webDriverSetting.HideCommand, isMaximized: true);
                    if (driver != null)
                    {
                        SettingsInfo setting = DataOperation.GetSetting(connString);
                        //setting.Email = "qway.ca.j@hotmail.com";
                        //setting.Password = "Ah630615";
                        display(String.Format("[{0}/{1}] [{2}] ....", count, totalAccount, setting.Email), false, true);
                        if (alibabaLogin(driver, connString, setting, random.Next(4000, 6000)))
                        {
                            ++count;
                            success = DataOperation.UpdateSettingPostStatus(connString, setting);
                            if (sendingMessages(driver, random, connString, setting))
                            {
                                if (isAll)
                                {
                                    success = pollingBusinessCardSent(driver, random, connString, setting);
                                    success = pollingBusinessCardReceived(driver, random, connString, setting);
                                }
                            }else
                                display($"[{status}] Failed with email verification.", true, true);
                        }
                        else
                            display($"[{status}] Login Failed.", true, true);
                    }
                    driver.Quit();
                    if (_Stop)
                        break;
                }
                catch (System.Exception ex)
                {
                    display($"[{status}] ERROR: {ex.Message}", true, true);
                }
            }
        }
        private static Boolean sendingMessages(ChromeDriver driver, Random random, String connString, SettingsInfo setting)
        {
            String status = Utilities.Utilities.GetCurrentMethodName();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Int32 countSuccess = 0;
            Boolean success = false;
            display(String.Format("Sending [{0}]", setting.OpenPages), false, false);
            WebDriverWait wait = new WebDriverWait(driver, _TimeSpanWaiting);
            List<SupplierInfo> suppliers = DataOperation.GetSuppliersToPost(connString, setting.OpenPages);
            display(String.Format("-> ", ""), false, false);
            foreach (SupplierInfo supplier in suppliers)
            {
                //supplier.ContactProfileUrl = "https://easco.en.alibaba.com/contactinfo.html";
                supplier.Account = setting.Account;
                String url = getSendingUrl(driver, wait, supplier);
                if (!String.IsNullOrEmpty(url))
                {
                    try
                    {
                        success = driver.GoToUrl(url);
                        if (driver.PageSource.Contains("board errorA"))
                        {
                            supplier.Errors[status] = Utilities.WebDriverExtension.GetElementValue(driver, wait, "//div[@class='board errorA']/h3");
                            supplier.FatalError = true;
                        }
                        else
                        {
                            IWebElement element = null;
                            if (driver.GetElement("//input[@name='email']", ref element))
                            {
                                supplier.Errors[status] = "Need email as verification";
                                supplier.FatalError = true;
                            }
                            else
                            {
                                String newMessage = getMessageToSend(supplier, driver, wait);

                                Utilities.WebDriverExtension.CheckElement(driver, wait, @"//input[@id='respond-in-oneday']", false);
                                Utilities.WebDriverExtension.CheckElement(driver, wait, @"//input[@id='agree-share-bc']", true);
                                System.Threading.Thread.Sleep(random.Next(1000, 2000));

                                //driver.Navigate().Refresh();
                                IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");
                                driver.SwitchTo().Frame(0);

                                if (!driver.GetElement("//body[@id='tinymce']", ref element, "tinymce"))
                                {
                                    driver.Navigate().Refresh();
                                    //if (!driver.GetElement("//textarea[@id='inquiry-content']", ref element))
                                    if (!driver.GetElement("//form[@class='inquiry-body']/div/div/div/textarea", ref element))
                                    {
                                        supplier.Errors[status] = "New Format!!";
                                        //try
                                        //{
                                        //    element = driver.FindElementById("inquiry-content");
                                        //}
                                        //catch (System.Exception)
                                        //{
                                        //    element = driver.FindElement(By.XPath("//textarea[@id='inquiry-content']"));
                                        //}
                                    }
                                    else
                                        sendMessage(newMessage, supplier, driver, wait, element, random);
                                }
                                else
                                    sendMessage(newMessage, supplier, driver, wait, element, random);
                            }
                        }
                        if (!supplier.HasError)
                        {
                            ++countSuccess;
                            success = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        supplier.Errors[status] = ex.Message;
                    }
                }
                DataOperation.UpdateSupplierPostStatus(connString, supplier);
                Utilities.Utilities.Log(message: supplier.HasError ? "X" : ".", isWriteLine: false, addTime: true,isError: supplier.HasError);
                if (supplier.FatalError)
                {
                    success = false;
                    break;
                }
            }
            stopWatch.Stop();
            Int64 averageTicks = countSuccess == 0 ? stopWatch.Elapsed.Ticks : stopWatch.Elapsed.Ticks / countSuccess;
            display($@"Done [{countSuccess}] [{averageTicks:c}]/per.");
            return success;
        }
        private static Boolean sendMessage(String message, SupplierInfo supplier, ChromeDriver driver, WebDriverWait wait, IWebElement element, Random random)
        {
            Boolean success = false;
            String status = Utilities.Utilities.GetCurrentMethodName();
            if (element.SendKeys(driver, message))
            {
                //System.Threading.Thread.Sleep(random.Next(1000, 3000));
                driver.SwitchTo().DefaultContent();
                element = wait.Until(d => d.FindElement(By.XPath("//div[@class='send-item']/input")));
                String urlLast = driver.Url;
                if (!clickToSend(driver, element))
                {
                    driver.SwitchTo().DefaultContent();
                    //System.Threading.Thread.Sleep(random.Next(3000, 9000));
                    if (driver.Url.CompareTo(urlLast) == 0)
                    {
                        if (!clickToSend(driver, element))
                        {
                            if (driver.PageSource.Contains("Send messages too frequently"))
                            {
                                _Stop = true;
                                supplier.Errors[status] = "Stop!!";
                            }
                            else
                            {
                                element.Submit();
                                driver.Navigate().Refresh();
                                if (driver.Url.CompareTo(urlLast) == 0)
                                    supplier.Errors[status] = "Not Send.";
                            }
                        }
                    }
                    success = driver.Url.CompareTo(urlLast) != 0;
                }
            }
            else
                supplier.Errors[status] = "Can't send message!!";
            return success;
        }
        private static Boolean clickToSend(ChromeDriver driver, IWebElement element)
        {
            Boolean success = false;
            if (element.Click(driver))
                if (driver.GetElement("//div[@class='ui2-feedback-title']", ref element, "Inquiry sent successfully", new WebDriverWait(driver, TimeSpan.FromMinutes(2))))
                    success = true;
            return success;

        }
        private static String getMessageToSend(SupplierInfo supplier, ChromeDriver driver, WebDriverWait wait)
        {
            String message = String.Empty;
            String contact = supplier.ContactName;
            if (String.IsNullOrEmpty(contact))
            {
                contact = Utilities.WebDriverExtension.GetElementValue(driver, wait, "//span[@class='company-contact']").Split(' ')[0];
                if (String.IsNullOrEmpty(contact))
                    contact = "Sir/Madam";
            }
            contact = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contact.ToLower());

            String originalMessage = System.IO.File.ReadAllText(supplier.ScriptFileName);
            message = originalMessage.Replace("{Name}", contact).Replace("{Company}", supplier.CompanyName);
            return message;
        }
        public static String GetSendingUrl()
        {
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: false, hideCommand: false);
            WebDriverWait wait = new WebDriverWait(driver, _TimeSpanWaiting);
            SupplierInfo supplier = new SupplierInfo()
            {
                Id = 69654,
                ContactProfileUrl = "http://hzlongte.en.alibaba.com/contactinfo.html"
            };

            return getSendingUrl(driver, wait, supplier);
        }
        private static Boolean gotoUrl(ChromeDriver driver, SupplierInfo supplier, String status)
        {
            Boolean success = false;
            supplier.CompanyUrl = supplier.ContactProfileUrl.GetBaseUrl();
            if (!supplier.CompanyUrl.ToLower().EndsWith("www.alibaba.com"))
                try
                {
                    driver.GoToUrl(supplier.CompanyUrl);
                    success = true;
                }
                catch (System.Exception) { }
            if (!success)
            {
                supplier.CompanyUrl = supplier.ContactProfileUrl;
                try
                {
                    driver.GoToUrl(supplier.CompanyUrl);
                    success = true;
                }
                catch (System.Exception ex)
                {
                    supplier.Errors[status] = ex.Message;
                }
            }
            return success;
        }
        private static String getSendingUrl(ChromeDriver driver, WebDriverWait wait, SupplierInfo supplier)
        {
            String url = String.Empty;
            String status = Utilities.Utilities.GetCurrentMethodName();
            if (gotoUrl(driver, supplier, status))
            {
                try
                {
                    //IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    //js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    //js.ExecuteScript("window.scrollTo(0, -document.body.scrollHeight)");
                    Dictionary<String, String> xPaths = getUrlXPaths();
                    if (String.IsNullOrEmpty(url = getSendingUrl(driver, wait, xPaths)))
                    {
                        //wait.Until(d => d.Navigate()).Refresh();
                        if (String.IsNullOrEmpty(url = getSendingUrl(driver, wait, xPaths)))
                        {
                            if (String.IsNullOrEmpty(supplier.Errors[status] = driver.GetValue(xPath: "//div[@class='banner-copy']", wait: wait, key: "banner-copy")))
                                supplier.Errors[status] = $"Sending Url is empty. [New content?]";
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    supplier.Errors[status] = ex.Message;
                }
            }
            else
                supplier.Errors[status] = $"ContactProfileUrl not avaiable. \n{StatusExtension.ErrorMessage}";
            return url;
        }
        private static Dictionary<String, String> getUrlXPaths()
        {
            Dictionary<String, String> xPaths = new Dictionary<String, String>();
            Dictionary<String, String> dic = new Dictionary<String, String>()  {
                {"sidebar-main", "//div[@class='{key}']/a"},
                {"message-send ui-button ui-button-primary", "//a[@class='{key}']"},
                {"next-row next-row-justify-center next-row-align-center contact-actions", "//div[@class='{key}']/a"}
            };
            foreach (String key in dic.Keys)
            {
                xPaths[key] = dic[key].Replace("{key}", key);
            }
            return xPaths;
        }
        private static String getSendingUrl(ChromeDriver driver, WebDriverWait wait, Dictionary<String, String> xPaths)
        {
            String url = String.Empty;
            foreach (String key in xPaths.Keys)
            {
                url = driver.GetValue(xPath: xPaths[key], wait: wait, key: key, attribute: "href");
                if (!String.IsNullOrEmpty(url))
                    break;
            }
            return url;
        }
        private static String getSendingUrl2(ChromeDriver driver, WebDriverWait wait, SupplierInfo supplier)
        {
            String url = String.Empty;
            String status = "getSendingUrl2";
            if (supplier.ContactProfileUrl.IsWebSiteAvailable())
            {
                try
                {

                    driver.Navigate().GoToUrl(supplier.ContactProfileUrl);
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    js.ExecuteScript("window.scrollTo(0, -document.body.scrollHeight)");
                    //js.ExecuteScript("scroll(0, 1000);");
                    //IWebElement element = driver.FindElement(By.XPath("//h3[@class='title']"));
                    //element.Click();
                    //element.SendKeys(Keys.PageDown);
                    System.Threading.Thread.Sleep(3 * 1000);
                    if (driver.PageSource.Contains("icbu-mod-wrapper with-border module-contact-person"))
                        url = Utilities.WebDriverExtension.GetElementValue(driver, wait, "//div[@class='icbu-mod-wrapper with-border module-contact-person']//a[@class='message']", "href");
                    else
                    {
                        System.Threading.Thread.Sleep(3 * 1000);
                        if (driver.PageSource.Contains("message-send ui-button ui-button-primary"))
                            url = Utilities.WebDriverExtension.GetElementValue(driver, wait, "//a[@class='message-send ui-button ui-button-primary']", "href");
                        else
                            supplier.Errors[status] = $"Sending Url is empty. [New content?]";
                        if (!String.IsNullOrEmpty(url) && !url.IsWebSiteAvailable())
                        {
                            supplier.Errors[status] = $"Sending Url not avaiable. \n{StatusExtension.ErrorMessage}";
                            url = String.Empty;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    supplier.Errors[status] = ex.Message;
                }
            }
            else
                supplier.Errors[status] = $"ContactProfileUrl not avaiable. \n{StatusExtension.ErrorMessage}";
            return url;
        }
        private static Boolean alibabaLogin(ChromeDriver driver, String connString, SettingsInfo setting, Int32 sleep)
        {
            //display(String.Format("Login [{0}] ... ", setting.Email), true, true);
            Boolean success = driver.AlibabaLogin(setting.Email, setting.Password, setting.UrlAlibaba, setting.UrlMessage, sleep);
            if (!success)
            {
                //display(String.Format(" [Logged in]", ""), false, false);
                DataOperation.UpdateSettingStatus(connString, setting, StatusExtension.ErrorMessage);
                //display(String.Format(" [Updated]", ""), true, false);
            }
            return success;
        }
        private static Boolean pollingBusinessCardSent(ChromeDriver driver, Random random, String connString, SettingsInfo setting)
        {
            String status = Utilities.Utilities.GetCurrentMethodName();
            Boolean success = false;
            Boolean terminated = false;
            Int32 totalPage = 0;
            Int32 currentPage = 0;
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, _TimeSpanWaiting);
                driver.GoToUrl(setting.UrlSent);
                totalPage = getTotalPages(driver);
                display(String.Format("[{0}] Sent Cards .", totalPage), false, true);
                do
                {
                    display(".", false, false);
                    currentPage = getCurrentPage(driver, wait);
                    success = clickRequestCard(driver, wait, random);
                    List<BusinessCardInfo> cards = getBusinessCardsSent(driver, setting.Account, setting.Email);
                    success = DataOperation.UpdateBusinessCards(connString, cards);
                    if (!setting.CheckAllPages && cards.Where(card => card.Result.Status == "NOT_SEND").FirstOrDefault<BusinessCardInfo>() == null)
                    {
                        terminated = true;
                        break;
                    }

                    if (currentPage < totalPage)
                    {
                        success = driver.ClickElement(wait, "//a[@class='next']");
                        System.Threading.Thread.Sleep(random.Next(2000, 5000));
                    }
                } while (currentPage < totalPage);
                //System.Threading.Thread.Sleep(random.Next(20000, 30000));
            }
            catch (System.Exception ex)
            {
                display($"[{status}] ERROR: {ex.Message}", true, true);
            }
            display(String.Format("Done [{0}].", terminated ? "Terminated" : "Completed"), true, false);
            return success;
        }
        private static Boolean pollingBusinessCardReceived(ChromeDriver driver, Random random, String connString, SettingsInfo setting)
        {
            String status = Utilities.Utilities.GetCurrentMethodName();
            Boolean success = false;
            Int32 totalPage = 0;
            Int32 currentPage = 0;
            Boolean terminated = false;

            try
            {
                WebDriverWait wait = new WebDriverWait(driver, _TimeSpanWaiting);
                driver.GoToUrl(setting.UrlReceivied);
                totalPage = getTotalPages(driver);
                display(String.Format("[{0}] Received Cards .", totalPage), false, true);
                do
                {
                    display(".", false, false);
                    currentPage = getCurrentPage(driver, wait);
                    if (currentPage > 0)
                    {
                        List<BusinessCardInfo> cards = getBusinessCardsReceived(driver, setting.Account, setting.Email);
                        success = DataOperation.UpdateBusinessCards(connString, cards);
                        if (!setting.CheckAllPages && cards.Where(card => card.Result.Code == "INSERT").FirstOrDefault<BusinessCardInfo>() == null)
                        {
                            terminated = true;
                            break;
                        }
                        if (currentPage < totalPage)
                        {
                            success = driver.ClickElement(wait, "//div[@class='ui2-pagination-pages']/a[@class='next']");
                            System.Threading.Thread.Sleep(random.Next(2000, 3000));
                        }
                    }
                    else
                        break;
                } while (currentPage < totalPage);
                //System.Threading.Thread.Sleep(random.Next(20000, 30000));
            }
            catch (System.Exception ex)
            {
                display($"[{status}] ERROR: {ex.Message}", true, true);
            }
            display(String.Format("Done [{0}].", terminated ? "Terminated" : "Completed"), true, false);
            return success;
        }
        private static Int32 getCurrentPage(ChromeDriver driver, WebDriverWait wait)
        {
            Int32 currentPage = 0;
            try
            {
                IWebElement element = wait.Until(d => d.FindElement(By.XPath("//div[@class='ui2-pagination-pages']/span[@class='current']")));
                currentPage = Convert.ToInt32(element.Text);
            }
            catch (System.Exception) { }
            return currentPage;
        }
        private static Int32 getTotalPages(ChromeDriver driver)
        {
            Int32 pages = 1;
            try
            {
                var elementPages = driver.FindElementsByXPath("//div[@class='ui2-pagination-pages']/a[@rel='nofollow']");
                pages = Convert.ToInt32(elementPages[elementPages.Count - 2].Text);
            }
            catch (System.Exception) { }
            return pages;
        }
        private static bool clickRequestCard(ChromeDriver driver, WebDriverWait wait, Random random)
        {
            Boolean success = false;
            try
            {
                if (driver.PageSource.Contains("ui2-button ui2-button-default ui2-button-normal ui2-button-medium util-clearfix J-req-card"))
                {
                    IReadOnlyCollection<IWebElement> elements = driver.FindElementsByXPath("//a[@class='ui2-button ui2-button-default ui2-button-normal ui2-button-medium util-clearfix J-req-card']");
                    if (elements != null && elements.Count > 0)
                    {
                        foreach (IWebElement element in elements)
                        {
                            OpenQA.Selenium.Interactions.Actions actions = new OpenQA.Selenium.Interactions.Actions(driver);
                            actions.MoveToElement(element).Click().Build().Perform();
                            System.Threading.Thread.Sleep(random.Next(1000, 2000));
                        }
                        //Console.Write("[{0}] Clicked. ", elements.Count);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("No Element to click: {0}", ex.Message);
            }
            return success;
        }
    }
}
