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
//using System.Drawing;
//using System.Net;
//using System.Web.Script.Serialization;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System.Data;
//using System.Data.SqlClient;
//using System.Runtime.InteropServices;
//using Microsoft.Office.Interop.Outlook;
//using System.Reflection;
//using System.IO;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

using Utilities;
//using EAGetMail;

namespace Amazon
{
    public static partial class BusinessLogic
    {
        private static WebDriverSettingInfo _WebDriverSetting = new WebDriverSettingInfo(true, true);
        internal static void PollingSellerDetail(string connString)
        {
            Boolean success = true;
            String code = "C";
            try
            {
                _WebDriverSetting = new WebDriverSettingInfo(true, true);
                Utilities.Utilities.Log(message: "Polling Seller dey=tails Start ... ", isWriteLine: false, addTime: true);
                List<SellerInfo> sellers = DataOperation.GetSeller(connString);
                Utilities.Utilities.Log(message: $"[{sellers.Count}]", isWriteLine: true);
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (SellerInfo seller in sellers)
                {
                    success = false;
                    Utilities.Utilities.Log(message: $"[{seller.Name}] : ", isWriteLine: false, addTime: true);
                    driver.Navigate().GoToUrl($"https://www.amazon.com/sp?seller={seller.SellerId}");
                    if (driver.PageSource.Contains("feedback-summary-table"))
                    {
                        IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("//table[@id='feedback-summary-table']/tbody/tr[5]/td[@class='a-text-right']"));
                        if (elements != null && elements.Count > 0)
                        {
                            seller.Update(elements.ToList<IWebElement>());
                            success = true;
                        }
                    }
                    else
                        seller.Update(null);
                    seller.Id = DataOperation.UpdateSeller(connString, seller);
                    success &= seller.Id > 0;
                    Utilities.Utilities.Log(message: success ? $"[Done]" : $"[X]", isWriteLine: true, addTime: false);
                }
                driver.Quit();
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: $"[Error]{ex.Message}", feedLine: true, isWriteLine: true, addTime: true);
            }
        }
        internal static void PollingSeller(string connString)
        {
            Boolean success = true;
            String code = "C";
            try
            {
                _WebDriverSetting = new WebDriverSettingInfo(true, false);
                Utilities.Utilities.Log(message: "Polling Seller Start ... ", isWriteLine: false, addTime: true);
                List<CategoryInfo> categories = DataOperation.GetCategory(connString);
                Utilities.Utilities.Log(message: $"[{categories.Count}]", isWriteLine: true);
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (CategoryInfo category in categories.Where(cat => !String.IsNullOrEmpty(cat.ToSellersUrl)))
                {
                    success = false;
                    pollingSeller(connString, $"{category.Name}", driver, category, category.ToSellersUrl, true);
                    Dictionary<String, String> urlList = getUrlList(driver, category.ToSellersUrl);
                    foreach (String key in urlList.Keys)
                        pollingSeller(connString, $"{category.Name}.{key}", driver, category, urlList[key], false);
                    category.Completed = true;
                    category.Id = DataOperation.UpdateCategory(connString, category);
                }
                driver.Quit();
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: $"[Error]{ex.Message}", feedLine: true, isWriteLine: true, addTime: true);
            }
        }
        private static Dictionary<String, String> getUrlList(ChromeDriver driver, String url)
        {
            driver.Navigate().GoToUrl(url);
            IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("//div[@id='indexBarHeader'][1]/div/div/span[@class='pagnLink']/a"));
            Dictionary<String, String> dic = new Dictionary<string, string>();
            //elements.Select(ele => new KeyValuePair<String, String>(ele.GetValue("", ""), ele.GetValue("", "href"))).ToDictionary<String, String>();
            foreach (IWebElement element in elements)
            {
                dic[element.GetValue("", "")] = element.GetValue("", "href");
            }
            return dic;
        }
        private static Boolean pollingSeller(string connString, string name, ChromeDriver driver, CategoryInfo category, String url, Boolean isTopSeller)
        {
            Boolean success = false;
            Utilities.Utilities.Log(message: $"[{name}] : ", isWriteLine: false, addTime: true);
            driver.Navigate().GoToUrl(url);
            IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("//div[@class='a-row a-spacing-none s-see-all-c3-refinement s-see-all-refinement-list']/ul/li/span/a"));
            if (elements != null && elements.Count > 0)
            {
                Utilities.Utilities.Log(message: $"[{elements.Count}]", isWriteLine: true, addTime: false);
                Int32 count = 0;
                Utilities.Utilities.Log(message: $"[{category.Name}] [{elements.Count}] -> ", isWriteLine: false, addTime: true);
                foreach (IWebElement ele in elements)
                {
                    SellerInfo seller = new SellerInfo(category, ele, isTopSeller);
                    if (success = !seller.HasError)
                    {
                        seller.Id = DataOperation.UpdateSeller(connString, seller);
                        if (success = seller.Id > 0)
                            ++count;
                    }
                    Utilities.Utilities.Log(message: success ? $"." : $"X", isWriteLine: false, addTime: false);
                }
                Utilities.Utilities.Log(message: $"[{count}] [Done]", isWriteLine: true, addTime: false);
            }
            return success;
        }
        internal static void PollingCategorySeller(string connString)
        {
            Boolean success = true;
            String code = String.Empty;
            try
            {
                _WebDriverSetting = new WebDriverSettingInfo(true, false);
                Utilities.Utilities.Log(message: "Polling Seller Url for Category Start ... ", isWriteLine: false, addTime: true);
                List<CategoryInfo> categories = DataOperation.GetCategory(connString);
                Utilities.Utilities.Log(message: $"[{categories.Count}]", isWriteLine: true);
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                Int32 count = 0;
                foreach (CategoryInfo parent in categories)
                {
                    success = false;
                    Utilities.Utilities.Log(message: $"[{parent.Name}] : ", isWriteLine: false, addTime: true);
                    driver.Navigate().GoToUrl(parent.Url);
                    IWebElement element = driver.GetElement("//h4[@class='a-size-small a-spacing-mini a-color-base a-text-bold' and text()='Seller']/following-sibling::ul/li/span/a");
                    if (element != null)
                    {
                        parent.ToSellersUrl = element.GetValue("", "href");
                        ++count;
                        success = true;
                    }
                    parent.Completed = true;
                    parent.Id = DataOperation.UpdateCategory(connString, parent);
                    Utilities.Utilities.Log(message: success ? $"[Yes]" : $"[No]", isWriteLine: true, addTime: false);
                }
                Utilities.Utilities.Log(message: $"[{count}] supplier Urls added", isWriteLine: true, addTime: true);
                driver.Quit();
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: $"[Error]{ex.Message}", feedLine: true, isWriteLine: true, addTime: true);
            }
        }
        internal static void PollingCategory(string connString)
        {
            Boolean success = true;
            String code = String.Empty;
            try
            {
                _WebDriverSetting = new WebDriverSettingInfo(true, false);
                Utilities.Utilities.Log(message: "Polling Sellers for Category Start ... ", isWriteLine: false, addTime: true);
                List<CategoryInfo> categories = DataOperation.GetCategory(connString);
                Utilities.Utilities.Log(message: $"[{categories.Count}]", isWriteLine: true);
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (CategoryInfo parent in categories)
                {
                    Int32 count = 0;
                    driver.Navigate().GoToUrl(parent.Url);
                    if (!pollingCategorySellerB(connString, parent, driver, ref count)
                        && !pollingCategorySellerA(connString, parent, driver, ref count)
                        )
                    {

                    }
                    Utilities.Utilities.Log(message: $" [{count}] [Done]", isWriteLine: true, addTime: false);
                    IWebElement element = driver.GetElement("//h4[@class='a-size-small a-spacing-mini a-color-base a-text-bold' and text()='Seller']/following-sibling::ul/li/span/a");
                    if (element != null)
                        parent.ToSellersUrl = element.GetValue("", "href");
                    parent.Completed = true;
                    parent.Id = DataOperation.UpdateCategory(connString, parent);
                }
                driver.Quit();
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: $"[Error]{ex.Message}", feedLine: true, isWriteLine: true, addTime: true);
            }
        }
        private static Boolean pollingCategorySellerA(string connString, CategoryInfo parent, ChromeDriver driver, ref Int32 count)
        {
            Boolean success = false;
            String code = "A";
            try
            {
                IWebElement element = driver.GetElement("//div[@id='nav-subnav']");
                if (element != null)
                {
                    IReadOnlyCollection<IWebElement> elements = element.FindElements(By.XPath(".//a[@class='nav-a']"));
                    if (elements != null)
                    {
                        Utilities.Utilities.Log(message: $"[{parent.Name}] [{elements.Count}] -> ", isWriteLine: false, addTime: true);
                        foreach (IWebElement ele in elements)
                        {
                            CategoryInfo category = new CategoryInfo(code, parent, ele);
                            if (success = category.HasError)
                            {
                                category.Id = DataOperation.UpdateCategory(connString, category);
                                if (success = category.Id > 0)
                                    ++count;
                            }
                            Utilities.Utilities.Log(message: success ? $"." : $"X", isWriteLine: false, addTime: false);
                        }
                        success = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: $"[Error]{ex.Message}", feedLine: true, isWriteLine: true, addTime: true);
            }
            return success;
        }
        private static Boolean pollingCategorySellerB(string connString, CategoryInfo parent, ChromeDriver driver, ref Int32 count)
        {
            Boolean success = false;
            String code = "B";
            try
            {
                if (driver.PageSource.Contains("a-unordered-list a-nostyle a-vertical s-ref-indent-two"))
                {
                    IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("//ul[@class='a-unordered-list a-nostyle a-vertical s-ref-indent-two']/div[@class='a-row a-expander-container a-expander-extend-container']/li/span/a"));
                    if (elements != null && elements.Count > 0)
                    {
                        Utilities.Utilities.Log(message: $"[{parent.Name}] [{elements.Count}] -> ", isWriteLine: false, addTime: true);
                        foreach (IWebElement ele in elements)
                        {
                            CategoryInfo category = new CategoryInfo(code, parent, ele);
                            if (success = category.HasError)
                            {
                                category.Id = DataOperation.UpdateCategory(connString, category);
                                if (success = category.Id > 0)
                                    ++count;
                            }
                            Utilities.Utilities.Log(message: success ? $"." : $"X", isWriteLine: false, addTime: false);
                        }
                        success = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: $"[Error]{ex.Message}", feedLine: true, isWriteLine: true, addTime: true);
            }
            return success;
        }
        internal static void PollingCategory(string connString, string code)
        {
            Boolean success = true;
            _WebDriverSetting = new WebDriverSettingInfo(true, false);
            Utilities.Utilities.Log(message: "Polling Categories Start ... ", isWriteLine: false, addTime: true);
            try
            {
                WebsiteInfo website = DataOperation.GetWebsite(connString, code);
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                driver.Navigate().GoToUrl(website.Url);
                IReadOnlyCollection<IWebElement> elements = wait.Until(d => d.FindElements(By.XPath("//div[@class='fsdDeptBox']")));
                if (elements != null)
                {
                    Utilities.Utilities.Log(message: $"[{elements.Count}]", isWriteLine: true);
                    foreach (IWebElement element in elements)
                    {
                        CategoryInfo parent = new CategoryInfo(element);
                        parent.Id = DataOperation.UpdateCategory(connString, parent);
                        IReadOnlyCollection<IWebElement> eles = element.FindElements(By.XPath(".//div[@class='fsdDeptCol']/a"));
                        if (eles != null)
                        {
                            Utilities.Utilities.Log(message: $"[{parent.Name}] [{eles.Count}] -> ", isWriteLine: false, addTime: true);
                            Int32 count = 0;
                            foreach (IWebElement ele in eles)
                            {
                                CategoryInfo category = new CategoryInfo("", parent, ele);
                                if (success = category.HasError)
                                {
                                    category.Id = DataOperation.UpdateCategory(connString, category);
                                    if (success = category.Id > 0)
                                        ++count;
                                }
                                Utilities.Utilities.Log(message: success ? $"." : $"X", isWriteLine: false, addTime: false);
                            }
                            Utilities.Utilities.Log(message: $" [{count}] [Done]", isWriteLine: true, addTime: false);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Utilities.Utilities.Log(message: "Polling Categories Start ... ", feedLine: true, isWriteLine: true, addTime: true);
            }
        }
    }
}
