/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.04.26
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
        public static void PollingCategoryUrls(String connString, String code)
        {
            Console.Clear();
            _WebDriverSetting = new WebDriverSettingInfo(hideCommand: true, hideBrowser: true);
            Boolean success = false;
            Int32 totalCategory = 0;
            Int32 countCategory = 0;
            Int32 totalSupplierAdded = 0;
            Int64 totalTicks = 0;
            Random random = new Random();

            List<SupplierCategoryInfo> list = DataOperation.GetCategoryList(connString, code);
            totalCategory = list.Count;
            display(String.Format("Polling Suppliers Start ... [{0}]", totalCategory), true, true);
            try
            {
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (SupplierCategoryInfo category in list)
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    display(String.Format("[{0}/{1}] - {2} ... ", ++countCategory, totalCategory, category.Name), false, true);
                    _CursorPosition = new Point(Console.CursorTop, Console.CursorLeft);
                    success = pollingSupplierUrls(connString, driver, wait, category);

                    stopWatch.Stop();
                    totalTicks += stopWatch.Elapsed.Ticks;
                    totalSupplierAdded += category.CountAdded;
                    Int64 countRemaining = (Int64)(totalSupplierAdded / countCategory) * (totalCategory - countCategory);
                    Int64 averageTicks = totalTicks / (totalSupplierAdded == 0 ? 1 : totalSupplierAdded);
                    TimeSpan timeSpan = new TimeSpan(averageTicks * countRemaining);
                    display(message: $"  [{timeSpan:d\\.hh\\:mm\\:ss}]", isWriteLine: true, addTime: false);
                }
                driver.Quit();
                display(String.Format("Completed [{0}]", success), true, true);
            }
            catch (System.Exception ex)
            {
                display(String.Format("[Webdriver] ERROR: {0}", ex.Message), true, true);
            }
            display(String.Format("*****Polling Suppliers End. [{0}]", totalCategory), true, true);
        }
        private static Boolean pollingSupplierUrls(String connString, ChromeDriver driver, WebDriverWait wait, SupplierCategoryInfo supplierCategory)
        {
            Boolean success = true;
            Random random = new Random();
            Int32 totalPage, totalPageLast, currentPage;
            Int32 count = 0;
            String status = Utilities.Utilities.GetCurrentMethodName(); // "pollingSupplierUrls";
            try
            {
                driver.Navigate().GoToUrl(supplierCategory.Url);
                if (driver.PageSource.Contains("dpl-board-alert-large"))
                    supplierCategory.Errors[status] = driver.GetElementValue(wait, "//div[@class='dpl-board-alert-large']/strong");
                else
                {
                    totalPage = getTotalPages(driver, wait);
                    totalPageLast = 0;
                    //String rootUrl = String.Empty;
                    if (totalPage > 0)
                    {
                        //rootUrl = driver.GetElementValue(wait, "//div[@class='ui2-pagination-pages']/a[@class='next']", "href");
                        //rootUrl = rootUrl.Substring(0, rootUrl.Length - 1);
                        do
                        {
                            currentPage = getCurrentPage(driver, wait, "//div[@class='ui2-pagination-pages']/span[@class='current']");
                            display(totalPage==totalPageLast? $"[{currentPage}]": $"[{currentPage}/{totalPage}]",isWriteLine: false, isPositionKept: false);
                            totalPageLast = totalPage;
                            //if (currentPage < 29)
                            //{
                            //    currentPage = 29;
                            //    driver.Navigate().GoToUrl("https://www.alibaba.com/catalogs/corporations/CID141905/29");
                            //}
                            List<String> urlList = getUrlList(driver, wait);
                            if (urlList.Count > 0)
                            {
                                count += urlList.Count;
                                updateSupplierUrls(connString, supplierCategory, urlList);
                                //display(String.Format(".", ""), false, false);
                            }
                            //String gotoUrl = String.Format("{0}{1}", rootUrl, currentPage + 1);
                            String gotoUrl = driver.GetElementValue(wait, "//div[@class='ui2-pagination-pages']/a[@class='next']", "href");
                            if (!String.IsNullOrEmpty(gotoUrl))
                            {
                                //if (currentPage < totalPage && gotoUrl.IsWebSiteAvailable())
                                //{
                                driver.Navigate().GoToUrl(gotoUrl);
                                if (driver.PageSource.Contains("ui2-pagination-pages"))
                                {
                                    totalPage = getTotalPages(driver, wait);
                                    if (totalPage < 0)
                                        totalPage = currentPage;
                                }
                                else
                                    break;
                                //}
                                //else break;
                            }
                            else break;
                        } while (true);
                    }
                }
                success = DataOperation.UpdateSupplierCategoryStatus(connString, supplierCategory);
                //System.Threading.Thread.Sleep(random.Next(20000, 30000));
                display(supplierCategory.HasError ? "[XX]" : $"[{supplierCategory.CountAdded}/{count}] Done", false, false);
            }
            catch (System.Exception ex)
            {
                display(String.Format("\n[pollingSuppliers] ERROR: {0}", ex.Message), true, true);
                success = false;
            }
            return success;
        }
        private static Int32 getCurrentPage(ChromeDriver driver, WebDriverWait wait, String xPath)
        {
            IWebElement element = wait.Until(d => d.FindElement(By.XPath(xPath)));
            Int32 currentPage = Convert.ToInt32(element.Text);
            return currentPage;
        }
        private static Int32 getTotalPages(ChromeDriver driver, WebDriverWait wait)
        {
            Int32 totalPage1 = getTotalPages(driver, wait, "//div[@class='ui2-pagination-pages']/span");
            Int32 totalPage2 = getTotalPages(driver, wait, "//div[@class='ui2-pagination-pages']/a", 2);
            return Math.Max(totalPage1, totalPage2);
        }
        private static Int32 getTotalPages(ChromeDriver driver, WebDriverWait wait, String xPath, Int32 previous = 1)
        {
            Int32 pages = -1;
            try
            {
                var elementPages = wait.Until(d => d.FindElements(By.XPath(xPath)));
                pages = Convert.ToInt32(elementPages[elementPages.Count - previous].Text);
            }
            catch (System.Exception) { }
            return pages;
        }
        public static void PollingSuppliersFromUrlMetadata(String connString, Boolean byWebDriver)
        {
            ChromeDriver driver = null;
            Boolean success = false;
            Int32 totalCategory = 0;
            Int32 countCategory = 0;
            Int64 totalTicks = 0;
            try
            {
                if (byWebDriver)
                    driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: true, hideCommand: true);
                List<SupplierUrlInfo> list = DataOperation.GetSupplierUrls(connString);
                totalCategory = list.Count;
                display(String.Format("Polling Suppliers from URL Start ... [{0}]", totalCategory), true, true);
                foreach (SupplierUrlInfo supplierUrl in list)
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    //display(String.Format("[{0}/{1}] - [{2}] {3} [{4}] Processing ...", ++countCategory, totalCategory, supplierUrl.Id, supplierUrl.CategoryName, supplierUrl.Tries), false, true);
                    display($"[{++countCategory}/{totalCategory}] - [{supplierUrl.Id}] [{supplierUrl.Tries}] Processing ...", false, true);
                    //supplierUrl.ProfileUrl = "http://chukuang.en.alibaba.com/contactinfo.html";
                    //supplierUrl.Id = 69570;
                    SupplierInfo supplier = new SupplierInfo(supplierUrl, driver);
                    if (!supplier.HasError)
                        success = DataOperation.UpdateSupplier(connString, supplier);
                    success = DataOperation.UpdateSupplierUrlStatus(connString, supplierUrl, supplier);
                    stopWatch.Stop();
                    totalTicks += stopWatch.Elapsed.Ticks;
                    TimeSpan timeSpan = new TimeSpan((long)(totalTicks / countCategory) * (totalCategory - countCategory));
                    display(String.Format(" [{0}] [{1:d\\.hh\\:mm\\:ss}]", supplier.HasError ? "XX" : supplier.CompanyId, timeSpan), true, false);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
            }
            Console.WriteLine("Polling Suppliers End. [{0}]", totalCategory);
            if (driver != null)
                driver.Quit();
        }
        public static void PollingSuppliersMetadataFromUrl(String connString)
        {
            Int32 totalCategory = 0;
            Int32 countCategory = 0;

            List<SupplierUrlInfo> list = DataOperation.GetSupplierUrls(connString);
            totalCategory = list.Count;
            display(String.Format("Polling Suppliers from URL Start ... [{0}]", totalCategory), true, true);
            try
            {
                foreach (SupplierUrlInfo supplierUrl in list)
                {
                    display(String.Format("[{0}/{1}] - [{2}] {3} [{4}] Processing ...", ++countCategory, totalCategory, supplierUrl.Id, supplierUrl.CategoryName, supplierUrl.Tries), false, true);
                    supplierUrl.Update();
                    DataOperation.UpdateSupplierURL(connString, supplierUrl);
                    display(String.Format(" [{0}]", supplierUrl.HasError ? "XXXX" : "Done"), true, false);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
            Console.WriteLine("Polling Suppliers End. [{0}]", totalCategory);
        }
        public static void PollingSuppliersFromUrl(String connString)
        {
            WebDriverSettingInfo webDriverSetting = new WebDriverSettingInfo(true, true);
            Boolean success = false;
            Int32 totalCategory = 0;
            Int32 countCategory = 0;
            Random random = new Random();

            List<SupplierUrlInfo> list = DataOperation.GetSupplierUrls(connString);
            totalCategory = list.Count;
            display(String.Format("Polling Suppliers from URL Start ... [{0}]", totalCategory), true, true);
            try
            {
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: webDriverSetting.HideBrowser, hideCommand: webDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (SupplierUrlInfo supplierUrl in list)
                {
                    display(String.Format("[{0}/{1}] - [{2}] {3} [{4}] Processing ...", ++countCategory, totalCategory, supplierUrl.Id, supplierUrl.CategoryName, supplierUrl.Tries), false, true);
                    try
                    {
                        SupplierInfo supplier = new SupplierInfo(driver, wait, supplierUrl.ProfileUrl, new SupplierCategoryInfo(supplierUrl.CategoryId, supplierUrl.CategoryName, supplierUrl.CategoryLevel));
                        if (!supplier.HasError)
                            success = DataOperation.UpdateSupplier(connString, supplier);
                        success = DataOperation.UpdateSupplierUrlStatus(connString, supplierUrl, supplier);
                        display(String.Format(" [{0}]", supplier.HasError ? "XX" : supplier.CompanyId), true, false);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("[Post] ERROR: {0}", ex.Message);
                    }
                }
                driver.Quit();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
            }
            Console.WriteLine("Polling Suppliers End. [{0}]", totalCategory);
        }
    }
}
