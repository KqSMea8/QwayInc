/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.05.14
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
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;

namespace Utilities
{
    public static class WebDriverExtension
    {
        private const String CHROME_DRIVER_LOCATION = @"C:\ChromeDriver";
        private static TimeSpan _TimeSpanWaiting = new TimeSpan(0, 0, 30);
        #region Public Methods
        public static String GetValue(this ChromeDriver driver, String xPath, String attribute = "", String key = "", WebDriverWait wait = null, String defaultValue = "")
        {
            String value = defaultValue;
            IWebElement element = null;
            if (driver.GetElement(xPath, ref element, key, wait))
                value = element.GetValue(driver, attribute, defaultValue);
            return value;
        }
        public static String GetValue(this IWebElement element, ChromeDriver driver = null, String attribute = "", String defaultValue = "")
        {
            String value = defaultValue;
            if (String.IsNullOrEmpty(attribute))
            {
                if (!element.Displayed && driver != null)
                    value = driver.ExecuteScript("return arguments[0].textContent", element).ToString();
                else value = element.Text;
            }
            else
            {
                value = element.GetAttribute(attribute);
            }
            if (!String.IsNullOrEmpty(value))
            {
                value = WebExtension.DecodeHtml(value);
                value = WebExtension.DecodeUrl(value);
            }
            return value;
        }
        //public static Boolean CheckElement(this ChromeDriver driver, String xPath, String key, ref IWebElement element)
        //{
        //    Boolean success = false;
        //    StatusExtension.Initialize();
        //    //TimeSpan timeSpan = driver.Manage().Timeouts().ImplicitWait;
        //    //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        //    try
        //    {
        //        if (String.IsNullOrEmpty(key) || driver.PageSource.Contains(key))
        //        {
        //            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        //            element = driver.FindElement(By.XPath(xPath));
        //            success = element != null;
        //            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
        //        }
        //    }
        //    catch (NoSuchElementException ex) { }
        //    catch (NotFoundException ex) { }
        //    catch (Exception ex) { }
        //    //driver.Manage().Timeouts().ImplicitWait = timeSpan;
        //    return success;
        //}
        public static Boolean CheckAlert(this ChromeDriver driver)
        {
            Boolean success = false;
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                success = true;
            }
            catch (NoAlertPresentException ex)
            {
            }
            return success;

        }
        public static Boolean GetGoogleSearches(this ChromeDriver driver, String keyword)
        {
            Boolean success = false;
            StatusExtension.Initialize();
            IWebElement element = null;
            try
            {
                driver.Navigate().GoToUrl("https://www.google.ca");
                element = driver.FindElement(By.Id("lst-ib"));
                element.SendKeys(keyword);
                element.SendKeys(Keys.Return);
                success = !driver.GetElement("//form[@id='captcha-form']", ref element, "captcha-form");
            }
            catch (Exception) { }
            return success;
        }
        public static Boolean GetElements(this IWebElement elementParent, String xPath, ref ReadOnlyCollection<IWebElement> elements, String key = "", ChromeDriver driver = null, WebDriverWait wait = null)
        {
            IWebElement element = null;
            return elementParent.getElement(false, xPath, ref element, ref elements, key, driver, wait);
        }
        public static Boolean GetElement(this IWebElement elementParent, String xPath, ref IWebElement element, String key = "", ChromeDriver driver = null, WebDriverWait wait = null)
        {
            ReadOnlyCollection<IWebElement> elements = null;
            return elementParent.getElement(true, xPath, ref element, ref elements, key, driver, wait);
        }
        public static Boolean GetElements(this ChromeDriver driver, String xPath, ref ReadOnlyCollection<IWebElement> elements, String key = "", WebDriverWait wait = null)
        {
            IWebElement element = null;
            return driver.getElement(false, xPath, ref element, ref elements, key, wait);
        }
        public static Boolean GetElement(this ChromeDriver driver, String xPath, ref IWebElement element, String key = "", WebDriverWait wait = null)
        {
            ReadOnlyCollection<IWebElement> elements = null;
            return driver.getElement(true, xPath, ref element, ref elements, key, wait);
        }
        private static Boolean getElement(this IWebElement elementParent, Boolean isSingle, String xPath, ref IWebElement element, ref ReadOnlyCollection<IWebElement> elements, String key, ChromeDriver driver, WebDriverWait wait)
        {
            Boolean success = false;
            try
            {
                if (String.IsNullOrEmpty(key) || (driver != null && driver.PageSource.Contains(key)))
                {
                    if (wait == null)
                    {
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        if (isSingle)
                            element = elementParent.FindElement(By.XPath(xPath));
                        else
                            elements = elementParent.FindElements(By.XPath(xPath));
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
                    }
                    else
                    {
                        if (isSingle)
                            element = elementParent.FindElement(By.XPath(xPath));
                        else
                            elements = elementParent.FindElements(By.XPath(xPath));
                    }
                    success = isSingle ? element != null : elements != null && elements.Count > 0;
                }
            }
            catch (NoSuchElementException) { }
            catch (Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }
        private static Boolean getElement(this ChromeDriver driver, Boolean isSingle, String xPath, ref IWebElement element, ref ReadOnlyCollection<IWebElement> elements, String key, WebDriverWait wait)
        {
            Boolean success = false;
            try
            {
                if (String.IsNullOrEmpty(key) || driver.PageSource.Contains(key))
                {
                    if (wait == null)
                    {
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        if (isSingle)
                            element = driver.FindElement(By.XPath(xPath));
                        else
                            elements = driver.FindElements(By.XPath(xPath));
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
                    }
                    else
                    {
                        if (isSingle)
                            element = wait.Until(d => d.FindElement(By.XPath(xPath)));
                        else
                            elements = wait.Until(d => d.FindElements(By.XPath(xPath)));
                    }
                    success = isSingle ? element != null : elements != null && elements.Count > 0;
                }
            }
            catch (NoSuchElementException) { }
            catch (Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }

        public static Boolean Click(this IWebElement element, ChromeDriver driver)
        {
            Boolean success = false;
            StatusExtension.Initialize();
            try
            {
                if (element.Displayed)
                    element.Click();
                else
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                success = true;
            }
            catch (Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }
        public static Boolean SendKeys(this IWebElement element, ChromeDriver driver, String message)
        {
            Boolean success = false;
            StatusExtension.Initialize();
            try
            {
                if (element.Displayed)
                {
                    element.Clear();
                    element.SendKeys(message);
                }
                else
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value=arguments[1];", element, message);
                success = true;
            }
            catch (Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }
        public static Boolean FindElement(this ChromeDriver driver, String xPath, ref IWebElement element)
        {
            Boolean success = false;
            StatusExtension.Initialize();
            try
            {
                element = driver.FindElement(By.XPath(xPath));
                success = element != null;
            }
            catch (Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }
        public static Boolean ClosePopup(this ChromeDriver driver)
        {
            StatusExtension.Initialize();
            Boolean success = false;
            String currentHandle = driver.CurrentWindowHandle;
            foreach (String handle in driver.WindowHandles)
            {
                if (handle != currentHandle)
                {
                    driver.SwitchTo().Window(handle);
                    driver.Close();
                    success = true;
                }
            }
            if (success)
                driver.SwitchTo().Window(currentHandle);
            //else
            //    success = driver.ClickByText("x");
            return success;
        }
        public static Boolean ClickByText(this ChromeDriver driver, String key, String keyToFind = "", Boolean isContained = false)
        {
            String translation = "translate(text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')";
            if (isContained)
                return driver.Click($"//*[contains({translation}, '{key}')]", key, keyToFind);
            else
                return driver.Click($"//*[{translation} = '{key}']", key, keyToFind);
        }
        public static Boolean Click(this ChromeDriver driver, String xPath, String key = "", String keyToFind = "")
        {
            StatusExtension.Initialize();
            Boolean success = false;
            if (String.IsNullOrEmpty(key) || driver.PageSource.ToLower().Contains(key))
            {
                try
                {
                    String currentUrl = driver.Url;
                    IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath(xPath));
                    foreach (IWebElement element in elements)
                    {
                        if (element.Enabled && element.Displayed)
                            element.Click();
                        else
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                        if (String.IsNullOrEmpty(keyToFind) || driver.PageSource.ToLower().Contains(keyToFind))
                        {
                            success = true;
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }
            return success;
        }
        public static Int32 GetGoogleSearchCurrenPage(this ChromeDriver driver)
        {
            Int32 page = 1;
            IWebElement element = null;
            StatusExtension.Initialize();
            if (driver.GetElement("//td[@class='cur']", ref element))
            {
                page = element.Text.Replace("\"", "").ConvertToInt32();
            }
            return page;
        }
        public static Boolean GetGoogleNextPage(this ChromeDriver driver)
        {
            Boolean success = false;
            StatusExtension.Initialize();
            try
            {
                IWebElement element = driver.FindElementById("pnnext");
                element.Click();
                success = true;
            }
            catch (Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }
        public static IWebElement GetElement(this ChromeDriver driver, String xPath)
        {
            StatusExtension.Initialize();
            IWebElement element = null;
            try
            {
                element = driver.FindElement(By.XPath(xPath));
            }
            catch (Exception) { }
            return element;
        }
        public static String GetValue(this ChromeDriver driver, String xPath, String attribute, String defaultValue = "")
        {
            String value = defaultValue;
            try
            {
                IWebElement element = driver.FindElement(By.XPath(xPath));
                if (element != null)
                    value = element.GetValue("", attribute, driver, defaultValue);
            }
            catch (OpenQA.Selenium.NoSuchElementException) { }
            return value;
        }
        public static String GetValue(this IWebElement element, String xPath, String attribute, ChromeDriver driver = null, String defaultValue = "")
        {
            String value = defaultValue;
            if (element != null)
            {
                IWebElement ele = null;
                if (String.IsNullOrEmpty(xPath))
                    ele = element;
                else
                    try { ele = element.FindElement(By.XPath(xPath)); }
                    catch (OpenQA.Selenium.NoSuchElementException) { }
                    catch (Exception) { }
                if (ele != null)
                {
                    if (String.IsNullOrEmpty(attribute))
                    {
                        if (!element.Displayed && driver != null)
                            value = driver.ExecuteScript("return arguments[0].textContent", ele).ToString();
                        else value = ele.Text;
                    }
                    else
                    {
                        value = ele.GetAttribute(attribute);
                    }
                    if (!String.IsNullOrEmpty(value))
                    {
                        value = WebExtension.DecodeHtml(value);
                        value = WebExtension.DecodeUrl(value);
                    }
                }
            }
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }
        #endregion

        public static String GetElementValue(this ChromeDriver driver, String xPath, WebDriverWait wait = null, String attribute = "", String uniqueKey = "")
        {
            StatusExtension.Initialize();
            String value = String.Empty;
            if (String.IsNullOrEmpty(uniqueKey) || driver.PageSource.Contains(uniqueKey))
            {
                IWebElement element = null;
                try
                {
                    if (wait == null)
                        element = driver.FindElement(By.XPath(xPath));
                    else
                        element = wait.Until<IWebElement>(d => d.FindElement(By.XPath(xPath)));
                    if (element != null)
                        value = element.GetValue(attribute);
                }
                catch (Exception ex) { StatusExtension.ErrorMessage = ex.Message; }
            }
            return value;
        }
        public static String GetValue(this IWebElement element, String attribute = "")
        {
            String value = String.Empty;
            if (element != null)
            {
                value = String.IsNullOrEmpty(attribute) ? element.Text : element.GetAttribute(attribute) ?? String.Empty;
                value = WebExtension.DecodeHtml(value);
            }
            return value;
        }
        public static String GetElementValue(this ChromeDriver driver, WebDriverWait wait, String xPath, String attribute = "")
        {
            StatusExtension.Initialize();
            IWebElement element = driver.CheckElement(xPath, 2);
            return getElementValue(element, attribute);
        }

        public static Dictionary<String, String> GetElementValues2(this ChromeDriver driver, String xPath, String xPathKey, String xPathValue, String uniqueKey = "")
        {
            StatusExtension.Initialize();
            Dictionary<String, String> dic = new Dictionary<String, String>();
            try
            {
                if (String.IsNullOrEmpty(uniqueKey) || driver.PageSource.Contains(uniqueKey))
                {
                    IWebElement element = driver.FindElement(By.XPath(xPath));
                    if (element != null)
                    {
                        IWebElement[] elementsKey = element.FindElements(By.XPath(xPathKey)).ToArray<IWebElement>();
                        IWebElement[] elementsValue = element.FindElements(By.XPath(xPathValue)).ToArray<IWebElement>();
                        for (Int32 index = 0; index < Math.Min(elementsKey.Length, elementsValue.Length); ++index)
                            dic[getElementValue(elementsKey[index])] = getElementValue(elementsValue[index]);
                    }
                }
            }
            catch (Exception) { }
            return dic;
        }
        public static Dictionary<String, String> GetElementValues(this ChromeDriver driver, String xPath, String xPathKey, String xPathValue, String attribute = "", String uniqueKey = "")
        {
            StatusExtension.Initialize();
            Dictionary<String, String> dic = new Dictionary<String, String>();
            try
            {
                if (String.IsNullOrEmpty(uniqueKey) || driver.PageSource.Contains(uniqueKey))
                {
                    IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath(xPath));
                    if (elements != null)
                        foreach (IWebElement element in elements)
                            dic[element.GetElementValue(xPathKey, attribute)] = element.GetElementValue(xPathValue, attribute);
                }
            }
            catch (Exception) { }
            return dic;
        }

        public static List<String> GetElementValues(this ChromeDriver driver, String xPath, String xPath2 = "", String attribute = "", String uniqueKey = "")
        {
            StatusExtension.Initialize();
            List<String> list = new List<String>();
            try
            {
                if (!String.IsNullOrEmpty(uniqueKey) && driver.PageSource.Contains(uniqueKey))
                {
                    IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath(xPath));
                    if (elements != null)
                    {
                        foreach (IWebElement element in elements)
                            if (String.IsNullOrEmpty(xPath2))
                                list.Add(element.GetValue(attribute));
                            else
                                list.Add(element.GetElementValue(xPath2, attribute));
                    }
                }
            }
            catch (Exception) { }
            return list;
        }

        public static List<String> GetElementValues(this IWebElement elementParant, String xPath, String xPath2 = "", String attribute = "")
        {
            StatusExtension.Initialize();
            List<String> list = new List<String>();
            try
            {
                IReadOnlyCollection<IWebElement> elements = elementParant.FindElements(By.XPath(xPath));
                if (elements != null)
                {
                    foreach (IWebElement element in elements)
                        if (String.IsNullOrEmpty(xPath2))
                            list.Add(element.GetValue(attribute));
                        else
                            list.Add(element.GetElementValue(xPath2, attribute));
                }
            }
            catch (Exception) { }
            return list;
        }

        public static String GetElementValue(this IWebElement elementParant, ChromeDriver driver, String xPath, String attribute = "", String keyWord = "")
        {
            StatusExtension.Initialize();
            String value = String.Empty;
            if (driver.PageSource.Contains(keyWord))
            {
                value = elementParant.GetElementValue(xPath, attribute);
            }
            return value;
        }

        public static Dictionary<String, String> GetElementDictionaryValues(this IWebElement elementParant, String xPathKey, String xPathValue, Int32 count, String attrKey = "", String attrValue = "")
        {
            String title = String.Empty;
            Dictionary<String, String> dic = new Dictionary<string, string>();
            for (Int32 index = 1; index <= count; ++index)
            {
                title = elementParant.GetElementValue(String.Format(xPathKey, index), attrKey);
                if (!String.IsNullOrEmpty(title))
                    dic[title] = elementParant.GetElementValue(String.Format(xPathValue, index), attrValue);
            }
            return dic;
        }

        public static IWebElement CheckElement(this ChromeDriver driver, String xPath, Int32 checkTimes = 2)
        {
            StatusExtension.Initialize();
            IWebElement element = null;
            Int32 count = 0;
            do
            {
                try
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    element = driver.FindElement(By.XPath(xPath));
                    if (element != null)
                        break;
                    //System.Threading.Thread.Sleep(1000);
                }
                catch (NoSuchElementException ex) { break; }
                catch (NotFoundException ex) { break; }
                catch (Exception ex) { }
            } while (count++ < checkTimes);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            return element;
        }

        public static Boolean ClickElement(this ChromeDriver driver, WebDriverWait wait, String xPath)
        {
            StatusExtension.Initialize();
            Boolean success = false;
            IWebElement element = driver.CheckElement(xPath, 2);
            success = element != null && element.Enabled && element.Displayed;
            if (success)
                element.Click();
            return success;
        }

        public static ChromeDriver GetChromeDriver(Boolean hideBrowser = false, Boolean hideCommand = false, Boolean isMaximized = false, TimeSpan? wait = null)
        {
            ChromeDriver driver = null;
            wait = wait == null ? new TimeSpan(0, 0, 60) : wait;
            try
            {
                ChromeOptions optionsChrome = new ChromeOptions();
                optionsChrome.AddArgument("--disable-popup-blocking");
                optionsChrome.AddArgument("--log-level=3");
                //optionsChrome.AddArgument("--disable-logging");
                //optionsChrome.AddArgument("--disable-infobars");
                //optionsChrome.AddArgument("--disable-default-apps");
                //optionsChrome.AddArgument("--no-first-run");
                //optionsChrome.AddArgument("--no-default-browser-check");
                //optionsChrome.AddArgument("--ignore-gpu-blacklist");
                //optionsChrome.AddArgument("--ignore-certificate-errors");
                optionsChrome.AddArgument("--disable-extensions");
                //optionsChrome.AddArgument("--disable-gpu");
                //optionsChrome.AddArgument("test-type");
                //optionsChrome.AddArgument("no-sandbox");
                //optionsChrome.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                if (hideBrowser)
                    optionsChrome.AddArgument("--headless");//hide browser
                //if (System.IO.Directory.Exists(CHROME_DRIVER_LOCATION))
                //    optionsChrome.BinaryLocation = CHROME_DRIVER_LOCATION;
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = hideCommand;

                System.Environment.SetEnvironmentVariable("webdriver.chrome.driver", @"C:\Alibaba\");
                driver = new ChromeDriver(service, optionsChrome);
                //driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 10, 0);
                if (isMaximized)
                    driver.Manage().Window.Maximize();
                driver.Manage().Timeouts().PageLoad = (TimeSpan)wait; ;
                driver.Manage().Timeouts().ImplicitWait = (TimeSpan)wait; ;

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[Webdriver] ERROR: {0}", ex.Message);
                driver = null;
            }
            return driver;
        }

        /// <summary>
        /// Obsolate
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="sleep"></param>
        /// <returns></returns>
        public static ChromeDriver AlibabaLogin(String email, String password, Int32 sleep = 5000)
        {
            ChromeDriver driver = GetChromeDriver();
            Boolean success = AlibabaLogin(driver, email, password, "", "", sleep);
            return success ? driver : null;
        }

        //public static ChromeDriver AlibabaLogin(this ChromeDriver driver, String email, String password, Int32 sleep = 5000)
        //{
        //    if (driver != null)
        //        try
        //        {
        //            driver.Navigate().GoToUrl("https://login.alibaba.com");
        //            IWebElement element;
        //            IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");
        //            driver.SwitchTo().Frame(0);
        //            element = driver.FindElement(By.XPath("//input[@id='fm-login-id']"));
        //            element.SendKeys(email);
        //            element = driver.FindElement(By.XPath("//input[@id='fm-login-password']"));
        //            element.SendKeys(password);
        //            element = driver.FindElement(By.XPath("//input[@id='fm-login-submit']"));
        //            element.Submit();
        //            System.Threading.Thread.Sleep(sleep);
        //            if (!String.IsNullOrEmpty(urlMessage))
        //            {
        //                driver.Navigate().GoToUrl("https://login.alibaba.com");
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            driver = null;
        //        }
        //    return driver;
        //}
        public static Boolean AlibabaLogin(this ChromeDriver driver, String email, String password, String urlAlibaba = "", String urlMessage = "", Int32 sleep = 5000)
        {
            StatusExtension.Initialize();
            Boolean success = false;
            try
            {
                urlAlibaba = String.IsNullOrEmpty(urlAlibaba) ? "https://login.alibaba.com" : urlAlibaba;
                WebDriverWait wait = new WebDriverWait(driver, _TimeSpanWaiting);
                success = driver.GoToUrl(urlAlibaba);
                Console.WriteLine("Login and then enter.");
                Console.ReadLine();
                //IWebElement element;
                //IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");
                //driver.SwitchTo().Frame(0);
                //element = driver.FindElement(By.XPath("//input[@id='fm-login-id']"));
                //element.SendKeys(email);
                //System.Threading.Thread.Sleep(sleep);
                //element = driver.FindElement(By.XPath("//input[@id='fm-login-password']"));
                //element.SendKeys(password);
                //if (!driver.FindElement(By.XPath("//dd[@id='fm-login-checkcode-wrap']")).Displayed)
                //{
                //    element = driver.FindElement(By.XPath("//input[@id='fm-login-submit']"));
                //    element.Click();
                //    //element.Submit();
                //    success = driver.GetElement("//div[@class='ui-searchbar-main']", ref element, wait: new WebDriverWait(driver, TimeSpan.FromMinutes(1)));  //wait display
                //    if (!driver.GetElement("//div[@id='havana_nco']", ref element))
                //    {
                //        if (driver.GetElement("//div[@id='has-login-field']/form/input", ref element, "has-login-field"))
                //            element.Click();
                //        if (String.IsNullOrEmpty(urlMessage))
                //            success = true;
                //        else
                //        {
                //            success = driver.GoToUrl(urlMessage);
                //            if (driver.PageSource.Contains("ui-feedback ui-feedback-alert"))
                //            {
                //                element = driver.CheckElement("//div[@class='ui-feedback ui-feedback-alert']", 0);
                //                StatusExtension.ErrorMessage = element.Text.DecodeHtml();
                //            }
                //            else
                //                success = true;
                //        }
                //    }
                //    else
                //    {
                //        StatusExtension.ErrorMessage = "Need verification";
                //    }
                //}
                //else
                //    StatusExtension.ErrorMessage = $"Need to check";
            }
            catch (System.Exception ex)
            {
                StatusExtension.ErrorMessage = ex.Message;
            }
            return success;
        }

        public static Boolean GoToUrl(this ChromeDriver driver, String url, Int32 fromMinutes = 1)
        {
            Boolean success = false;
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(fromMinutes);
            try
            {
                driver.Navigate().GoToUrl(url);
                success = true;
            }
            catch (Exception)
            {
            }
            return success;
        }

        public static String GetElementValue(this IWebElement elementParant, String xPath, Int32 index, String attribute = "")
        {
            StatusExtension.Initialize();
            String value = String.Empty;
            IReadOnlyCollection<IWebElement> elements = null;
            Int32 count = 0;
            while (count++ < 10)
            {
                try
                {
                    elements = elementParant.FindElements(By.XPath(xPath));
                    if (elements != null)
                        break;
                    //System.Threading.Thread.Sleep(1000);
                }
                catch (Exception) { }
            }
            if (elements != null && elements.Count > index)
            {
                Int32 i = 0;
                IWebElement element = null;
                foreach (IWebElement ele in elements)
                {
                    if (i++ == index)
                    {
                        element = ele;
                        break;
                    }
                }
                if (element != null)
                    value = getElementValue(element, attribute);
            }
            return value;
        }

        public static String GetElementHiddenValue(this ChromeDriver driver, String xPath, String attribute = "")
        {
            StatusExtension.Initialize();
            IWebElement element = driver.GetElement(xPath);
            return getElementHiddenValue(element, driver, attribute);
        }

        public static String GetElementHiddenValue(this IWebElement elementParant, ChromeDriver driver, String xPath, String attribute = "")
        {
            StatusExtension.Initialize();
            IWebElement element = GetElement(elementParant, xPath);
            return getElementHiddenValue(element, driver, attribute);
        }
        private static String getElementHiddenValue(IWebElement element, ChromeDriver driver, String attribute = "")
        {
            String value = String.Empty;
            if (element != null)
            {
                if (String.IsNullOrEmpty(attribute))
                    if (!element.Displayed)
                        value = driver.ExecuteScript("return arguments[0].textContent", element).ToString();
                    else value = element.Text;
                else
                    value = element.GetAttribute(attribute) ?? String.Empty;
                value = WebExtension.DecodeHtml(value);
            }
            return value;
        }
        private static String getElementValue(IWebElement element, String attribute = "")
        {
            return GetValue(element, attribute);
        }

        public static String GetElementValue(this IWebElement elementParant, String xPath, String attribute = "")
        {
            StatusExtension.Initialize();
            if (String.IsNullOrEmpty(xPath))
                return getElementValue(elementParant, attribute);
            else
            {
                IWebElement element = GetElement(elementParant, xPath);
                return getElementValue(element, attribute);
            }
        }

        public static Boolean CheckElement(ChromeDriver driver, WebDriverWait wait, String xPath, Boolean toBeChecked)
        {
            StatusExtension.Initialize();
            Boolean success = false;
            IWebElement element = GetElement(driver, wait, xPath);
            if (element != null)
            {
                success = toBeChecked != element.Selected;
                if (success)
                    element.Click();
            }
            return success;
        }


        public static IWebElement GetElement(IWebElement elementParant, String xPath)
        {
            StatusExtension.Initialize();
            IWebElement element = null;
            try
            {
                element = elementParant.FindElement(By.XPath(xPath));
                //System.Threading.Thread.Sleep(1000);
            }
            catch (Exception) { }
            return element;
        }

        public static IWebElement GetElement(ChromeDriver driver, WebDriverWait wait, String xPath)
        {
            StatusExtension.Initialize();
            IWebElement element = null;
            try
            {
                element = wait.Until<IWebElement>(d => d.FindElement(By.XPath(xPath)));
                //if (element != null && element.Displayed && element.Enabled)
                //System.Threading.Thread.Sleep(10000);
            }
            catch (Exception) { }
            return element;
        }
    }
}
