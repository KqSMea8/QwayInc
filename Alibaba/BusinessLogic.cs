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
using System.Collections.ObjectModel;

using Utilities;
using EAGetMail;

namespace Alibaba
{
    public static partial class BusinessLogic
    {
        #region Properties
        private static TimeSpan _TimeSpanWaiting = new TimeSpan(0, 5, 0);
        public static Boolean HasError { get { return ErrorMessages.Count > 0 || !String.IsNullOrEmpty(ErrorMessage); } }

        public static String ErrorMessage = String.Empty;
        public static Dictionary<String, String> ErrorMessages = new Dictionary<String, String>();

        private static WebDriverSettingInfo _WebDriverSetting = new WebDriverSettingInfo(false, false);
        private static Boolean _Debug = true;
        private static Point _CursorPosition = new Point(0, 0);
        private static String _LogFileName = @"Alibaba.log";
        private static DateTime _DateTimeStart;
        public static void PollingProductCategory(String connString, String code)
        {
            String status = Utilities.Utilities.GetCurrentMethodName();
            display(message: $"*****Polling Product Categories Start ... ", isWriteLine: true, addTime: true);
            try
            {
                WebsiteInfo website = DataOperation.GetWebsite(connString, code);
                WebsiteInfo websiteSupplier = DataOperation.GetWebsite(connString, "SP");
                String html = String.Empty;
                using (WebClient client = new WebClient())
                {
                    html = client.DownloadString(website.Url);
                }
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                HtmlNodeCollection nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='item util-clearfix']");
                foreach (HtmlNode node in nodes)
                {
                    SupplierCategoryInfo category = new SupplierCategoryInfo(node, 1, 0, websiteSupplier.Url);
                    Int32 parentId = DataOperation.UpdateProductCategory(connString, category);
                    display($"[{parentId}] [{category.Name}] .... ", false, true);
                    Int32 count = 0;
                    HtmlNodeCollection nodes2 = node.SelectNodes(".//div[@class='sub-item']");
                    foreach (HtmlNode node2 in nodes2)
                    {
                        category = new SupplierCategoryInfo(node2, 2, parentId, websiteSupplier.Url);
                        parentId = DataOperation.UpdateProductCategory(connString, category);
                        HtmlNodeCollection nodes3 = node2.SelectNodes(".//ul[@class='sub-item-cont util-clearfix']/li");
                        foreach (HtmlNode node3 in nodes3)
                        {
                            category = new SupplierCategoryInfo(node3, 3, parentId, websiteSupplier.Url);
                            category.Id = DataOperation.UpdateProductCategory(connString, category);
                            count++;
                        }
                    }
                    display($"Done. [{count}]", true, false);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }

        }

        public static void CheckingLogin(String connString)
        {
            Boolean success = false;
            Random random = new Random();
            List<SettingsInfo> list = DataOperation.GetSettings(connString);
            display(String.Format("Account login checking [{0}] ....", list.Count), true, true);
            foreach (SettingsInfo setting in list)
            {
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                display(String.Format("[{0}] ....", setting.Email), false, true);
                if (driver != null)
                    success = alibabaLogin(driver, connString, setting, random.Next(4000, 6000));
                else success = false;
                display(String.Format("[{0}]", success ? "OK" : "XX"), true, false);
                driver.Quit();
            }
        }


        private static List<String> getUrlList(ChromeDriver driver, WebDriverWait wait)
        {
            List<String> list = new List<String>();
            String url = driver.Url;
            IReadOnlyCollection<IWebElement> elements = null;
            try
            {
                //elements = wait.Until(d => d.FindElements(By.XPath("//div[@class='company']/a[@class='cd']")));
                elements = driver.FindElements(By.XPath("//div[@class='company']/a[@class='cd']"));
                foreach (IWebElement element in elements)
                {
                    list.Add(element.GetAttribute("href"));
                }
            }
            catch (NotFoundException) { }
            catch (WebDriverException)
            {
                driver = null;
                driver = WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[getUrlList] ERROR: {0}", ex.Message);
            }
            return list;
        }

        private static void updateSupplierUrls(string connString, SupplierCategoryInfo supplierCategory, List<String> urlList)
        {
            foreach (String url in urlList)
            {
                SQLResultStatusInfo result = DataOperation.UpdateSupplierURL(connString, url, "", "", supplierCategory);
                if (result.Id > 0) ++supplierCategory.CountAdded;
            }
        }

        public static void DumpingWebsite(String connString, WebsiteInfo website)
        {
#if DEBUG
#else
            _Debug = false;
#endif
            _Debug = true;
            display(String.Format("[{0}] [{1}] [{2}]", website.Code, website.DumpLevel, website.WebsiteUrl), true, true);
            dumpingWebsite(connString, website);
        }


        private static Boolean dumpingWebsite(String connString, WebsiteInfo websiteParrent)
        {
            Boolean success = true;
            if (websiteParrent.WebsiteUrl.IsWebSiteAvailable())
            {
                using (WebClient client = new WebClient())
                {
                    websiteParrent.Metadata = client.DownloadString(websiteParrent.WebsiteUrl);
                }
            }
            else
            {
                websiteParrent.Metadata = "Website not Available";
                websiteParrent.Active = false;
            }
            SQLResultStatusInfo resultStatus = DataOperation.UpdateWebsite(connString, websiteParrent);
            if (resultStatus.Code == "INSERT" && websiteParrent.DumpLevel > 0)
            {
                Int32 parrentId = resultStatus.Id;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(websiteParrent.Metadata);
                HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//*[@href]");
                if (nodeList != null && nodeList.Count > 0)
                {
                    Int32 count = 0;
                    display(String.Format("Dumping [{0}] [{1}]", websiteParrent.WebsiteUrl, nodeList.Count), true, true, true);
                    foreach (HtmlNode node in nodeList)
                    {
                        if (node.Attributes.Contains("href"))
                        {
                            HtmlAttribute attr = node.Attributes["href"];
                            WebsiteInfo website = new WebsiteInfo(websiteParrent);
                            website.ParentId = parrentId;
                            website.Url = attr.Value;
                            website.Name = node.Name;
                            if (!website.IsIgnored)
                            {
                                count++;
                                display(String.Format(". ", ""), false, false);
                                success = dumpingWebsite(connString, website);
                            }
                        }
                    }
                    display(String.Format("[{0}] dumpped.", count), true, true, true);
                }
            }
            return success;
        }

