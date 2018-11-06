/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.09.22
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
using System.Net.Mail;
using Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;
using System.Threading;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Google.Apis.Customsearch.v1.Data;
using AegisImplicitMail;

namespace Google
{
    public static partial class BusinessLogic
    {
        private const String API_KEY = "AIzaSyCwFvHnS7ps107BmBFo4Tr09QFbRbMz7IU";
        private const String SEARCH_ID = "015538826641141501626:59gwbfycqn0";

        private static WebDriverSettingInfo _WebDriverSetting = new WebDriverSettingInfo(false, false);
        #region PollingKeyWordDetails

        internal static void PollingSearchByAPI(string connString, string code)
        {
            Boolean success = false;
            Utilities.Utilities.Log(message: "Polling Google Search from API ... ", isWriteLine: false, addTime: true);
            List<KeyWordInfo> keyWrods = DataOperation.GetKeyWords(connString, code);
            Utilities.Utilities.Log(message: $"[{keyWrods.Count}]", isWriteLine: true);
            String category = String.Empty;
            CustomsearchService customSearchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = API_KEY });
            foreach (KeyWordInfo keyWord in keyWrods)
            {
                if (category != keyWord.Category)
                {
                    Utilities.Utilities.Log(message: $"[{keyWord.Category}] ... ", isWriteLine: true, addTime: true);
                    category = keyWord.Category;
                }
                Utilities.Utilities.Log(message: $"Polling [{keyWord.KeyWordToSearch}] -> ", isWriteLine: false, addTime: true);

                CseResource.ListRequest listRequest = customSearchService.Cse.List(keyWord.KeyWordToSearch);
                listRequest.Cx = SEARCH_ID;
                listRequest.Filter = CseResource.ListRequest.FilterEnum.Value1;
                listRequest.Start = 0;
                listRequest.Num = 10;
                listRequest.LowRange = "100";
                listRequest.HighRange = "200";

                IList<Result> paging = new List<Result>();
                Int32 page = 0;
                Int32 count = 0;
                Int32 countError = 0;
                success = true;
                do
                {
                    listRequest.Start = page * 10 + 1;
                    Utilities.Utilities.Log(message: $"[{++page}]", isWriteLine: false, addTime: false);
                    try
                    {
                        paging = listRequest.Execute().Items;
                        if (paging != null)
                            foreach (Result item in paging)
                                if (item.FileFormat == null)
                                {
                                    ++count;
                                    KeyWordDetailInfo keyWordDetail = new KeyWordDetailInfo(item, keyWord, false, page);
                                    if (success = !keyWordDetail.HasError)
                                        success = DataOperation.UpdateSearchResult(connString, keyWordDetail);
                                    if (!success) ++countError;
                                }
                    }
                    catch (Google.GoogleApiException ex)
                    {
                        Utilities.Utilities.Log(message: $"[Error]\n{ex.Message}", isWriteLine: true, addTime: true, feedLine: true);
                        success = false;
                        break;
                    }
                } while (page < 10 && paging != null);
                if (success)
                {
                    keyWord.Pages = page;
                    success = DataOperation.UpdateSearchHistory(connString, keyWord);
                    Utilities.Utilities.Log(message: $"[Done] [{count}/{countError}]", isWriteLine: true, addTime: false);
                }
            }
        }

        internal static void PollingSearch(string connString, String code)
        {
            _WebDriverSetting.HideBrowser = false;
            Boolean success = false;
            Random random = new Random();
            Int32 sleep = -1;
            Utilities.Utilities.Log(message: "Polling Details for key word Start ... ", isWriteLine: false, addTime: true);
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand, isMaximized: true);
            List<KeyWordInfo> keyWrods = DataOperation.GetKeyWords(connString, code);
            Utilities.Utilities.Log(message: $"[{keyWrods.Count}]", isWriteLine: true);
            String category = String.Empty;
            foreach (KeyWordInfo keyWord in keyWrods)
            {
                if (category != keyWord.Category)
                {
                    Utilities.Utilities.Log(message: $"[{keyWord.Category}] ... ", isWriteLine: true, addTime: true);
                    category = keyWord.Category;
                }
                Utilities.Utilities.Log(message: $"Polling [{keyWord.KeyWordToSearch}] -> ", isWriteLine: false, addTime: true);
                Int32 page = 1;
                if (driver.GetGoogleSearches(keyWord.KeyWordToSearch))
                {
                    do
                    {
                        page = driver.GetGoogleSearchCurrenPage();
                        if (page > 0)
                        {
                            if (page == 1)
                            {
                                ReadOnlyCollection<IWebElement> elements = null;
                                if (driver.GetElements("//div[@class='card-section']/div/p/a", ref elements, "card-section"))
                                    success = pollingVed(connString, driver, keyWord, elements, true, page);
                            }
                            success = pollingKeyWordDetails(connString, driver, keyWord, true, page);
                            success = pollingKeyWordDetails(connString, driver, keyWord, false, page);
                            Utilities.Utilities.Log(message: $"[{page}]", isWriteLine: false, addTime: false);
                            //Thread.Sleep(random.Next(1 * 1000, 5 * 1000));
                        }
                    } while (page > 0 && page < keyWord.Pages && driver.GetGoogleNextPage());
                    keyWord.Pages = page;
                    success = DataOperation.UpdateSearchHistory(connString, keyWord);
                    Utilities.Utilities.Log(message: $"[Done]", isWriteLine: true, addTime: false);
                    //sleep = random.Next(40 * 1000, 80 * 1000);
                }
                else
                {
                    //sleep = random.Next(400 * 1000, 800 * 1000);
                    driver.Quit();
                    Console.WriteLine("Exit Application");
                    Environment.Exit(-100);
                }
                //Thread.Sleep(sleep);
            }
            driver.Quit();
        }

        private static bool pollingKeyWordDetails(String connString, ChromeDriver driver, KeyWordInfo keyWord, Boolean isAds, Int32 page)
        {
            Boolean success = false;
            ReadOnlyCollection<IWebElement> elements = getElements(isAds, driver);
            if (elements != null)
            {
                foreach (IWebElement element in elements)
                {
                    KeyWordDetailInfo keyWordDetail = new KeyWordDetailInfo(driver, element, keyWord, isAds, page);
                    if (success = !keyWordDetail.HasError)
                    {
                        success = DataOperation.UpdateSearchResult(connString, keyWordDetail);
                    }
                    //Utilities.Utilities.Log(message: success ? $"." : "X", isWriteLine: false, addTime: false);
                }
            }
            return success;
        }
        private static ReadOnlyCollection<IWebElement> getElements(Boolean isAds, ChromeDriver driver)
        {
            ReadOnlyCollection<IWebElement> elements = null;
            Dictionary<String, String> xPathList = new Dictionary<String, String>();
            if (isAds)
            {
                xPathList.Add("//div[@id='tvcap']/div/ol/li", "tvcap");
                xPathList.Add("//div[@id='bottomads']/div/ol/li", "bottomads");
            }
            else
            {
                xPathList.Add("//div[@class='srg']/div/div", "srg");
            }
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            foreach (String xpath in xPathList.Keys)
            {
                if (driver.GetElements(xpath, ref elements, xPathList[xpath]))
                    break;
            }
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            String message = String.Empty;
            if (elements == null || elements.Count == 0)
                message = "No";

            return elements;
        }
        private static bool pollingVed(String connString, ChromeDriver driver, KeyWordInfo keyWord, ReadOnlyCollection<IWebElement> elements, Boolean isAds, Int32 page)
        {
            Boolean success = false;
            if (elements != null)
            {
                Dictionary<String, String> veds = new Dictionary<String, String>();
                foreach (IWebElement element in elements)
                {
                    try
                    {
                        Uri uri = new Uri(element.GetValue("", "href"));
                        String ved = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("ved");
                        veds[ved] = element.Text;
                        success = true;
                    }
                    catch (Exception) { }
                }
                success = DataOperation.UpdateVedCodeCrossLink(connString, keyWord, veds);
                Utilities.Utilities.Log(message: success ? $"K" : "O", isWriteLine: false, addTime: false);
            }
            return success;
        }
        #endregion
        internal static void SendingEmail(string connString, String code, String provider)
        {
            Boolean success = false;
            Utilities.Utilities.Log(message: "Sending emails Start ... ", isWriteLine: true, addTime: true);
            List<EmailSettingInfo> emailSettings = DataOperation.GetEmailSetting(connString, code);
            foreach (EmailSettingInfo emailSetting in emailSettings)
            {
                Utilities.Utilities.Log(message: $"[{emailSetting.Code}].[{emailSetting.Name}] ... ", isWriteLine: false, addTime: true);
                String emailBodyOrg = System.IO.File.ReadAllText(emailSetting.FileFullName);
                List<EmailInfo> emails = DataOperation.GetEmails(connString, emailSetting.Code);
                Int32 total = (Int32)(emails.Count * emailSetting.SendPercentage / 100);
                Utilities.Utilities.Log(message: $"[{total}/{emails.Count}]", isWriteLine: true);
                Int32 count = 0;
                foreach (EmailInfo email in emails.Take(total))
                {
                    success = false;
                    //Utilities.Utilities.Log(message: $"Sending [{count}].[{email.CompanyName}] - [{email.Email}] ... ", isWriteLine: false, addTime: true);
                    Utilities.Utilities.Log(message: $"Sending [{count + 1}].[{email.Email}] ... ", isWriteLine: false, addTime: true);
                    List<MailAddress> mailList = getMailAddressList(email.Email);
                    if (mailList.Count > 0)
                    {
#if DEBUG
                        //mailList = new List<MailAddress>() {
                        //    new MailAddress("andrewhuang@hotmail.com"),
                        //    new MailAddress("andrew.huang.2009@gmail.com"),
                        //    new MailAddress("andrew.huang@qway.ca")
                        //};
#endif
                        switch (provider)
                        {
                            case "AIM":
                                success = sendingEmailAIM(emailSetting, email, emailBodyOrg, mailList);
                                break;
                        }
                        if (success)
                        {
                            ++count;
                        }
                    }
                    else
                        email.Comments = "Email list is invalid.";
                    success &= DataOperation.UpdateEmail(connString, email);
                    Utilities.Utilities.Log(message: success ? $"[Sent]" : "[XXXX]", isWriteLine: true, addTime: false, isError: !success);
                    //if (count > total)
                    //    break;
                }
                Utilities.Utilities.Log(message: $"[{count}] sent.", isWriteLine: true, addTime: true);
            }
        }

        private static bool sendingEmailAIM(EmailSettingInfo emailSetting, EmailInfo email, String emailBodyOrg, List<MailAddress> mailList)
        {
            Boolean success = false;
            MimeMailMessage mailMessage = getMimeMailMessage(emailSetting, email, emailBodyOrg, mailList);
            MimeMailer mailer = getMimeMailer();
            System.IO.StreamWriter sw = Utilities.Utilities.CloseConsole();
            try
            {
                mailer.SendMail(mailMessage);
                success = true;
            }
            catch (Exception ex)
            {
                email.Comments = ex.Message;
            }
            Utilities.Utilities.OpenConsole(sw);
            return success;
        }

        private static List<MailAddress> getMailAddressList(String emailList)
        {
            List<MailAddress> list = new List<MailAddress>();
            string[] toEmails = emailList.Split(';');
            foreach (String toEmail in toEmails)
                try
                {
                    MailAddress mailAddress = new System.Net.Mail.MailAddress(toEmail);
                    if (mailAddress.Address == toEmail)
                        list.Add(mailAddress);
                }
                catch (Exception) { }
            return list;
        }

        private static MimeMailer getMimeMailer()
        {
            MimeMailer mailer = new MimeMailer("smtp.office365.com", 587);
            mailer.User = "sales@qway.ca";
            mailer.Password = "@Ah630615";
            mailer.SslType = SslMode.Tls;

            mailer.AuthenticationMode = AuthenticationType.Base64;
            return mailer;

        }

        private static MimeMailMessage getMimeMailMessage(EmailSettingInfo emailSetting, EmailInfo email, String emailBodyOrg, List<MailAddress> mailList)
        {
            String companyName = email.CompanyName.Split('|')[0].Split(':')[0].Trim();
            email.CompanyName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(companyName.ToLower());
            String emailBody = emailBodyOrg.Replace("{Company}", email.CompanyName).Replace("\r\n", "<br />");

            MimeMailMessage mailMessage = new MimeMailMessage();
            mailMessage.From = new MimeMailAddress("sales@qway.ca");
            mailMessage.Subject = emailSetting.Subject;
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = emailBody;

            System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(emailBody, @"<(.|\n)*?>", string.Empty), null, "text/plain");
            System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(emailBody, null, "text/html");
            mailMessage.AlternateViews.Add(plainView);
            mailMessage.AlternateViews.Add(htmlView);

            foreach (MailAddress emailAddress in mailList)
                mailMessage.To.Add(emailAddress);

            return mailMessage;
        }
        private static MailMessage getMailMessage(EmailSettingInfo emailSetting)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("qway.inc@gmail.com");
            mailMessage.Subject = emailSetting.Subject;
            mailMessage.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
            mailMessage.IsBodyHtml = true;
            return mailMessage;
        }

        internal static void SendingEmail(string connString, String code)
        {
            Boolean success = false;
            Utilities.Utilities.Log(message: "Sending emails Start ... ", isWriteLine: true, addTime: true);
            List<EmailSettingInfo> emailSettings = DataOperation.GetEmailSetting(connString, code);
            foreach (EmailSettingInfo emailSetting in emailSettings)
            {
                Utilities.Utilities.Log(message: $"[{emailSetting.Code}].[{emailSetting.Name}] ... ", isWriteLine: false, addTime: true);
                String emailBodyOrg = System.IO.File.ReadAllText(emailSetting.FileFullName);
                List<EmailInfo> emails = DataOperation.GetEmails(connString, code);
                Int32 total = (Int32)(emails.Count * emailSetting.SendPercentage / 100);
                Utilities.Utilities.Log(message: $"[{total}/{emails.Count}]", isWriteLine: true);
                Int32 count = 1;
                foreach (EmailInfo email in emails.Take(total))
                {
                    //email.Email = "sales@qway.ca";
                    MailMessage mailMessage = getMailMessage(emailSetting);
                    SmtpClient smtpClient = getsmtpClient();
                    success = false;
                    Utilities.Utilities.Log(message: $"Sending [{count}].[{email.CompanyName}] - [{email.Email}] ... ", isWriteLine: false, addTime: true);
                    if (getMailMessageTo(ref mailMessage, email.Email))
                    {
                        setBody(email, emailBodyOrg, ref mailMessage);
                        try
                        {
                            smtpClient.Send(mailMessage);
                            ++count;
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            email.Comments = ex.Message;
                        }
                    }
                    else
                        email.Comments = "Email is invalid.";
                    success &= DataOperation.UpdateEmail(connString, email);
                    Utilities.Utilities.Log(message: success ? $"[Sent]" : "[XXXX]", isWriteLine: true, addTime: false);
                    if (count > total)
                        break;
                    smtpClient.Dispose();
                    mailMessage.Dispose();
                    //System.Threading.Thread.Sleep(5000);
                }
            }
        }

        private static void setBody(EmailInfo email, String emailBodyOrg, ref MailMessage mailMessage)
        {
            String companyName = email.CompanyName.Split('|')[0].Split(':')[0].Trim();
            email.CompanyName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(companyName.ToLower());
            String emailBody = emailBodyOrg.Replace("{Company}", email.CompanyName).Replace("\r\n", "<br />");
            System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(emailBody, @"<(.|\n)*?>", string.Empty), null, "text/plain");
            System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(emailBody, null, "text/html");
            mailMessage.AlternateViews.Add(plainView);
            mailMessage.AlternateViews.Add(htmlView);
        }

        private static Boolean getMailMessageTo(ref MailMessage mailMessage, String emailList)
        {
            Boolean success = false;
            string[] toEmails = emailList.Split(';');
            mailMessage.To.Clear();
            foreach (String toEmail in toEmails)
                try
                {
                    MailAddress mailAddress = new System.Net.Mail.MailAddress(toEmail);
                    if (mailAddress.Address == toEmail)
                    {
                        mailMessage.To.Add(toEmail);
                        success = true;
                    }
                }
                catch (Exception) { }
            return success;
        }

        private static SmtpClient getsmtpClient()
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Credentials = new System.Net.NetworkCredential("qway.inc@gmail.com", "Ah630615");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            return smtpClient;
        }

        internal static void PollingEmail(string connString)
        {
            _WebDriverSetting.HideBrowser = false;
            Boolean success = false;
            Utilities.Utilities.Log(message: "Polling Eamil from result Start ... ", isWriteLine: false, addTime: true);
            List<KeyWordDetailInfo> keyWrodDetails = DataOperation.GetKeyWordDetails(connString);
            Utilities.Utilities.Log(message: $"[{keyWrodDetails.Count}]", isWriteLine: true);
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand, isMaximized: true, wait: new TimeSpan(0, 0, 30));
            Int32 count = 0;
            Int32 index = 0;
            String message = String.Empty;
            foreach (KeyWordDetailInfo keyWordDetail in keyWrodDetails)
            {
                //keyWordDetail.Website = "https://www.castertown.com/"; //Popup window
                //keyWordDetail.Website = "http://careers.pharmaprix.ca/"; //Contain 'Contact'
                //keyWordDetail.Website = "http://souhaitenombre.tk/?number=877-617-0472"; //Alert
                success = false;
                Utilities.Utilities.Log(message: $"[{++index}].[{keyWordDetail.Website}] -> ", isWriteLine: false, addTime: true);
                //if (!pollingEmails(keyWordDetail, driver, keyWordDetail.Website))
                //if (!keyWordDetail.Url.ToLower().Contains("google"))
                //    pollingEmails(keyWordDetail, driver, keyWordDetail.Url);
                pollingEmails(keyWordDetail, driver, keyWordDetail.Website);
                if (String.IsNullOrEmpty(keyWordDetail.Email) && String.IsNullOrEmpty(keyWordDetail.Comments))
                    keyWordDetail.Comments = "Email not found";
                if (DataOperation.UpdateKeyWordDetailAfter(connString, keyWordDetail))
                {
                    success = String.IsNullOrEmpty(keyWordDetail.Comments);
                    if (success)
                        ++count;
                }
                Utilities.Utilities.Log(message: success ? $"[Done]" : "[XXXX]", isError: !success, isWriteLine: true, addTime: false);
            }
            Utilities.Utilities.Log(message: $"[{count}] emails found.", isWriteLine: true, addTime: true);
            driver.Quit();
            Console.Error.WriteLine("END");
        }
        private static Boolean pollingEmails(KeyWordDetailInfo keyWordDetail, ChromeDriver driver, String url)
        {
            Boolean success = false;
            //url = url.GetValidUrl(true).GetBaseUrl();
            Utilities.Utilities.Log(message: "[1]", isWriteLine: false, addTime: false);
            if (url.IsWebSiteAvailable())
            {
                if (pollingEmail(keyWordDetail, driver, url))
                    if (!String.IsNullOrEmpty(keyWordDetail.Email))
                        success = true;
                    else if (pollingEmail(keyWordDetail, driver, url.GetValidUrl(false).GetBaseUrl()))
                        success = !String.IsNullOrEmpty(keyWordDetail.Email);
            }
            else
                keyWordDetail.Comments = StatusExtension.ErrorMessage;
            return success;
        }
        private static Boolean pollingEmail(KeyWordDetailInfo keyWordDetail, ChromeDriver driver, String url)
        {
            Boolean success = false;
            keyWordDetail.Comments = String.Empty;
            try
            {
                try
                {
                    Utilities.Utilities.Log(message: "[2]", isWriteLine: false, addTime: false);
                    driver.Navigate().GoToUrl(url);
                    Utilities.Utilities.Log(message: "[3]", isWriteLine: false, addTime: false);
                }
                catch (WebDriverTimeoutException)
                {
                    //driver.FindElement(By.TagName("body")).SendKeys("Keys.ESCAPE");
                    driver.ExecuteScript("window.stop();");
                    //((IJavaScriptExecutor)driver).ExecuteScript("window.stop();");
                }
                KeyWordDetailInfo.TimeoutCount = 0;
                if (!preventPopup(ref driver, keyWordDetail))
                {
                    driver.ClosePopup();
                    keyWordDetail.Update(driver);
                    Utilities.Utilities.Log(message: "[4]", isWriteLine: false, addTime: false);
                    if (click(driver))
                    {
                        Utilities.Utilities.Log(message: "[5]", isWriteLine: false, addTime: false);
                        if (!preventPopup(ref driver, keyWordDetail))
                            keyWordDetail.Update(driver);
                        Utilities.Utilities.Log(message: "[6]", isWriteLine: false, addTime: false);
                    }
                    success = !String.IsNullOrEmpty(keyWordDetail.Email);
                }
            }
            catch (Exception ex)
            {
                ++KeyWordDetailInfo.TimeoutCount;
                //driver.Navigate().GoToUrl("about:blank");
                //restartDriver(ref driver);
                keyWordDetail.Comments = $"[Restart]{ex.Message}";
                Utilities.Utilities.Log(message: "[X1]", isError: true, isWriteLine: false, addTime: false);
                if (KeyWordDetailInfo.TimeoutCount >= 2)
                {
                    driver.Quit();
                    Console.Error.WriteLine("STOP");
                    Environment.Exit(-100);
                }
            }
            return success;
        }
        private static Boolean preventPopup(ref ChromeDriver driver, KeyWordDetailInfo keyWordDetail)
        {
            Boolean success = false;
            if (success = driver.CheckAlert())
            {
                restartDriver(ref driver);
                keyWordDetail.Comments = "new Popup";
                Utilities.Utilities.Log(message: "[X2]", isError: true, isWriteLine: false, addTime: false);
            }
            return success;
        }
        private static void restartDriver(ref ChromeDriver driver)
        {
            //driver.Dispose();
            driver.Quit();
            driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand, isMaximized: true, wait: new TimeSpan(0, 0, 30));
        }
        private static Boolean click(ChromeDriver driver)
        {
            Boolean success = false;
            foreach (String key in KeyWordDetailInfo.ClickKeys.Keys)
            {
                Utilities.Utilities.Log(message: ".", isWriteLine: false, addTime: false);
                if (driver.ClickByText(key, "@", KeyWordDetailInfo.ClickKeys[key]))
                {
                    success = true;
                    break;
                }
            }
            return success;
        }
        //internal static void PollingKeyWordDetails2(string connString, String code)
        //{
        //    Boolean success = false;
        //    _WebDriverSetting.HideBrowser = false;
        //    Utilities.Utilities.Log(message: "Polling Details for key word Start ... ", isWriteLine: false, addTime: true);
        //    List<KeyWordInfo> keyWrods = DataOperation.GetKeyWords(connString, code);
        //    Utilities.Utilities.Log(message: $"[{keyWrods.Count}]", isWriteLine: true);
        //    ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand, isMaximized: true);
        //    String category = String.Empty;
        //    foreach (KeyWordInfo keyWord in keyWrods)
        //    {
        //        if (category != keyWord.Category)
        //        {
        //            Utilities.Utilities.Log(message: $"[{keyWord.Category}] ... ", isWriteLine: true, addTime: true);
        //            category = keyWord.Category;
        //        }
        //        Utilities.Utilities.Log(message: $"Polling [{keyWord.KeyWord}] -> ", isWriteLine: false, addTime: true);
        //        Int32 page = 1;
        //        if (driver.GetGoogleSearches(keyWord.KeyWordToSearch))
        //        {
        //            do
        //            {
        //                page = driver.GetGoogleSearchCurrenPage();
        //                if (page == 1)
        //                    success = pollingVed(connString, driver, keyWord, driver.FindElements(By.XPath("//div[@class='card-section']/div/p/a")), true, page);
        //                success = pollingKeyWordDetails(connString, driver, keyWord, driver.FindElements(By.XPath("//div[@id='tvcap']/div/ol/li")), true, page);
        //                success = pollingKeyWordDetails(connString, driver, keyWord, driver.FindElements(By.XPath("//div[@class='srg']/div/div")), false, page);
        //                Utilities.Utilities.Log(message: $"[{page}]", isWriteLine: false, addTime: false);
        //            } while (page < keyWord.Pages && driver.GetGoogleNextPage());
        //            Utilities.Utilities.Log(message: $"[Done]", isWriteLine: true, addTime: false);
        //        }
        //        if (page >= keyWord.Pages)
        //        {
        //            keyWord.IsCompleted = true;
        //            success = DataOperation.UpdateKeyWord(connString, keyWord);
        //        }
        //    }
        //    driver.Quit();
        //}

    }
}
