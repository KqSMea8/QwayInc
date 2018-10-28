/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.09.08
 *   Description: 
 *       Company: Qway Inc.
 *  Project Name: Hotels
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
using System.IO;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

using System.Threading;

using Utilities;
using HtmlAgilityPack;
namespace Alibaba
{
    public static partial class BusinessLogic
    {
        public static void PollingHotel(String connString, String code)
        {
            switch (code)
            {
                case "H":  //Polling Hotel from html pages
                    //pollingHotelFromHtmlPage(connString, @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Hotels");
                    pollingHotel(connString);
                    break;
                case "E":  //Polling Hotel from html pages
                    //pollingHotelFromHtmlPage(connString, @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Hotels");
                    pollingEmail(connString);
                    break;
            }
        }

        private static void pollingEmail(string connString)
        {
            Boolean success = false;
            WebDriverSettingInfo webDriverSetting = new WebDriverSettingInfo(hideCommand: true, hideBrowser: false);
            List<HotelInfo> hotels = DataOperation.GetHotels(connString);
            display($"polling Email: {hotels.Count}:", isWriteLine: true, addTime: true);
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: webDriverSetting.HideBrowser, hideCommand: webDriverSetting.HideCommand, isMaximized: true);
            Int32 count = 0;
            foreach (HotelInfo hotel in hotels)
            {
                display($"[{hotel.Id}] {hotel.Name} -> ", isWriteLine: false, addTime: true);
                try
                {
                    driver.Navigate().GoToUrl("https://www.google.ca");
                    IWebElement element;
                    element = driver.FindElement(By.Id("lst-ib"));
                    element.SendKeys($"{hotel.Name}, {hotel.Region}");
                    element.SendKeys(Keys.Return);
                    element = driver.FindElement(By.XPath("//a[@class='ab_button' and text()='Website']"));
                    element.Click();

                    hotel.Update(driver);
                }
                catch (Exception ex)
                {
                    hotel.Comments = ex.Message;
                }
                success = DataOperation.AddHotel(connString, hotel);
                ++count;
                display($" [Done]", isWriteLine: true);
            }
            driver.Quit();
        }

        private static Int32 scrollToBottom(ChromeDriver driver, String uniqueKey, Int32 sleepSeconds = 2, Boolean isProgressShown = false)
        {
            Int32 pages = 0;
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            do
            {
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                Thread.Sleep(sleepSeconds * 1000);
                if (isProgressShown)
                    display($".", isWriteLine: false);
                ++pages;
            } while (!driver.PageSource.Contains(uniqueKey));
            return pages;
        }
        private static void pollingHotel(String connString)
        {
            WebDriverSettingInfo webDriverSetting = new WebDriverSettingInfo(hideCommand: true, hideBrowser: false);
            Boolean success = false;
            String baseUrl = "https://ca.hotels.com/search.do?resolved-location=CITY:{id}:UNKNOWN:UNKNOWN&f-star-rating=3,2&destination-id={id}&q-destination={region}&q-check-in={checkin}&q-check-out={checkout}&q-rooms=1&q-room-0-adults=2&q-room-0-children=0";
            baseUrl = baseUrl.Replace("{checkin}", "2018-09-25");
            baseUrl = baseUrl.Replace("{checkout}", "2018-09-26");
            Dictionary<String, String> cities = new Dictionary<string, string>()
            {
                {"170961","Kitchener, Ontario, Canada" },
                {"1633548","Hamilton, Ontario" },
                {"169995","Quebec, Quebec, Canada" },
                {"169714","Ottawa, Ontario, Canada" },
                {"167228","Edmonton, Alberta, Canada" },
                {"169003","Calgary, Alberta, Canada" },
                {"169712","Vancouver, British Columbia, Canada" },
                {"169716","Montreal, Quebec, Canada" },
                {"1636865","Toronto, Ontario, Canada" },
                {"170165","Winnipeg, Manitoba, Canada" }
            };
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: webDriverSetting.HideBrowser, hideCommand: webDriverSetting.HideCommand, isMaximized: true);
            foreach (String key in cities.Keys)
            {
                display($"{cities[key]} -> ", isWriteLine: false, addTime: true);
                String url = baseUrl.Replace("{id}", key);
                url = url.Replace("{region}", cities[key]);
                driver.Navigate().GoToUrl(url);
                Int32 count = scrollToBottom(driver, "info unavailable-info", 2, true);
                IReadOnlyCollection<IWebElement> elements = driver.FindElementsByXPath("//ol/li");
                display($" pages: [{count}] -> [{elements.Count}]", isWriteLine: false);
                count = 0;
                foreach (IWebElement element in elements)
                {
                    HotelInfo hotel = new HotelInfo(driver, element, cities[key]);
                    if (!String.IsNullOrEmpty(hotel.Name))
                    {
                        success = DataOperation.AddHotel(connString, hotel);
                        ++count;
                        if (count % 10 == 0)
                            display($".", isWriteLine: false);
                    }
                }
                display($" Done[{count}]", isWriteLine: true);
            }
            driver.Quit();
        }
        private static void pollingHotelFromHtmlPage(String connString, String filePath)
        {
            Boolean success = false;
            DirectoryInfo di = new DirectoryInfo(filePath);
            FileInfo[] files = di.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            foreach (FileInfo fi in files)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(fi.FullName);
                String region = System.Text.RegularExpressions.Regex.Match(fi.FullName, @"in (.+?).html").Groups[1].Value;
                HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//ol/li");
                display($"{region}: {nodeList.Count}");
                Int32 index = 1;
                //HtmlNode node = doc.DocumentNode.SelectSingleNode($"//ol/li[{index++}]");
                HtmlNode node = doc.DocumentNode.SelectSingleNode($"//ol[1]");
                while (node != null)
                {
                    HotelInfo hotel = new HotelInfo(node, region);
                    success = DataOperation.AddHotel(connString, hotel);
                    node = doc.DocumentNode.SelectSingleNode($"//ol/li[{index++}]");
                };
                //foreach (HtmlNode node in nodeList)
                //{
                //    HotelInfo hotel = new HotelInfo(node, region);
                //    success = DataOperation.AddHotel(connString, hotel);
                //}
            }
        }
    }
}