        #endregion
        #region Public Methods

        public static void PollingSuppliersFromCusinessCard(String connString)
        {
            _WebDriverSetting = new WebDriverSettingInfo(false, false);
            Boolean success = false;
            Int32 total = 0;
            Int32 count = 0;
            Random random = new Random();

            List<BusinessCardInfo> list = DataOperation.GetSupplierUrlFromBC(connString);
            total = list.Count;
            display(String.Format("*****Polling Suppliers from URL Start ... [{0}]", total), true, true);
            try
            {
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (BusinessCardInfo businessCard in list)
                {
                    display(String.Format("[{0}/{1}] - [{2}] {3} Processing ...", ++count, total, businessCard.Id, businessCard.Company), false, true);
                    try
                    {
                        //SupplierInfo supplier = getSupplier(connString, driver, wait, businessCard.CompanyProfileUrl, new SupplierCategoryInfo(businessCard.CategoryId, businessCard.CategoryName, businessCard.CategoryLevel));
                        //success = DataOperation.UpdateSupplierUrlStatus(connString, businessCard, supplier);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("[Post] ERROR: {0}", ex.Message);
                    }
                    Console.WriteLine(" Done", "");
                }
                driver.Quit();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
            }
            Console.WriteLine("*****Polling Suppliers End. [{0}]", total);
        }
        private static void display(String message, Boolean isWriteLine = true, Boolean addTime = false, Boolean feedLine = false, Boolean isClear = false, Boolean isPositionKept = false)
        {
            if (_Debug)
            {
                if (isPositionKept)
                    Console.SetCursorPosition(_CursorPosition.X, _CursorPosition.Y);
                else
                {
                    if (feedLine)
                        message = $"\n{message}";
                    String time = String.Format("[{0:HH:mm:ss.fff}] ", DateTime.Now);
                    message = String.Format("{0}{1}{2}", addTime ? time : "", message, isWriteLine ? "\n" : "");
                }
                Console.Write(message);
                if (!String.IsNullOrEmpty(_LogFileName))
                    System.IO.File.AppendAllText(_LogFileName, message);
            }
        }

