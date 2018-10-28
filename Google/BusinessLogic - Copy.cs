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

namespace Google
{
    public static partial class BusinessLogic
    {
        private static WebDriverSettingInfo _WebDriverSetting = new WebDriverSettingInfo(false, false);

        internal static void PollingEmail(string connString, String code)
        {
            Boolean success = false;
            Utilities.Utilities.Log(message: "Sending emails Start ... ", isWriteLine: true, addTime: true);
            EmailSettingInfo emailSetting = DataOperation.GetEmailSetting(connString, code);
            Utilities.Utilities.Log(message: $"[{emailSetting.Code}] [{emailSetting.Name}] ... ", isWriteLine: false, addTime: true);
            MailMessage mailMessage = getMailMessage(emailSetting);
            SmtpClient smtpClient = getsmtpClient();
            String emailBodyOrg = System.IO.File.ReadAllText(emailSetting.FileFullName);
            List<EmailInfo> emails = DataOperation.GetEmails(connString, code);
            Int32 count = (Int32)(emails.Count * emailSetting.SendPercentage / 100);
            Utilities.Utilities.Log(message: $"[{count}/{emails.Count}]", isWriteLine: true);
            foreach (EmailInfo email in emails.Take(count))
            {
                //email.Email = "sales@qway.ca";
                success = false;
                Utilities.Utilities.Log(message: $"Sending [{email.Email}] ... ", isWriteLine: false, addTime: true);
                email.Code = code;
                email.CompanyName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(email.CompanyName.ToLower());
                String emailBody = emailBodyOrg.Replace("{Company}", email.CompanyName).Replace("\r\n", "<br />");
                string[] toEmails = email.Email.Split(';');
                mailMessage.To.Clear();
                foreach (String toEmail in toEmails)
                    try { mailMessage.To.Add(toEmail); }
                    catch (Exception) { }
                System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(emailBody, @"<(.|\n)*?>", string.Empty), null, "text/plain");
                System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(emailBody, null, "text/html");
                mailMessage.AlternateViews.Add(plainView);
                mailMessage.AlternateViews.Add(htmlView);
                try
                {
                    smtpClient.Send(mailMessage);
                    success = true;
                }
                catch (Exception ex)
                {
                    email.Comments = ex.Message;
                }
                success = DataOperation.UpdateEmail(connString, email);
                Utilities.Utilities.Log(message: success ? $"[Done]" : "[X]", isWriteLine: true, addTime: false);
            }
            smtpClient.Dispose();
            mailMessage.Dispose();
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

        private static SmtpClient getsmtpClient()
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Credentials = new System.Net.NetworkCredential("qway.inc@gmail.com", "Ah630615");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            return smtpClient;
        }

        private static Boolean pollingEmails(KeyWordDetailInfo keyWordDetail, ChromeDriver driver, String url)
        {
            Boolean success = false;
            if (pollingEmail(keyWordDetail, driver, url.GetValidUrl(true).GetBaseUrl()))
                if (!String.IsNullOrEmpty(keyWordDetail.Email))
                    success = true;
                else if (pollingEmail(keyWordDetail, driver, url.GetValidUrl(false).GetBaseUrl()))
                    success = !String.IsNullOrEmpty(keyWordDetail.Email);
            return success;
        }
        private static Boolean pollingEmail(KeyWordDetailInfo keyWordDetail, ChromeDriver driver, String url)
        {
            Boolean success = false;
            keyWordDetail.Comments = String.Empty;
            try
            {
                driver.Navigate().GoToUrl(url);
                keyWordDetail.Update(driver);
                if (String.IsNullOrEmpty(keyWordDetail.Email))
                {
                    success = driver.Click("//*[text()='Contact Us']");
                    keyWordDetail.Update(driver);
                }
            }
            catch (Exception ex)
            {
                keyWordDetail.Comments = ex.Message;
            }
            return success;
        }

        internal static void PollingKeyWordDetails(string connString, String code)
        {
            Boolean success = false;
            _WebDriverSetting.HideBrowser = true;
            Utilities.Utilities.Log(message: "Polling Details for key word Start ... ", isWriteLine: false, addTime: true);
            List<KeyWordInfo> keyWrods = DataOperation.GetKeyWords(connString, code);
            Utilities.Utilities.Log(message: $"[{keyWrods.Count}]", isWriteLine: true);
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: _WebDriverSetting.HideBrowser, hideCommand: _WebDriverSetting.HideCommand, isMaximized: true);
            String category = String.Empty;
            foreach (KeyWordInfo keyWord in keyWrods)
            {
                if (category != keyWord.Category)
                {
                    Utilities.Utilities.Log(message: $"[{keyWord.Category}] ... ", isWriteLine: true, addTime: true);
                    category = keyWord.Category;
                }
                Utilities.Utilities.Log(message: $"Polling [{keyWord.KeyWord}] -> ", isWriteLine: false, addTime: true);
                Int32 page = 1;
                if (driver.GetGoogleSearches(keyWord.KeyWord))
                {
                    do
                    {
                        page = driver.GetGoogleSearchCurrenPage();
                        success = pollingKeyWordDetails(connString, driver, keyWord, driver.FindElements(By.XPath("//div[@id='tvcap']/div/ol/li")), true, page);
                        success = pollingKeyWordDetails(connString, driver, keyWord, driver.FindElements(By.XPath("//div[@class='srg']/div/div")), false, page);
                        Utilities.Utilities.Log(message: $"[{page}]", isWriteLine: false, addTime: false);
                    } while (page < keyWord.Pages && driver.GetGoogleNextPage());
                    Utilities.Utilities.Log(message: $"[Done]", isWriteLine: true, addTime: false);
                }
                if (page >= keyWord.Pages)
                {
                    keyWord.IsCompleted = true;
                    success = DataOperation.UpdateKeyWord(connString, keyWord);
                }
            }
            driver.Quit();
        }

        private static bool pollingKeyWordDetails(string connString, ChromeDriver driver, KeyWordInfo keyWord, ReadOnlyCollection<IWebElement> elements, Boolean isAds, Int32 page)
        {
            Boolean success = false;
            if (elements != null)
            {
                foreach (IWebElement element in elements)
                {
                    KeyWordDetailInfo keyWordDetail = new KeyWordDetailInfo(driver, element, keyWord, isAds, page);
                    if (success = !keyWordDetail.HasError)
                    {
                        success = DataOperation.UpdateKeyWordDetail(connString, keyWordDetail);
                    }
                    Utilities.Utilities.Log(message: success ? $"." : "X", isWriteLine: false, addTime: false);
                }
            }
            return success;
        }
    }
}