        public static void PollingSupplierCategories(String connString)
        {
            Boolean success = true;
            _WebDriverSetting = new WebDriverSettingInfo(false, false);
            Console.WriteLine("*****Polling Supplier Categories Start ... ", "");
            try
            {
                WebsiteInfo website = DataOperation.GetWebsite(connString, "SC");
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                driver.Navigate().GoToUrl(website.Url);
                IReadOnlyCollection<IWebElement> elements = wait.Until(d => d.FindElements(By.XPath("//div[@class='ui-box ui-box-normal ui-box-wrap clearfix']/div/dl")));
                if (elements != null)
                {
                    Dictionary<String, SupplierCategoryInfo> listLevel1 = new Dictionary<String, SupplierCategoryInfo>();
                    success = updateSupplierCategories(connString, driver, wait, elements, ref listLevel1);
                    Console.WriteLine("Level 2 Start [{0}] ... ", listLevel1.Count);
                    Int32 index = 0;
                    foreach (String url in listLevel1.Keys)
                    {
                        Console.Write("[{0}] {1} ... ", ++index, listLevel1[url].Name);
                        if (url.IsWebSiteAvailable())
                        {
                            driver.Navigate().GoToUrl(url);
                            elements = wait.Until(d => d.FindElements(By.XPath("//div[@class='ui-box-content']/div/ul/li")));
                            success = updateSupplierCategories(connString, driver, wait, elements, listLevel1[url]);
                            Console.WriteLine("Done.", "");
                        }
                        else
                            Console.WriteLine("[{0}] Can't open.", url);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }

        }
        public static void PollingSuppliers(String connString)
        {
            WebDriverSettingInfo webDriverSetting = new WebDriverSettingInfo(true, true);
            Boolean success = false;
            Int32 totalCategory = 0;
            Int32 countCategory = 0;
            Random random = new Random();

            List<SupplierCategoryInfo> list = DataOperation.GetCategoryList(connString, "SA");
            totalCategory = list.Count;
            display(String.Format("*****Polling Suppliers Start ... [{0}]", totalCategory), true, true);
            try
            {
                ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: webDriverSetting.HideBrowser, hideCommand: webDriverSetting.HideCommand);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
                foreach (SupplierCategoryInfo category in list)
                {
                    display(String.Format("*****[{0}/{1}] - {2} Processing ...", ++countCategory, totalCategory, category.Name), true, true);
                    //if (category.Level == 1)
                    try
                    {
                        driver.Navigate().GoToUrl(category.Url);
                        switch (category.Level)
                        {
                            case 1:
                                //success = updateSupplierCategories(connString, driver, wait, category);
                                break;
                            case 2:
                            //pollingSuppliers2(connString);
                            //break;
                            case 3:
                                success = pollingSuppliers(connString, driver, wait, category);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (TimeoutException ex) { throw ex; }
                    catch (System.Exception ex)
                    {
                        display(String.Format("[Post] ERROR: {0}", ex.Message), true, true);
                    }
                    display(String.Format("*****[{0}/{1}] - {2} Done", countCategory, totalCategory, category.Name), true, true);
                    //break;
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
        public static Boolean PollingBusinessCards(String connString, String urlSent, String urlReceived, Boolean forAll)
        {
            StatusExtension.Initialize();
            Boolean success = false;
            success = pollingBusinessCardsSent(connString, urlSent, forAll);
            success = pollingBusinessCardsReceived(connString, urlReceived, forAll);
            return success;
        }
        public static Boolean SendingMessage(String connString, Int32 count)
        {
            Boolean success = true;
            Int32 totalSetting = 0;
            Int32 countSetting = 0;
            Int32 totalSupplier = 0;
            Int32 countSupplier = 0;
            Random random = new Random();

            String originalMessage = "Properties.Resources.MessageScript";
            List<SettingsInfo> list = getSettingList(connString, count, "M");
            totalSetting = list.Count;
            foreach (SettingsInfo setting in list)
            {
                Console.WriteLine("*****[{0}/{1}] - {2} Processing ...", ++countSetting, totalSetting, setting.Email);
                List<Supplier> suppliers = getSupplierList(connString, setting.OpenPages);
                totalSupplier = suppliers.Count;
                try
                {
                    ChromeDriver driver = Utilities.WebDriverExtension.AlibabaLogin(setting.Email, setting.Password, random.Next(4000, 6000));
                    WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 30));
                    foreach (Supplier supplier in suppliers)
                    {
                        if (supplier.IsWebSiteAvailable)
                        {
                            Console.Write("[{0}/{1}] - {2} Posting ...", ++countSupplier, totalSupplier, supplier);
                            supplier.Account = setting.Account;
                            try
                            {
                                driver.Navigate().GoToUrl(supplier.ProfileUrl);
                                String url = Utilities.WebDriverExtension.GetElementValue(driver, wait, "//a[@class='message-send ui-button ui-button-primary']", "href");

                                driver.Navigate().GoToUrl(url);
                                String contact = supplier.ContactName;
                                if (String.IsNullOrEmpty(contact))
                                    contact = Utilities.WebDriverExtension.GetElementValue(driver, wait, "//span[@class='company-contact']").Split(' ')[0];
                                contact = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contact.ToLower());

                                Utilities.WebDriverExtension.CheckElement(driver, wait, @"//input[@id='respond-in-oneday']", false);
                                Utilities.WebDriverExtension.CheckElement(driver, wait, @"//input[@id='agree-share-bc']", true);
                                System.Threading.Thread.Sleep(random.Next(1000, 2000));
                                IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");
                                driver.SwitchTo().Frame(0);

                                String newMessage = originalMessage.Replace("{Name}", contact);

                                IWebElement element;
                                element = driver.FindElement(By.XPath("//body[@id='tinymce']"));
                                element.SendKeys(newMessage);
                                System.Threading.Thread.Sleep(random.Next(3000, 5000));
                                driver.SwitchTo().DefaultContent();
                                element = wait.Until(d => d.FindElement(By.XPath("//div[@class='send-item']/input")));
                                element.Submit();
                                Console.WriteLine("Done");
                                System.Threading.Thread.Sleep(random.Next(20000, 30000));
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine("[Post] ERROR: {0}", ex.Message);
                            }
                        }
                        else
                            Console.WriteLine("Can't open.");
                        updateSupplierPostDate(connString, supplier);
                        Console.WriteLine("Supplier Updated.");
                    }
                    driver.Quit();
                    success = updateSetting(connString, setting);
                    Console.WriteLine("*****[{0}] updated and Done", setting.Email);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
                    success = false;
                    break;
                }
            }
            Console.WriteLine("Completed [{0}]", success);
            return success;
        }
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="connString"></param>
        public static Boolean PollingBusinessCards2(String connString, string filePath)
        {
            Boolean success = false;
            initializeError();
            FileInfo[] files = getFiles(filePath);
            DataTable dt = new DataTable();
            String fileName = String.Empty;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[qwi].[AB.UpdateReceivedCard]";
                    cmd.CommandTimeout = 0;
                    foreach (FileInfo file in files)
                    {
                        fileName = file.FullName;
                        //fileName = @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Supplier\Technology\Technology Suppliers - Reliable Technology Suppliers and Manufacturers at Alibaba.com - Page 16.html";
                        List<BusinessCard> businessCards = getBusinessCards(connString, filePath, fileName);
                        updateBusinessCards(conn, cmd, businessCards);
                        Console.WriteLine("[{0}] suppliers updated", businessCards.Count);
                    }

                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("ERROR in {1}:\n{0}", ex.Message, fileName);
                }
            }
            return success;
        }

        public static void SavingSuppliers(String connString, String saveFolder, Boolean multipleTimesTry = false)
        {
            Int32 count = 0;
            Int32 tries = 0;
            String supplierName = String.Empty;
            ChromeDriver driver = getChromeDriver();
            if (driver != null)
            {
                DataTable dtSuppliers = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSuppliers]", parameters: null);
                foreach (DataRow row in dtSuppliers.Rows)
                {
                    supplierName = row["CompanyName"].ToString();
                    Console.WriteLine("[{0}] ... ", supplierName);
                    String url = getSupplierURL(driver, supplierName);
                    if (!String.IsNullOrEmpty(url))
                        if (!url.StartsWith(@"http://alisec.taobao.com"))
                        {
                            saveSupplierWebpage(url, saveFolder, supplierName);
                            Console.WriteLine("Done [{0}/{1}]", ++count, tries);
                        }
                        else
                        {
                            Console.WriteLine("Can't continue. [{0}] pages saved. [{1}] tries.", count, ++tries);
                            if (multipleTimesTry)
                            {
                                driver.Close();
                                if (tries < 10)
                                {
                                    driver = getChromeDriver();
                                    count = 0;
                                    if (driver != null) continue; else break;
                                }
                                else
                                    break;
                            }
                            else break;
                        }
                    else
                        Console.WriteLine("ERROR: {0}", ErrorMessage);
                }
                driver.Close();
            }
            else
                Console.WriteLine("Can't Operate");
        }
        public static Boolean PollingSuppliers(String connString, String filePath, String fileDestPath, Boolean checkExists = false)
        {
            Boolean success = false;
            initializeError();
            FileInfo[] files = getFiles(filePath);
            String logFile = System.IO.Path.Combine(filePath, "Supplier.csv");
            DataTable dt = new DataTable();
            String fileName = String.Empty;
            using (System.IO.StreamWriter logStreamWriter = new System.IO.StreamWriter(logFile))
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "[qwi].[AB.UpdateSupplier]";
                        cmd.CommandTimeout = 0;
                        foreach (FileInfo file in files)
                        {
                            fileName = file.FullName;
                            //fileName = @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Supplier\New\Yiwu Blueastatine Bags & Cases Firm.html";
                            List<Supplier> suppliers = getSuppliers(filePath, fileName, logStreamWriter, checkExists);
                            if (suppliers.Count > 0)
                            {
                                updateSupplier(conn, cmd, suppliers);
                                Console.WriteLine("[{0}] suppliers updated", suppliers.Count);
                                moveFiles(fileName, fileDestPath);
                            }
                            else
                            {
                                updateSupplierNotFound(connString, fileName);
                            }
                        }

                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("ERROR in {1}:\n{0}", System.IO.Path.GetFileName(fileName), ex.Message);
                        logStreamWriter.WriteLine(String.Format("{0},{1},{2}", "Error", ex.Message, fileName));
                    }
                }
            }
            return success;
        }
        public static void OpenSupplierWebPages(String connString, Int32 pages, String account)
        {
            Int32 count = 0;
            List<Supplier> suppliers = getSuppliers(connString, pages, account);
            foreach (Supplier supplier in suppliers)
            {
                try
                {
                    Console.Write("{0}  {1} ", supplier, supplier.IsWebSiteAvailable ? "opening..." : "can't open.");
                    if (supplier.IsWebSiteAvailable)
                    {
                        System.Diagnostics.Process.Start(supplier.ProfileUrl);
                        Console.WriteLine("Done: {0}", ++count);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("\nERROR: {0}\n{1}", supplier, ex.Message);
                    supplier.Errors.Add("OpenWeb", ex.Message);
                }
                updateSupplierPostDate(connString, supplier);
            }
            Console.WriteLine("[{0}] opened", count);
        }

        #endregion
        #region Private Mesthods

        private static bool updateSupplierCategories(String connString, ChromeDriver driver, WebDriverWait wait, IReadOnlyCollection<IWebElement> elementsParent, SupplierCategoryInfo supplierCategoryParent)
        {
            Boolean success = true;
            SupplierCategoryInfo supplierCategory = null;
            foreach (IWebElement element in elementsParent)
            {
                ReadOnlyCollection<IWebElement> elements = null;
                element.GetElements(xPath: ".//ul/li/a", elements: ref elements);
                if (elements == null || elements.Count == 0)
                {
                    supplierCategory = new SupplierCategoryInfo(driver, wait, element, supplierCategoryParent, false);
                    if (!supplierCategory.HasError)
                    {
                        supplierCategory.Id = DataOperation.UpdateSupplierCategory(connString, supplierCategory);
                    }
                    else
                        Console.WriteLine("ERROR: ", supplierCategory.Errors.ToString2());
                }
                else
                {
                    foreach (IWebElement ele in elements)
                    {
                        SupplierCategoryInfo subSupplierCategory = new SupplierCategoryInfo(driver, wait, ele, supplierCategory, true);
                        if (!subSupplierCategory.HasError)
                        {
                            if (subSupplierCategory.Name != "..." && !String.IsNullOrEmpty(subSupplierCategory.Url) && !String.IsNullOrEmpty(subSupplierCategory.Name))
                                subSupplierCategory.Id = DataOperation.UpdateSupplierCategory(connString, subSupplierCategory);
                        }
                        else
                            Console.WriteLine("ERROR: ", subSupplierCategory.Errors.ToString2());
                    }
                }
            }
            return success;
        }

        private static bool updateSupplierCategories(String connString, ChromeDriver driver, WebDriverWait wait, IReadOnlyCollection<IWebElement> elements, ref Dictionary<String, SupplierCategoryInfo> listLevel1)
        {
            Console.Write("Polling Supplier Categories Level 1 ... ", "");
            Boolean success = true;
            foreach (IWebElement element in elements)
            {
                SupplierCategoryInfo supplierCategory = new SupplierCategoryInfo(driver, wait, element);
                if (!supplierCategory.HasError)
                {
                    supplierCategory.Id = DataOperation.UpdateSupplierCategory(connString, supplierCategory);
                    //supplierCategory.Level = 2;
                    foreach (String key in supplierCategory.SubCategories.Keys)
                        DataOperation.UpdateSupplierCategory(connString, new SupplierCategoryInfo(supplierCategory, supplierCategory.SubCategories[key], key, true));
                    listLevel1[supplierCategory.Url] = supplierCategory;
                }
                else
                    Console.WriteLine("ERROR: ", supplierCategory.Errors.ToString2());

            }
            Console.WriteLine("Done.", "");
            return success;
        }


        private static Boolean pollingSuppliers(String connString, ChromeDriver driver, WebDriverWait wait, SupplierCategoryInfo supplierCategory)
        {
            Boolean success = true;
            Random random = new Random();
            Int32 totalPage, currentPage, count;
            try
            {
                totalPage = getTotalPages(driver, wait, "//div[@class='ui2-pagination-pages']/span");
                String rootUrl = String.Empty;
                if (totalPage > 1)
                {
                    rootUrl = driver.GetElementValue(wait, "//div[@class='ui2-pagination-pages']/a[@class='next']", "href");
                    rootUrl = rootUrl.Substring(0, rootUrl.Length - 1);
                }
                do
                {
                    currentPage = getCurrentPage(driver, wait, "//div[@class='ui2-pagination-pages']/span[@class='current']");
                    String currentUrl = driver.Url;
                    display(String.Format("Page {0} of {1} in processing ", currentPage, totalPage), false, true);
                    List<String> urlList = getUrlList(driver, wait);
                    display(String.Format(" [{0}] ", urlList.Count), false, false);
                    List<SupplierInfo> suppliers = getSuppliers(connString, driver, wait, urlList, supplierCategory);
                    success = DataOperation.UpdateSuppliers(connString, suppliers);
                    display(String.Format("[{0}] loaded.", suppliers.Count), true, true);
                    String gotoUrl = String.Format("{0}{1}", rootUrl, currentPage + 1);
                    if (currentPage < totalPage && gotoUrl.IsWebSiteAvailable())
                    {
                        driver.Navigate().GoToUrl(gotoUrl);
                        totalPage = getTotalPages(driver, wait, "//div[@class='ui2-pagination-pages']/a", 2);
                    }
                    else break;

                    //    if (currentPage < totalPage)
                    //{
                    //    driver.Navigate().GoToUrl(currentUrl);
                    //    count = 0;
                    //    do
                    //    {
                    //        try
                    //        {
                    //            success = driver.ClickElement(wait, "//div[@class='ui2-pagination-pages']/a[@class='next']");
                    //            if (!success)
                    //                break;
                    //        }
                    //        catch (System.Exception ex) { }
                    //        System.Threading.Thread.Sleep(random.Next(2000, 5000));
                    //    } while (!success && count++ < 10);
                    //}
                    //} while (currentPage < totalPage);
                } while (true);
                success = DataOperation.UpdateSupplierCategoryStatus(connString, supplierCategory);
                System.Threading.Thread.Sleep(random.Next(20000, 30000));
                display(String.Format("*****[{0}] updated and Done", supplierCategory.Name), true, true);
            }
            catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                display(String.Format("\n[pollingSuppliers] ERROR: {0}", ex.Message), true, true);
                success = false;
            }
            return success;
        }

        private static List<SupplierInfo> getSuppliers(String connString, ChromeDriver driver, WebDriverWait wait, List<String> urlList, SupplierCategoryInfo category)
        {
            List<SupplierInfo> suppliers = new List<SupplierInfo>();
            foreach (String url in urlList)
            {
                String errorMessage = String.Empty;
                String status = String.Empty;
                //SupplierInfo supplier = getSupplier(connString, driver, wait, url, category);
                SupplierInfo supplier = new SupplierInfo(driver, wait, url, category);
                display(supplier.HasError ? "X" : ".", false, false);
                if (!supplier.HasError)
                {
                    DataOperation.UpdateSupplier(connString, supplier);
                    suppliers.Add(supplier);
                }
                else
                    DataOperation.UpdateSupplierURL(connString, url, supplier.Status, supplier.ErrorMessage, category);
            }
            return suppliers;
        }
        private static Boolean pollingBusinessCardsReceived(string connString, string urlReceived, Boolean forAll)
        {
            Boolean success = false;
            Int32 totalSetting = 0;
            Int32 countSetting = 0;
            Int32 totalPage = 0;
            Int32 currentPage = 0;
            Random random = new Random();

            String originalMessage = "Properties.Resources.MessageScript";
            //List<SettingsInfo> list = getSettingList(connString, 1, "XXX");
            List<SettingsInfo> list = getSettingList(connString, 1, "M");
            totalSetting = list.Count;
            foreach (SettingsInfo setting in list)
            {
                Console.WriteLine("*****[{0}/{1}] - {2} Processing ...", ++countSetting, totalSetting, setting.Email);
                try
                {
                    ChromeDriver driver = Utilities.WebDriverExtension.AlibabaLogin(setting.Email, setting.Password, random.Next(4000, 6000));
                    WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));
                    try
                    {
                        driver.Navigate().GoToUrl(urlReceived);
                        totalPage = getTotalPages(driver);
                        do
                        {
                            currentPage = getCurrentPage(driver, wait);
                            Console.Write("Page {0} of {1} in processing ...", currentPage, totalPage);
                            List<BusinessCardInfo> cards = getBusinessCardsReceived(driver, setting.Account, setting.Email);
                            success = DataOperation.UpdateBusinessCards(connString, cards);
                            Console.WriteLine("Done. [{0}] [{1}]", cards.Count, success);
                            if (!forAll && cards.Where(card => card.Result.Code == "INSERT").FirstOrDefault<BusinessCardInfo>() == null)
                            {
                                Console.WriteLine("Updated before.", "");
                                break;
                            }
                            if (currentPage < totalPage)
                            {
                                success = driver.ClickElement(wait, "//div[@class='ui2-pagination-pages']/a[@class='next']");
                                System.Threading.Thread.Sleep(random.Next(2000, 3000));
                            }
                        } while (currentPage < totalPage);
                        System.Threading.Thread.Sleep(random.Next(20000, 30000));
                        success = updateSetting(connString, setting);
                        Console.WriteLine("*****[{0}] updated and Done", setting.Email);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("[Post] ERROR: {0}", ex.Message);
                    }
                    driver.Quit();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
                    success = false;
                    break;
                }
            }
            Console.WriteLine("Completed [{0}]", success);
            return success;
        }

        private static List<BusinessCardInfo> getBusinessCardsReceived(ChromeDriver driver, String account, String accountEmail)
        {
            List<BusinessCardInfo> cards = new List<BusinessCardInfo>();
            var elements = driver.FindElementsByXPath("//div[@class='ui2-list-body list-body']/ul/li");
            foreach (IWebElement element in elements)
                cards.Add(new BusinessCardInfo(driver, element, account, accountEmail, false));
            return cards;
        }

        private static List<BusinessCardInfo> getBusinessCardsSent(ChromeDriver driver, String account, String accountEmail)
        {
            List<BusinessCardInfo> cards = new List<BusinessCardInfo>();
            var elements = driver.FindElementsByXPath("//li[@class='ui2-list-item list-item list-item-sent']");
            foreach (IWebElement element in elements)
                cards.Add(new BusinessCardInfo(driver, element, account, accountEmail, true));
            return cards;
        }

        private static Boolean pollingBusinessCardsSent(String connString, String urlSent, Boolean forAll)
        {
            Boolean success = false;
            Int32 totalSetting = 0;
            Int32 countSetting = 0;
            Int32 totalPage = 0;
            Int32 currentPage = 0;
            Random random = new Random();

            String originalMessage = "Properties.Resources.MessageScript";
            List<SettingsInfo> list = getSettingList(connString, 1, "XXX");
            //List<SettingsInfo> list = getSettingList(connString, 1, "M");
            totalSetting = list.Count;
            foreach (SettingsInfo setting in list)
            {
                Console.WriteLine("*****[{0}/{1}] - {2} Processing ...", ++countSetting, totalSetting, setting.Email);
                try
                {
                    ChromeDriver driver = Utilities.WebDriverExtension.AlibabaLogin(setting.Email, setting.Password, random.Next(4000, 6000));
                    WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));
                    try
                    {
                        driver.Navigate().GoToUrl(urlSent);
                        totalPage = getTotalPages(driver);
                        do
                        {
                            currentPage = getCurrentPage(driver, wait);
                            Console.Write("Page {0} of {1} in processing ...", currentPage, totalPage);
                            success = clickRequestCard(driver, wait, random);
                            List<BusinessCardInfo> cards = getBusinessCardsSent(driver, setting.Account, setting.Email);
                            success = DataOperation.UpdateBusinessCards(connString, cards);
                            Console.WriteLine("Done. [{0}] [{1}]", cards.Count, success);
                            if (!forAll && cards.Where(card => card.Result.Status == "NOT_SEND").FirstOrDefault<BusinessCardInfo>() == null)
                            {
                                Console.WriteLine("Updated before.", "");
                                break;
                            }

                            if (currentPage < totalPage)
                            {
                                success = driver.ClickElement(wait, "//a[@class='next']");
                                System.Threading.Thread.Sleep(random.Next(2000, 3000));
                            }
                        } while (currentPage < totalPage);
                        System.Threading.Thread.Sleep(random.Next(20000, 30000));
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("[Post] ERROR: {0}", ex.Message);
                    }
                    driver.Quit();
                    success = updateSetting(connString, setting);
                    Console.WriteLine("*****[{0}] updated and Done", setting.Email);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
                    success = false;
                    break;
                }
            }
            Console.WriteLine("Completed [{0}]", success);
            return success;
        }


        private static Boolean updateSetting(string connString, SettingsInfo setting)
        {
            Boolean success = true;
            DataTable dataTable = DataOperation.UpdateSetting(connString, setting);
            return success;
        }

        private static List<Supplier> getSupplierList(String connString, Int32 pages)
        {
            DataTable dataTable = DataOperation.GetSupplierList(connString, pages);
            return Utilities.DataExtension.Serialize<Supplier>(dataTable);
        }

        private static List<SettingsInfo> getSettingList(String connString, Int32 count, String flag)
        {
            DataTable dataTable = DataOperation.GetSettingList(connString, count, flag);
            return Utilities.DataExtension.Serialize<SettingsInfo>(dataTable);
        }

        private static void updateEmails(String connString, MailServerInfo mailServer, List<EmailInfo> emails)
        {
            SQLResultStatusInfo sqlResultStatus = null;
            foreach (EmailInfo email in emails)
            {
                sqlResultStatus = DataOperation.UpdateEmail(connString, email);
            }
            DateTime lastEmailRetrived = emails.Max(e => e.SentOn);
            sqlResultStatus = DataOperation.UpdateEmail(connString, mailServer.Email, lastEmailRetrived);
        }

        private static List<EmailInfo> getEmails(String email, EAGetMail.MailClient mailClient, MailInfo[] mails)
        {
            List<EmailInfo> emails = new List<EmailInfo>();
            foreach (EAGetMail.MailInfo mailInfo in mails)
            {
                EAGetMail.Mail mail = mailClient.GetMail(mailInfo);
                emails.Add(new EmailInfo(email, mailInfo, mail));
            }
            return emails;
        }

        private static NameSpace getNameSpace()
        {
            Application mapiApp = new Application();
            if (Process.GetProcessesByName("OUTLOOK").Count() > 0)
            {
                mapiApp = Marshal.GetActiveObject("Outlook.Application") as Application;
            }

            NameSpace mapiNameSpace = mapiApp.GetNamespace("MAPI");
            mapiNameSpace.Logon("", "", Missing.Value, Missing.Value);
            return mapiNameSpace;
        }
        private static void initializeError()
        {
            ErrorMessages = new Dictionary<String, String>();
        }

        private static void updateBusinessCards(SqlConnection conn, SqlCommand cmd, List<BusinessCard> businessCards)
        {
            DataTable dt = new DataTable();
            try
            {
                foreach (BusinessCard businessCard in businessCards)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(SQLExtension.GetSqlParameters(businessCard.GetSQLParameters()));
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR in updateSupplier: {0}", ex.Message);
            }

        }
        private static String getSupplierURL(ChromeDriver driver, String supplierName)
        {
            String url = String.Empty;
            try
            {
                IWebElement element;
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-type']"));
                System.Threading.Thread.Sleep(2000);
                element.Submit();
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-type']/div/ul/li/a[@data-value='suppliers']"));
                System.Threading.Thread.Sleep(2000);
                element.Submit();
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-body']/form/div[@class='ui-searchbar-main']/input"));
                System.Threading.Thread.Sleep(2000);
                element.Clear();
                element.SendKeys(supplierName);
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-body']/form/input[@class='ui-searchbar-submit']"));
                System.Threading.Thread.Sleep(2000);
                element.Submit();
                url = driver.Url;
                System.Threading.Thread.Sleep(60000);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return url;
        }
        private static Boolean saveSupplierWebpage(String url, String saveFolder, String supplierName)
        {
            try
            {
                supplierName = supplierName.RemoveInvalidChars();
                String fileFullName = System.IO.Path.Combine(saveFolder, string.Format("{0}.html", supplierName));
                WebClient client = new WebClient();
                client.DownloadFile(url, fileFullName);
            }
            catch (System.Exception ex)
            {
            }
            return true;
        }
        private static ChromeDriver getChromeDriver()
        {
            ChromeOptions optionsChrome = new ChromeOptions();
            optionsChrome.AddArgument("--log-level=3");
            optionsChrome.AddArgument("--disable-logging");

            ChromeDriver driver = new ChromeDriver(optionsChrome);
            try
            {
                driver.Navigate().GoToUrl("https://www.alibaba.com/");
                IWebElement element;
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-type']"));
                System.Threading.Thread.Sleep(1000);
                element.Submit();
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-type']/div/ul/li/a[@data-value='suppliers']"));
                System.Threading.Thread.Sleep(1000);
                element.Submit();
                element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-body']/form/div[@class='ui-searchbar-main']/input"));
            }
            catch (System.Exception ex)
            {
                driver = null;
            }
            return driver;
        }

        private static List<BusinessCard> getBusinessCards(String connString, String filePath, String fileName)
        {
            List<BusinessCard> businessCards = new List<BusinessCard>();
            TextReader tr = new StreamReader(fileName);
            string markup = tr.ReadToEnd();
            tr.Close();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(markup);
            HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//div[@class='ui2-list-body list-body']/ul/li[@class='ui2-list-item list-item']");
            Int32 index = 0;
            Int32 count = 0;
            String status = String.Empty;
            if (nodeList != null)
            {
                count = nodeList.Count;
                Console.WriteLine("[{1}]*****{0}*****[{2}]", fileName, count, nodeList == null);
                foreach (HtmlNode node in nodeList)
                {

                    BusinessCard businessCard = new BusinessCard(connString, fileName, node);
                    if (!businessCard.BusinessCardExists)
                    {
                        businessCards.Add(businessCard);
                        Console.WriteLine("{0}/{1} {2}", ++index, count, businessCard);
                    }
                    else
                    {
                        //status = String.IsNullOrEmpty(businessCard.Id) ? "Id Empty" : String.Format("[{0}] Exists", businessCard.Id);
                    }
                    //break;
                }
            }
            else
            {
                status = "Ignored";
                Console.WriteLine("Ignored: {0}", fileName);
            }
            return businessCards;
        }

        private static void updateSupplierNotFound(String connString, String fileName)
        {
            String supplier = System.IO.Path.GetFileNameWithoutExtension(fileName);
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@CompanyName", supplier)
            };
            DataTable dt = Utilities.SQLExtension.GetDataTableFromStoredProcedure(connString, "[qwi].[AB.UpdateCompanyNotFound]", "", parameters);
            System.IO.File.Delete(fileName);
            Console.WriteLine("***[{0}] Not Found", supplier);
        }

        private static void moveFiles(String fileName, String fileDestPath)
        {
            try
            {

                String destFile = System.IO.Path.Combine(fileDestPath, System.IO.Path.GetFileName(fileName));
                if (System.IO.File.Exists(destFile))
                    System.IO.File.Delete(destFile);
                System.IO.File.Move(fileName, destFile);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("NOT MOVE: {0}\n{1}", System.IO.Path.GetFileName(fileName), ex.Message);
            }
            String destFolder = String.Empty;
            try
            {
                String sourceFolder = fileName.Replace(".html", "_files");
                if (System.IO.Directory.Exists(sourceFolder))
                {
                    destFolder = System.IO.Path.Combine(fileDestPath, System.IO.Path.GetFileName(sourceFolder));
                    if (System.IO.Directory.Exists(destFolder))
                        System.IO.Directory.Delete(destFolder);
                    System.IO.Directory.Move(sourceFolder, destFolder);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("NOT MOVE: {0}\n{1}", System.IO.Path.GetDirectoryName(destFolder), ex.Message);
            }
        }

        private static void updateSupplierPostDate(String connString, Supplier supplier)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(supplier);
            DataTable dt = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierPostDate]", parameters: parameters);
        }

        private static List<Supplier> getSuppliers(String connString, Int32 pages, String account)
        {
            List<Supplier> suppliers = new List<Supplier>();
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(pages);
            DataTable dt = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetWebPageLinks]", parameters: parameters);
            foreach (DataRow row in dt.Rows)
            {
                suppliers.Add(new Supplier(row, account));

            }
            return suppliers;
        }
        private static List<KeyValuePair<String, Object>> getSQLParameters(Supplier supplier)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@CompanyId", supplier.Id),
                new KeyValuePair<String, Object>("@IsWebSiteAvailable", supplier.IsWebSiteAvailable),
                new KeyValuePair<String, Object>("@Account", supplier.Account)
            };
            return parameters;
        }
        private static List<KeyValuePair<String, Object>> getSQLParameters(Int32 pages)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Pages",pages)
            };
            return parameters;
        }

        private static void updateSupplier(SqlConnection conn, SqlCommand cmd, List<Supplier> suppliers)
        {
            DataTable dt = new DataTable();
            try
            {
                foreach (Supplier supplier in suppliers)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(SQLExtension.GetSqlParameters(supplier.GetSQLParameters()));
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR in updateSupplier: {0}", ex.Message);
            }

        }

        private static List<Supplier> getSuppliers(String filePath, String fileName, System.IO.StreamWriter logStreamWriter, Boolean checkExists)
        {
            List<Supplier> suppliers = new List<Supplier>();
            TextReader tr = new StreamReader(fileName);
            string markup = tr.ReadToEnd();
            tr.Close();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(markup);
            HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//div[@class='f-icon m-item  ']");
            Int32 index = 0;
            Int32 count = 0;
            String status = String.Empty;
            if (nodeList != null)
            {
                count = nodeList.Count;
                Console.WriteLine("[{1}]*****{0}*****[{2}]", System.IO.Path.GetFileName(fileName), count, nodeList == null);
                foreach (HtmlNode node in nodeList)
                {

                    Supplier supplier = new Supplier(filePath, fileName, node, checkExists);
                    if (!String.IsNullOrEmpty(supplier.Id))
                    {
                        if (checkExists || !supplier.SupplierExists)
                        {
                            suppliers.Add(supplier);
                            Console.WriteLine("{0}/{1} {2}", ++index, count, supplier);
                        }
                        else
                        {
                            status = String.IsNullOrEmpty(supplier.Id) ? "Id Empty" : String.Format("[{0}] Exists", supplier.Id);
                            logStreamWriter.WriteLine(String.Format("{0},{1}/{2},{3}", status, index, count, fileName));
                        }
                        ////break;
                    }
                    else
                        status = "Id Empty";
                }
            }
            else
            {
                status = "Ignored";
                Console.WriteLine("Ignored: {0}", System.IO.Path.GetFileName(fileName));
            }
            logStreamWriter.WriteLine(String.Format("{0},{1}/{2},{3}", status, index, count, fileName));
            return suppliers;
        }

        private static FileInfo[] getFiles(String filePath)
        {
            DirectoryInfo di = new DirectoryInfo(filePath);
            //FileInfo[] files = di.GetFiles("*.html", SearchOption.AllDirectories);
            FileInfo[] files = di.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            return files;
        }
        private static Dictionary<String, String> getMAPIFolders()
        {
            Dictionary<String, String> dic = new Dictionary<String, String>() {
                {"qway.inc@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.a@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.b@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.c@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.d@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.e@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.f@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc.g@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.ca@hotmail.com", "feedback@service.alibaba.com" },
                {"qway.inc@gmail.com", "feedback@service.alibaba.com" }
            };
            return dic;
        }

        private static SupplierInfo getSupplier(String connString, ChromeDriver driver, WebDriverWait wait, String url, SupplierCategoryInfo category)
        {
            Boolean success = true;
            SupplierInfo supplier = new SupplierInfo();
            String metadata = String.Empty;
            if (url.IsWebSiteAvailable())
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        metadata = client.DownloadString(url);
                    }
                    String flag = String.Empty;
                    driver.Navigate().GoToUrl(url);
                    IWebElement element = driver.CheckElement("//div[@class='banner-copy']");
                    if (element == null)
                    {
                        element = driver.CheckElement("//div[@class='main-wrap region region-type-big']");
                        if (element == null)
                        {
                            element = driver.CheckElement("//div[@class='root']/div[@id='bd']");
                            if (element != null)
                                flag = "B";
                            else
                            {
                                flag = String.Empty;
                                supplier.Status = "L15 - getSuppliers";
                                supplier.Errors[supplier.Status] = "New content format";
                            }
                        }
                        else
                        {
                            flag = "A";
                        }
                    }
                    else
                    {
                        flag = String.Empty;
                        supplier.Status = "L16 - getSuppliers";
                        supplier.Errors[supplier.Status] = element.Text;
                    }
                    if (!String.IsNullOrEmpty(flag))
                    {
                        supplier = new SupplierInfo(driver, element, category, flag);
                        if (!supplier.HasError)
                        {
                            success &= DataOperation.UpdateSupplier(connString, supplier);
                            Console.Write(".");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    supplier.Status = "L12 - getSuppliers";
                    supplier.Errors[supplier.Status] = ex.Message;
                }
            }
            else
            {
                supplier.Status = "L11 - getSuppliers";
                supplier.Errors[supplier.Status] = StatusExtension.ErrorMessage;
            }
            if (_WebDriverSetting.HideError)
                Console.Write(supplier.HasError ? "X" : ".");
            else
                Console.WriteLine("[{0}] ERROR: {1}. Logged.", supplier.Status, supplier.ErrorMessage);
            return supplier;
        }
        #endregion
        //private static List<EmailInfo> getEmails(String mapiFolderName, String senderEmailAddress)
        //{
        //    List<EmailInfo> emails = new List<EmailInfo>();
        //    try
        //    {
        //        NameSpace mapiNameSpace = getNameSpace();
        //        Folders mapiFolders = mapiNameSpace.Folders;
        //        foreach (MAPIFolder mapiFolder in mapiFolders)
        //        {
        //            if (mapiFolder.Name.ToLower() == mapiFolderName.ToLower())
        //            {
        //                mapiFolderName = mapiFolder.Name;
        //                Folder mapiFolderStore = mapiFolder.Store.GetDefaultFolder(OlDefaultFolders.olFolderInbox) as Folder;
        //                parseEmail(ref emails, mapiFolderStore, mapiFolderStore.Name, mapiFolder.Name, senderEmailAddress);
        //                foreach (Folder folder in mapiFolderStore.Folders)
        //                    parseEmail(ref emails, folder, mapiFolderStore.Name, mapiFolder.Name, senderEmailAddress);
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        ErrorMessages.Add("getEmails", ex.Message);
        //    }
        //    return emails;
        //}
        //private static void parseEmail(ref List<Email> emails, Folder folder, String mapiFolderStoreName, String mapiFolderName, String senderEmailAddress)
        //{
        //    foreach (Object obj in folder.Items)
        //    {
        //        MailItem mailItem = obj as MailItem;
        //        if (mailItem != null)
        //        {
        //            if (mailItem.SenderEmailAddress.ToLower() == senderEmailAddress.ToLower())
        //            {
        //                Email email = new Email(mapiFolderName, mailItem.SenderEmailAddress, mapiFolderStoreName, folder.Name);
        //                email.SetProperties(mailItem);
        //                emails.Add(email);
        //            }
        //        }
        //    }

        //}

        //public static Boolean PollingEmails(String connString, Dictionary<String, String> dicMAPIFolders = null)
        //{
        //    Boolean success = false;
        //    initializeError();
        //    Int32 count = 0;
        //    List<Email> emails = new List<Email>();
        //    if (dicMAPIFolders == null)
        //        dicMAPIFolders = getMAPIFolders();
        //    foreach (String mapiFolderName in dicMAPIFolders.Keys)
        //    {
        //        emails = getEmails(mapiFolderName, dicMAPIFolders[mapiFolderName]);
        //        Console.WriteLine("[{0}] {1}: {2}", emails.Count, mapiFolderName, dicMAPIFolders[mapiFolderName]);
        //        save(connString, emails);
        //        count += emails.Count;
        //    }
        //    Console.WriteLine("[{0}] emails in total", count);

        //    return success;
        //}

        //private static Boolean updateSupplierCategories(String connString, ChromeDriver driver, WebDriverWait wait, SupplierCategoryInfo supplierCategory)
        //{
        //    Boolean success = true;
        //    IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("//div[@class='g-float-left g-col-left-3']/ul/li"));
        //    SupplierCategoryInfo currentCategory = new SupplierCategoryInfo();
        //    foreach (IWebElement element in elements)
        //    {
        //        IReadOnlyCollection<IWebElement> subElements = element.FindElements(By.XPath(".//ul/li"));
        //        if (subElements.Count == 0)
        //        {
        //            String url = element.GetElementValue(".//a", "href");
        //            String name = element.GetElementValue(".//a");
        //            currentCategory = new SupplierCategoryInfo(supplierCategory, name, url, false);
        //            success &= updateSupplierCategory(connString, currentCategory);
        //        }
        //        else
        //        {
        //            foreach (IWebElement subElement in subElements)
        //            {
        //                String name = subElement.GetElementValue(".//a");
        //                String url = subElement.GetElementValue(".//a", "href");
        //                success &= updateSupplierCategory(connString, new SupplierCategoryInfo(currentCategory, name, url, true));
        //            }
        //        }
        //    }
        //    Console.WriteLine("*****[{0}] Done. [{1}]", supplierCategory, elements.Count);
        //    return success;
        //}
        //private static Boolean updateSupplierCategory(String connString, SupplierCategoryInfo category)
        //{
        //    DataTable dataTable = DataOperation.UpdateSupplierCategory(connString, category);
        //    SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
        //    category.Id = Convert.ToInt32(sqlResultStatus.Status);
        //    return !StatusExtension.HasError;
        //}
    }
}
