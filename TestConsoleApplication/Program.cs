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
using System.IO;
using System.Threading;
using System.Diagnostics;

//using System.Net;
using HtmlAgilityPack;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using System.Reflection;
using System.Web;
//using System.Web.HttpUtility;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
//using System.Windows.Forms;
using System.Net.Mail;
//using EAGetMail;

using Alibaba;
using Utilities;
using AegisImplicitMail;

namespace TestConsoleApplication
{
    class Program
    {
        private static String _ConnectionString = String.Empty;
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            _ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString;

            //Console.Write("AAAA");
            //Console.WriteLine("BBBB");
            //Console.Write("CCCC");
            //Console.WriteLine("DDDD");

            //testClipboard();
            //test();
            //testLoadPage();
            //testContact();
            //testSaveWebpage();
            //testSupplier();
            //testJSON();
            //testReadEmail();
            //pollingEmails();
            //openSupplierWebPages();
            //testWebapgesOpenedToday();
            //pollingBusinessCard();
            //testChromeDriver();
            //savingSuppliers();
            //pollingSuppliers();
            //savingSuppliers();
            //pollingSuppliers();
            //testOutlook365();
            //testSendingMessage();
            //testSerialize();
            //testRandom();
            //testDecode();
            //testDumpWebsite();
            //testUrl();
            //testSQL();
            //testPollingSupplier();
            //testTimeSpan();
            //testDisplay();
            //testLog();
            //testSendEmail();
            //testWebDriver();
            testSendEmailByAegis();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            Console.ReadLine();
        }

        //private static void compEvent(object sender, AsyncCompletedEventArgs e)
        //{
        //    if (e.UserState != null)
        //        Console.Out.WriteLine(e.UserState.ToString());

        //    Console.Out.WriteLine("is it canceled? " + e.Cancelled);

        //    if (e.Error != null)
        //        Console.Out.WriteLine("Error : " + e.Error.Message);
        //}
        private static void testWebDriver()
        {
            ChromeOptions optionsChrome = new ChromeOptions();
            optionsChrome.AddArgument("--disable-popup-blocking");
            optionsChrome.AddArgument("--log-level=3");
            optionsChrome.AddArgument("--disable-extensions");
            //optionsChrome.AddArgument("--disable-logging");
            //optionsChrome.AddArgument("--disable-infobars");
            //optionsChrome.AddArgument("--disable-default-apps");
            //optionsChrome.AddArgument("--no-first-run");
            //optionsChrome.AddArgument("--no-default-browser-check");
            //optionsChrome.AddArgument("--ignore-gpu-blacklist");
            //optionsChrome.AddArgument("--ignore-certificate-errors");
            //optionsChrome.AddArgument("--disable-gpu");
            //optionsChrome.AddArgument("test-type");
            //optionsChrome.AddArgument("no-sandbox");
            //optionsChrome.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;

            System.Environment.SetEnvironmentVariable("webdriver.chrome.driver", @"C:\Alibaba\");
            ChromeDriver driver = new ChromeDriver(service, optionsChrome);
            driver.Navigate().GoToUrl("https://www.google.ca");
            driver.Quit();
            driver = new ChromeDriver(service, optionsChrome);
            driver.Navigate().GoToUrl("https://www.alibaba.com/");


        }

        private static void testSendEmailByAegis()
        {
            try
            {
                var mail = "sales@qway.ca";
                //var host = "smtpout.secureserver.net";
                //var host = "smtpout.europe.secureserver.net";
                var host = "smtp.office365.com";
                var user = "sales@qway.ca";
                var pass = "@Ah630615";

                //Generate Message 
                var message = new MimeMailMessage();
                message.From = new MimeMailAddress(mail);
                message.To.Add("andrew.huang@qway.ca");
                message.Subject = "Subject Text - 008";
                message.Body = "<html><body>Body <b>Text</b>...</body></html>";
                message.IsBodyHtml = true;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                //System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(message.Body, @"<(.|\n)*?>", string.Empty), null, "text/plain");
                //System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");

                //message.AlternateViews.Add(plainView);
                //message.AlternateViews.Add(htmlView);
                //Create Smtp Client
                var mailer = new MimeMailer(host, 587);
                mailer.User = user;
                mailer.Password = pass;
                mailer.SslType = SslMode.Tls;
                
                mailer.AuthenticationMode = AuthenticationType.Base64;

                //Set a delegate function for call back
                mailer.SendCompleted += Mailer_SendCompleted;
                //mailer.SendMailAsync(message);

                mailer.TestConnection(); 
                mailer.Send(message);
            }
            catch (System.Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }
        }

        private static void Mailer_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //if (e.UserState != null)
            //    Console.Out.WriteLine(e.UserState.ToString());

            //Console.Out.WriteLine("is it canceled? " + e.Cancelled);

            if (e.Error != null)
                Console.Out.WriteLine("Error : " + e.Error.Message);
        }

        private static void testSendEmail()
        {
            #region sales@qway.ca
            MailMessage mail = new MailMessage();
            
            mail.From = new System.Net.Mail.MailAddress("sales@qway.ca");
            mail.To.Add("andrewhuang@hotmail.com");
            mail.Subject = "Test Mail - 008";
            mail.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
            mail.IsBodyHtml = true;
            mail.Body = "This is for testing SMTP mail from GMAIL";
            //string body = System.IO.File.ReadAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html");
            //System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(body, @"<(.|\n)*?>", string.Empty), null, "text/plain");
            //System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(body, null, "text/html");

            //mail.AlternateViews.Add(plainView);
            //mail.AlternateViews.Add(htmlView);
            //mail.Attachments.Add(new System.Net.Mail.Attachment(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html"));

            //SmtpClient SmtpServer = new SmtpClient("smtpout.secureserver.net");
            //SmtpClient SmtpServer = new SmtpClient("qway-ca.mail.protection.outlook.com");
            //SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
            System.Web.Mail.MailMessage mailMsg = new System.Web.Mail.MailMessage();
            // ...
            mailMsg.Fields.Add
                        ("http://schemas.microsoft.com/cdo/configuration/smtpusessl",
                             true);

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
 
            //SmtpClient SmtpServer = new SmtpClient("relay-hosting.secureserver.net");
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Port = 587;  //Tried 80, 3535, 25, 465 (SSL)
            //SmtpServer.Port = 25;
            SmtpServer.EnableSsl = true;
            SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
            //SmtpServer.ServicePoint.MaxIdleTime = 1;
            SmtpServer.Timeout = 600000;

            //SmtpServer.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
            SmtpServer.Credentials = new System.Net.NetworkCredential("qway@qway.ca", "@Ah630615");
            //SmtpServer.Credentials = new System.Net.NetworkCredential("andrewhuang@hotmail.com", "aH630615");
            //SmtpServer.Credentials = new System.Net.NetworkCredential("andrew.huang@websan.com", "@Chieh000");
            try
            {
                SmtpServer.Send(mail);
            }
            catch (System.Exception ex)
            {
                while (ex!=null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
                string error = "";
            }
            #endregion


            #region Gamil
            //MailMessage mail = new MailMessage();
            //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            //mail.From = new MailAddress("sales@qway.ca");
            //mail.To.Add("sales@qway.ca");
            //mail.Subject = "Test Mail - 017";
            //mail.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
            //mail.IsBodyHtml = false;
            ////mail.Body = "This is for testing \nSMTP mail from GMAIL";
            ////string body = "This is for testing \nSMTP mail from GMAIL";
            //string body = System.IO.File.ReadAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html");
            //System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(body, @"<(.|\n)*?>", string.Empty), null, "text/plain");
            //System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(body, null, "text/html");

            //mail.AlternateViews.Add(plainView);
            ////mail.AlternateViews.Add(htmlView);
            //mail.Attachments.Add(new System.Net.Mail.Attachment(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html"));
            //SmtpServer.Port = 587;
            //SmtpServer.Credentials = new System.Net.NetworkCredential("qway.inc@gmail.com", "Ah630615");
            //SmtpServer.EnableSsl = true;

            //SmtpServer.Send(mail);

            #endregion

            #region Hotmail
            //MailMessage mail = new MailMessage();

            //mail.From = new MailAddress("sales@qway.ca");
            //mail.To.Add("andrewhuang@hotmail.com");
            //mail.Subject = "Test Mail - 008";
            //mail.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
            //mail.IsBodyHtml = true;
            ////mail.Body = "This is for testing SMTP mail from GMAIL";
            //string body = System.IO.File.ReadAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html");
            //System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(body, @"<(.|\n)*?>", string.Empty), null, "text/plain");
            //System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(body, null, "text/html");


            //SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
            //mail.AlternateViews.Add(plainView);
            //mail.AlternateViews.Add(htmlView);
            //mail.Attachments.Add(new System.Net.Mail.Attachment(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html"));
            //SmtpServer.Port = 587;
            //SmtpServer.UseDefaultCredentials = false;
            //SmtpServer.Credentials = new System.Net.NetworkCredential("andrewhuang@hotmail.com", "aH630615");
            //SmtpServer.EnableSsl = true;

            //SmtpServer.Send(mail);
            #endregion

        }

        private static void testLog()
        {
            String lLogFileName = @"Alibaba.log";
            System.IO.File.AppendAllText(lLogFileName, "AppendAllText 01\n");
            System.IO.File.AppendAllText(lLogFileName, "AppendAllText 02");
        }

        private static void testDisplay()
        {
            Console.Clear();
            Console.Write("Start .... ");
            Int32 origRow = Console.CursorTop;
            Int32 origCol = Console.CursorLeft;
            for (Int32 i = 0; i < 10; ++i)
            {
                Console.SetCursorPosition(origCol, origRow);
                Console.Write($"pege [{i}/10]");
                Thread.Sleep(200);
            }
            Console.WriteLine("Done");
        }

        private static void testTimeSpan()
        {
            List<Int32> list = new List<int>() { 1, 5, 3, 4, 2, 3 };
            Int32 total = list.Count;
            Int32 count = 0;
            long totalTicks = 10;
            foreach (Int32 item in list)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Thread.Sleep(item * 1000);
                ++count;
                stopWatch.Stop();
                totalTicks += stopWatch.Elapsed.Ticks;
                long ticksAverage = totalTicks / count;
                long ticksRemaining = (long)(totalTicks / count) * (total - count);
                TimeSpan tsAverage = new TimeSpan(ticksAverage);
                TimeSpan tsRemaining = new TimeSpan(ticksRemaining);
                TimeSpan timeSpan = new TimeSpan((long)(totalTicks / count) * (total - count));
                Console.WriteLine($"Count:{count}, Total TimeSpan: {new TimeSpan(totalTicks)}, AVE: {tsAverage}, Remaining: {tsRemaining}, [{timeSpan:d\\.hh\\:mm\\:ss}], [{timeSpan}]");
            }
        }

        private static void testChromeDriver()
        {
            String url = @"https://cnguomao.en.alibaba.com/company_profile.html";
            ChromeDriver driver = new ChromeDriver();
            try
            {
                driver.Navigate().GoToUrl(url);
                IWebElement element = driver.FindElement(By.XPath("//div[@class='page-body']"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                String innerhtml = (String)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", element);

            }
            catch (System.Exception ex)
            {
                driver = null;
            }

        }

        private static void testPollingSupplier()
        {

            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: false, hideCommand: false);
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
            String urlA = "http://ahqidian.en.alibaba.com";
            String urlB = "https://10shidu.en.alibaba.com";
            String urlC = "https://riverscompany.trustpass.alibaba.com";    //Contact B

            String urlCompanyA = "/company_profile.html".GetAbsoluteUrl(urlA);
            String urlCompanyB = "/company_profile.html".GetAbsoluteUrl(urlB);
            String urlCompanyC = "/company_profile.html".GetAbsoluteUrl(urlC);
            String urlContactA = "/contactinfo.html".GetAbsoluteUrl(urlA);
            String urlContactB = "/contactinfo.html".GetAbsoluteUrl(urlB);
            String urlContactC = "/contactinfo.html".GetAbsoluteUrl(urlC);


            SupplierInfo supplier = new SupplierInfo(driver, wait, urlContactA, new SupplierCategoryInfo(12345, "XXXXX", 3));
            bool success = supplier.HasError;
        }

        private static void testSQL()
        {
            DataTable dt = SQLExtension.GetDataTableFromCommand(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, "SELECT [CompanyMetadata] FROM [qwi].[AB.SupplierURL] WHERE [Id] = 8956");
            System.IO.File.WriteAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\HTMLPage2.html", dt.Rows[0]["CompanyMetadata"].ToString());
        }

        private static void testUrl()
        {
            String url = "/contactinfo.html?spm=a2700.8304367.topnav.19.13b4124eQxuJ9Y";
            String baseUrl = "https://jandaood.trustpass.alibaba.com/company_profile.html";
            Uri uri = new Uri(new Uri(baseUrl), url);
            String url2 = uri.AbsolutePath;

            Uri myUri = new Uri("https://www.google.ca/search?hl=en-CA&rlz=1C1CHBF_enCA729CA730&biw=1309&bih=891&q=how+can+i+confirm+an+email+was+delivered%3F&sa=X&ved=0ahUKEwjnqLz4webdAhVk5YMKHak4A7g4HhDVAgicASgE");
            string param1 = HttpUtility.ParseQueryString(myUri.Query).Get("ved");
        }

        //private static void testDumpWebsite()
        //{
        //    WebClient client = new WebClient();
        //    String html = client.DownloadString("https://www.ul.com/");

        //    HtmlDocument doc = new HtmlDocument();
        //    doc.LoadHtml(html);
        //    //HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//*[@href,contains('//')]");
        //    HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//*[@href]");
        //    foreach (HtmlNode node in nodeList)
        //    {
        //        HtmlAttribute attr = node.Attributes["href"];
        //        string url = attr.Value;

        //    }

        //}
        private static void testSupplier()
        {
            String flag = String.Empty;
            ChromeDriver driver = Utilities.WebDriverExtension.GetChromeDriver(hideBrowser: false, hideCommand: false);
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 3, 0));
            driver.Navigate().GoToUrl("https://dm-pc.en.alibaba.com/contactinfo.html");
            IWebElement element = driver.CheckElement("//div[@class='main-wrap region region-type-big']");
            if (element == null)
            {
                element = driver.CheckElement("//div[@class='root']/div[@id='bd']");
                if (element != null)
                    flag = "B";
                else
                {
                    flag = String.Empty;
                }
            }
            else
            {
                flag = "A";
            }
        }

        static void testDecode()
        {
            //String html = @"%7B%22gdc%22%3A%7B%22pageType%22%3A9%2C%22backgroundThemeColor%22%3A%22%23333333%22%2C%22bizId%22%3A1804222424%2C%22siteId%22%3A5000099381%2C%22title%22%3A%22Contacts%22%2C%22pageId%22%3A5000373810%2C%22lang%22%3A%22en%22%2C%22pageName%22%3A%22default%22%7D%2C%22mds%22%3A%7B%22assetsPackageName%22%3A%22icbumod%22%2C%22assetsVersion%22%3A%220.0.1%22%2C%22componentId%22%3A88%2C%22componentType%22%3A1%2C%22config%22%3A%7B%22CACHE_TIME%22%3A%221800%22%2C%22IS_RENDER%22%3A%22false%22%2C%22IS_MULTI_END%22%3A%22true%22%2C%22IS_REAL_TIME%22%3A%22true%22%2C%22ONLY_WIRELESS%22%3A%22false%22%7D%2C%22defaultImage%22%3A%22%5C%2F%5C%2Fimg.alicdn.com%5C%2Ftfs%5C%2FTB1zN5XSXXXXXbUXpXXXXXXXXXX-750-376.png%22%2C%22formData%22%3A%7B%22pcBgImg%22%3A%22%5C%2F%5C%2Fsc01.alicdn.com%5C%2Fkf%5C%2FHTB1sKjYgeSSBuNjy0Flq6zBpVXaY.jpg%22%2C%22fontFamily%22%3A%22Roboto%22%2C%22color%22%3A%22%23BCC7D0%22%2C%22isShowDescription%22%3Afalse%2C%22isUseDefaultBg%22%3Afalse%2C%22customNavs%22%3A%5B%5D%2C%22hideBottom%22%3Atrue%2C%22fontSize%22%3A%2232%22%2C%22isShowBgImage%22%3Atrue%2C%22fontStyle%22%3A%22blod%22%2C%22isUseDefaultBgMobile%22%3Afalse%2C%22fontWeight%22%3A%22blod%22%7D%2C%22isEmpty%22%3A%22false%22%2C%22isMultiEnd%22%3A%22true%22%2C%22moduleData%22%3A%7B%22data%22%3A%7B%22companyHasPassAssessment%22%3Afalse%2C%22companyMainMarket%22%3A%7B%22authenticated%22%3Afalse%2C%22descMarkets%22%3A%5B%7B%22name%22%3A%22North America%22%2C%22percentFormat%22%3A%2240.00%25%22%7D%2C%7B%22name%22%3A%22Central America%22%2C%22percentFormat%22%3A%2210.00%25%22%7D%2C%7B%22name%22%3A%22Western Europe%22%2C%22percentFormat%22%3A%2210.00%25%22%7D%2C%7B%22name%22%3A%22Southern Europe%22%2C%22percentFormat%22%3A%2210.00%25%22%7D%2C%7B%22name%22%3A%22Southeast Asia%22%2C%22percentFormat%22%3A%2210.00%25%22%7D%2C%7B%22name%22%3A%22Eastern Europe%22%2C%22percentFormat%22%3A%2210.00%25%22%7D%2C%7B%22name%22%3A%22Eastern Asia%22%2C%22percentFormat%22%3A%225.00%25%22%7D%2C%7B%22name%22%3A%22Northern Europe%22%2C%22percentFormat%22%3A%225.00%25%22%7D%5D%7D%2C%22companyName%22%3A%22Shenzhen Keysun Technology Limited%22%2C%22companyLogoFileUrl%22%3A%22%5C%2F%5C%2Fsc01.alicdn.com%5C%2Fkf%5C%2FHTB1B2d5LXXXXXazXFXX760XFXXXf.png%22%2C%22companyRegisterFullCountry%22%3A%22China %28Mainland%29%22%2C%22aliMemberEncryptId%22%3A%22IDX1DB0iT6JCZvzj_ejW63fOaSsqToFugtO_7YG7_14cfC_WTE6eRCOLE8077In1rahO%22%2C%22accountIsPaidMember%22%3Atrue%2C%22companyHasAssessmentVideo%22%3Afalse%2C%22sesameUnsolvedComplaintNum%22%3A0%2C%22esiteUrls%22%3A%7B%22companyVideoUrl%22%3A%22%5C%2Fcompany_profile%5C%2Fvideo_introdution.html%22%2C%22contactsUrl%22%3A%22%5C%2Fcontactinfo.html%22%2C%22homeUrl%22%3A%22%5C%2F%22%2C%22companyProfileUrl%22%3A%22%5C%2Fcompany_profile.html%22%2C%22companyFeedbackUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ffeedback.html%22%2C%22trustPassProfileAvUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ftrustpass_profile.html%3Fcertification_type%3Dintl_av%22%2C%22trustPassProfileOnsiteUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ftrustpass_profile.html%3Fcertification_type%3Dintl_onsite%22%2C%22transactionLevelUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ftransaction_level.html%22%2C%22tradeCapacityUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ftrade_capacity.html%22%2C%22creditIndexUrl%22%3A%22%5C%2Fcredit.html%22%2C%22trustPassProfileAssessmentUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ftrustpass_profile.html%3Fcertification_type%3Dintl_assessment%22%2C%22tradeHistoryUrl%22%3A%22%5C%2Fcompany_profile%5C%2Ftrade_history.html%22%7D%2C%22productIsMarketGoods%22%3Afalse%2C%22isShowFeedbackBuyerNum%22%3Atrue%2C%22companyEncryptId%22%3A%22IDX1vIsua0mqXmid6sTo25fkZkCoR0ASGYNRU4EAa4nYkrAn9pRd00HeOug79g_7NBTB%22%2C%22menus%22%3A%5B%7B%22name%22%3A%22index%22%2C%22title%22%3A%22Home%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2F%22%7D%2C%7B%22name%22%3A%22products%22%2C%22title%22%3A%22Product Categories%22%2C%22items%22%3A%5B%7B%22name%22%3A805545681%2C%22title%22%3A%22Efficiency Level VI Power Adapter%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-805545681%5C%2FEfficiency_Level_VI_Power_Adapter.html%22%7D%2C%7B%22name%22%3A801743631%2C%22title%22%3A%22UL 1310 Class 2 Power supply%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-801743631%5C%2FUL_1310_Class_2_Power_supply.html%22%7D%2C%7B%22name%22%3A805446876%2C%22title%22%3A%22Christmas Power Supply Unit%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-805446876%5C%2FChristmas_Power_Supply_Unit.html%22%7D%2C%7B%22name%22%3A805431610%2C%22title%22%3A%22Wall Mount Power Adapter%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-805431610%5C%2FWall_Mount_Power_Adapter.html%22%7D%2C%7B%22name%22%3A805544089%2C%22title%22%3A%22Desktop AC DC Adapter%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-805544089%5C%2FDesktop_AC_DC_Adapter.html%22%7D%2C%7B%22name%22%3A803500577%2C%22title%22%3A%22Laptop Adapter%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-803500577%5C%2FLaptop_Adapter.html%22%7D%2C%7B%22name%22%3A801225755%2C%22title%22%3A%22Interchangeable Power Adapter%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-801225755%5C%2FInterchangeable_Power_Adapter.html%22%7D%2C%7B%22name%22%3A221966461%2C%22title%22%3A%22LED Driver%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-221966461%5C%2FLED_Driver.html%22%7D%2C%7B%22name%22%3A806010747%2C%22title%22%3A%22SMPS Switching Power Supply%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-806010747%5C%2FSMPS_Switching_Power_Supply.html%22%7D%2C%7B%22name%22%3A803553979%2C%22title%22%3A%22Power Banks%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductgrouplist-803553979%5C%2FPower_Banks.html%22%7D%2C%7B%22name%22%3A%22See all categories%22%2C%22title%22%3A%22See all categories%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fproductlist.html%22%7D%5D%2C%22url%22%3A%22%5C%2Fproductlist.html%22%7D%2C%7B%22name%22%3A%22companyProfile%22%2C%22title%22%3A%22Company Profile%22%2C%22items%22%3A%5B%7B%22name%22%3A%22Company Overview%22%2C%22title%22%3A%22Company Overview%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fcompany_profile.html%22%7D%2C%7B%22name%22%3A%22Industrial Certification%22%2C%22title%22%3A%22Industrial Certification%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fcompany_profile%5C%2Ftrustpass_profile.html%22%7D%2C%7B%22name%22%3A%22Company Capability%22%2C%22title%22%3A%22Company Capability%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fcompany_profile%5C%2Ftrade_capacity.html%22%7D%2C%7B%22name%22%3A%22Business Performance%22%2C%22title%22%3A%22Business Performance%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fcompany_profile%5C%2Ftrade_history.html%22%7D%5D%2C%22url%22%3A%22%5C%2Fcompany_profile.html%22%7D%2C%7B%22name%22%3A%22contacts%22%2C%22title%22%3A%22Contacts%22%2C%22items%22%3A%5B%5D%2C%22url%22%3A%22%5C%2Fcontactinfo.html%22%7D%5D%2C%22productIsExhibition%22%3Afalse%2C%22companyRegisterCountry%22%3A%22CN%22%2C%22sesameSolvedComplaintNum%22%3A0%2C%22siteType%22%3A%22esite%22%2C%22tradeHalfYear%22%3A%7B%22compId%22%3A218676155%2C%22channel%22%3A%222%22%2C%22ordAmt%22%3A%2220%2C000%2B%22%2C%22ordCnt6m%22%3A11%7D%2C%22sesameReplyAvgTimeRange%22%3A%22%3C24h%22%2C%22companyHasPassAV%22%3Afalse%2C%22sesameFirstReplyRate%22%3A%2294.6%25%22%2C%22companyType%22%3A%22MANUFACTURER%22%2C%22supplierRating%22%3A5%2C%22companyHasPassOnsite%22%3Atrue%2C%22xsrfToken%22%3A%22%22%2C%22baoAccountAmount%22%3A%2242%2C000%22%2C%22currentMenu%22%3A%22contacts%22%2C%22wapSubDomain%22%3A%22keysuntech.m.en.alibaba.com%22%2C%22baoAccountIsDisplayAssurance%22%3Atrue%2C%22companyLocation%22%3A%22Guangdong%2C China %28Mainland%29%22%2C%22companyBusinessType%22%3A%7B%22authenticated%22%3Afalse%2C%22value%22%3A%22Manufacturer%22%7D%2C%22tradeIsView%22%3Atrue%2C%22esiteSubDomain%22%3A%22keysuntech.en.alibaba.com%22%2C%22companyId%22%3A218676155%2C%22companyJoinYears%22%3A%225%22%2C%22companyHasPassBaseVerify%22%3Afalse%2C%22supplierMarketSuccessfulProductCount%22%3A0%2C%22baoAccountCreditLevel%22%3A4%2C%22companyPromotionTag%22%3A%5B%5D%2C%22isShowComplaintsInfo%22%3Atrue%2C%22hasTAOrderYear%22%3Atrue%7D%2C%22hideBottom%22%3Atrue%2C%22definition%22%3A%7B%22title%22%3A%22Shop Sign%22%2C%22gridWidth%22%3A1200%7D%2C%22config%22%3A%7B%22pcBgImg%22%3A%22%5C%2F%5C%2Fsc01.alicdn.com%5C%2Fkf%5C%2FHTB1sKjYgeSSBuNjy0Flq6zBpVXaY.jpg%22%2C%22fontFamily%22%3A%22Roboto%22%2C%22color%22%3A%22%23BCC7D0%22%2C%22isShowDescription%22%3Afalse%2C%22isUseDefaultBg%22%3Afalse%2C%22customNavs%22%3A%5B%5D%2C%22hideBottom%22%3Atrue%2C%22fontSize%22%3A%2232%22%2C%22isShowBgImage%22%3Atrue%2C%22fontStyle%22%3A%22blod%22%2C%22isUseDefaultBgMobile%22%3Afalse%2C%22fontWeight%22%3A%22blod%22%7D%7D%2C%22moduleName%22%3A%22shopSign%22%2C%22moduleNameAlias%22%3A%22icbu-pc-shopSign%22%2C%22moduleTitle%22%3A%22%E5%BA%97%E6%8B%9B%22%2C%22moduleType%22%3A%22component%22%2C%22ownerType%22%3A5%2C%22widgetId%22%3A%225000817616%22%7D%7D";
            String html = @"%7B%22gdc%22%3A%7B%22pageType%22%3A9%2C%22backgroundThemeColor%22%3A%22%23333333%22%2C%22bizId%22%3A1804222424%2C%22siteId%22%3A5000099381%2C%22title%22%3A%22Contacts%22%2C%22pageId%22%3A5000373810%2C%22lang%22%3A%22en%22%2C%22pageName%22%3A%22default%22%7D%2C%22mds%22%3A%7B%22assetsVersion%22%3A%220.0.7%22%2C%22componentId%22%3A87%2C%22componentType%22%3A1%2C%22config%22%3A%7B%22CACHE_TIME%22%3A%221800%22%2C%22IS_RENDER%22%3A%22false%22%2C%22IS_MULTI_END%22%3A%22false%22%2C%22IS_REAL_TIME%22%3A%22false%22%2C%22ONLY_WIRELESS%22%3A%22false%22%7D%2C%22defaultImage%22%3A%22%5C%2F%5C%2Fimg.alicdn.com%5C%2Ftfs%5C%2FTB1zN5XSXXXXXbUXpXXXXXXXXXX-750-376.png%22%2C%22formData%22%3A%7B%7D%2C%22isEmpty%22%3A%22false%22%2C%22isMultiEnd%22%3A%22false%22%2C%22moduleData%22%3A%7B%22data%22%3A%7B%22enAccountId%22%3A%22IDX17NNX_GcZD2kzgs7GqfMWtGZRnnQ00Vw2QIxXd7Q_sx8HP1EKTLu-MAO4YpDydIgb%22%2C%22feedBackDomain%22%3A%222%22%2C%22feedBackObjectEncryptId%22%3A%22IDX1KwDtnc7t1RfuQthQT8BixwS8cUlnPXSUzns_oYXe99tFT5MUii0S7sJD8txFURWZ%22%2C%22feedBackObjectId%22%3A%22218676155%22%2C%22inquiryFor%22%3A%22company%22%7D%2C%22definition%22%3A%7B%22title%22%3A%22Fast Feedback%22%2C%22gridWidth%22%3A1200%7D%7D%2C%22moduleName%22%3A%22fastFeedback%22%2C%22moduleTitle%22%3A%22%E8%AF%A2%E7%9B%98%E7%9B%B4%E9%80%9A%E8%BD%A6%22%2C%22moduleType%22%3A%22component%22%2C%22ownerType%22%3A5%2C%22widgetId%22%3A%225000817628%22%7D%7D";
            string sss = Utilities.WebExtension.DecodeHtml(html);
            string ss = System.Net.WebUtility.UrlDecode(html);
        }
        private static void testRandom()
        {
            Random r = new Random();
            for (int j = 0; j < 10; ++j)
            {
                for (int i = 0; i < 10; ++i)
                {
                    Console.Write("{0},", r.Next(1, 10));
                }
                Console.WriteLine();
            }
        }

        private static void testSerialize()
        {
            DataTable dataTable = new DataTable();
            DataColumn dc = null;
            dc = new DataColumn("Id", typeof(System.Int32));
            dataTable.Columns.Add(dc);
            dc = new DataColumn("Email", typeof(System.String));
            dataTable.Columns.Add(dc);
            dc = new DataColumn("Email2", typeof(System.String));
            dataTable.Columns.Add(dc);
            dc = new DataColumn("LastRunTime", typeof(System.DateTime));
            dataTable.Columns.Add(dc);
            DataRow row0 = dataTable.NewRow();
            row0["Id"] = 123;
            row0["Email"] = "AAAAAAAAAAA";
            row0["Email2"] = "AAAAAAAAAAA222";
            row0["LastRunTime"] = DateTime.Now;
            dataTable.Rows.Add(row0);
            row0 = dataTable.NewRow();
            row0["Id"] = 234;
            row0["Email"] = "BBBBBBBBB";
            row0["Email2"] = "BBBBBBBBB222";
            row0["LastRunTime"] = DateTime.Now;
            dataTable.Rows.Add(row0);
            dataTable.AcceptChanges();

            List<Alibaba.SettingsInfo> list = Utilities.DataExtension.Serialize<SettingsInfo>(dataTable);
            List<Alibaba.SettingsInfo> settings = new List<SettingsInfo>();
            Alibaba.SettingsInfo setting = new SettingsInfo();
            Type type = setting.GetType();
            PropertyInfo[] pInfos = type.GetProperties();

            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    setting = (Alibaba.SettingsInfo)Activator.CreateInstance(typeof(Alibaba.SettingsInfo), new object[] { });
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        object colValue = row[i]; //DataTable 列值
                        for (int j = 0; j < pInfos.Length; j++)
                        {
                            //全部转换为小写的作用是防止数据库列名的大小写和属性的大小写不一致
                            if (dataTable.Columns[i].ColumnName.ToLower() == pInfos[j].Name.ToLower())
                            {
                                PropertyInfo pInfo = type.GetProperty(pInfos[j].Name);  //setting某一属性对象


                                #region 将列值赋给object属性
                                if (colValue != null)
                                {
                                    if (pInfos[j].PropertyType.FullName == "System.String")
                                    {
                                        pInfo.SetValue(setting, Convert.ToString(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Int32")
                                    {
                                        pInfo.SetValue(setting, Convert.ToInt32(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Int64")
                                    {
                                        pInfo.SetValue(setting, Convert.ToInt64(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Single")
                                    {
                                        pInfo.SetValue(setting, Convert.ToSingle(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Double")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDouble(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Decimal")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDecimal(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Char")
                                    {
                                        pInfo.SetValue(setting, Convert.ToChar(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Boolean")
                                    {
                                        pInfo.SetValue(setting, Convert.ToBoolean(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.DateTime")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDateTime(colValue), null);
                                    }
                                    //可空类型
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDateTime(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDateTime(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToInt32(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToInt32(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.Int64, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToInt64(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToInt64(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.Decimal, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDecimal(colValue), null);
                                    }
                                    else if (pInfos[j].PropertyType.FullName == "System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                                    {
                                        pInfo.SetValue(setting, Convert.ToDecimal(colValue), null);
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    pInfo.SetValue(setting, null, null);
                                }
                                #endregion

                                break;
                            }
                        }
                    }
                    settings.Add(setting);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }




        }

        private static void testClipboard()
        {
            String originalMessage = System.IO.File.ReadAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.html");
            String newMessage = originalMessage.Replace("{Name}", "Andrew");
            System.Windows.Forms.Clipboard.SetText(newMessage, System.Windows.Forms.TextDataFormat.Html);

            String returnHtmlText = null;
            if (System.Windows.Forms.Clipboard.ContainsText(System.Windows.Forms.TextDataFormat.Html))
            {
                returnHtmlText = System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.Html);
                String replacementHtmlText = returnHtmlText.Replace("{Name}", "Andrew");
                System.Windows.Forms.Clipboard.SetText(replacementHtmlText, System.Windows.Forms.TextDataFormat.Html);
            }

            System.Windows.Forms.DataFormats.Format formatMessage = System.Windows.Forms.DataFormats.GetFormat("message");

            MessageObject messageObject = (MessageObject)System.Windows.Forms.Clipboard.GetData("message");
            String message = System.Windows.Forms.Clipboard.GetText();
            System.Windows.Forms.IDataObject clipboardObject = System.Windows.Forms.Clipboard.GetDataObject();

            // Converts the IDataObject type to MyNewObject type. 
            MessageObject messageObject1 = (MessageObject)clipboardObject.GetData(formatMessage.Name);
            messageObject.MessageValue = messageObject.MessageValue.Replace("{Name}", "Andrew");
            System.Windows.Forms.Clipboard.SetDataObject(messageObject);

            Console.WriteLine(System.Windows.Forms.Clipboard.GetDataObject().ToString());
        }
        [Serializable]
        public class MessageObject : Object
        {
            private string myValue;

            // Creates a default constructor for the class.
            public MessageObject()
            {
                myValue = "This is the value of the class";
            }

            // Creates a property to retrieve or set the value.
            public string MessageValue
            {
                get
                {
                    return myValue;
                }
                set
                {
                    myValue = value;
                }
            }
        }

        public static ChromeDriver Login(String email, String password)
        {
            ChromeOptions optionsChrome = new ChromeOptions();
            optionsChrome.AddArgument("--log-level=3");
            optionsChrome.AddArgument("--disable-logging");
            ChromeDriver driver = new ChromeDriver(optionsChrome);
            try
            {
                driver.Navigate().GoToUrl("https://login.alibaba.com");
                //WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 30));
                IWebElement element;
                IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");
                driver.SwitchTo().Frame(0);
                element = driver.FindElement(By.XPath("//input[@id='fm-login-id']"));
                element.SendKeys(email);
                element = driver.FindElement(By.XPath("//input[@id='fm-login-password']"));
                element.SendKeys(password);
                element = driver.FindElement(By.XPath("//input[@id='fm-login-submit']"));
                element.Submit();
                Thread.Sleep(2000);
            }
            catch (System.Exception ex)
            {
                driver = null;
            }
            return driver;
        }
        private static void testSendingMessage()
        {
            //String url = @"https://cnsunnyrain.en.alibaba.com/company_profile.html#top-nav-bar";
            //String contact = "Ms. Fanny Feng";
            //ChromeDriver driver = Login("qway.inc.f@hotmail.com", "Ah630615");
            //WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 30));
            //try
            //{
            //    driver.Navigate().GoToUrl(url);
            //    IWebElement element;
            //    element = driver.FindElement(By.XPath("//a[@class='message-send ui-button ui-button-primary']"));
            //    url = element.GetAttribute("href");
            //    driver.Navigate().GoToUrl(url);
            //    String contactName = Utilities.WebDriverExtension.GetElementValue(driver, "//span[@class='company-contact']").Split(' ')[0];
            //    Utilities.WebDriverExtension.CheckElement(driver, @"//input[@id='respond-in-oneday']", false);
            //    Utilities.WebDriverExtension.CheckElement(driver, @"//input[@id='agree-share-bc']", true);
            //    Thread.Sleep(1000);
            //    IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");
            //    driver.SwitchTo().Frame(0);
            //    element = driver.FindElement(By.XPath("//body[@id='tinymce']"));
            //    String originalMessage = System.IO.File.ReadAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Message.txt");
            //    String newMessage = originalMessage.Replace("{Name}", contact);
            //    element.SendKeys(newMessage);
            //    Thread.Sleep(5000);
            //    driver.SwitchTo().DefaultContent();
            //    element = wait.Until(d => d.FindElement(By.XPath("//div[@class='send-item']/input")));
            //    element.Click();
            //    //element.Submit();

            //}
            //catch (System.Exception ex)
            //{

            //    throw;
            //}

        }
        private static void testChromeDriver2()
        {
            String url = @"https://profile.alibaba.com/sent_list.htm?spm=a2700.8443308.b8101.d24003.4aa63e5fMPc54a";
            //WebClient client = new WebClient();
            //String html = client.DownloadString(url);
            //HtmlDocument doc = new HtmlDocument();
            //doc.LoadHtml(html);
            //HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//div[@class='f-icon m-item  ']");

            ChromeOptions optionsChrome = new ChromeOptions();
            optionsChrome.AddArgument("--log-level=3");
            optionsChrome.AddArgument("--disable-logging");

            ChromeDriver driver = new ChromeDriver(optionsChrome);
            try
            {
                driver.Navigate().GoToUrl("https://login.alibaba.com/?return_url=http%3A%2F%2Fprofile.alibaba.com%2Fsent_list.htm");
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 30));
                //Console.WriteLine("Login Alibaba");
                //Console.ReadKey();
                //driver.Navigate().GoToUrl("https://profile.alibaba.com/sent_list.htm?spm=a2700.8443308.b8101.d24003.4aa63e5fMPc54a");
                IWebElement element;
                IList<IWebElement> iFramList = driver.FindElementsByTagName("iframe");

                driver.SwitchTo().Frame(0);
                element = driver.FindElement(By.XPath("//input[@id='fm-login-id']"));
                element.SendKeys("qway.inc.e@hotmail.com");
                element = driver.FindElement(By.XPath("//input[@id='fm-login-password']"));
                element.SendKeys("Ah630615");
                element = driver.FindElement(By.XPath("//input[@id='fm-login-submit']"));
                element.Submit();
                //Send keys to /html/body xpath of iFrame

                element = wait.Until(d => d.FindElement(By.Id("fm-login-id")));
                //element = driver.FindElement(By.XPath("//input[@id='fm-login-id']"));
                element = driver.FindElementById("fm-login-id");
                element = driver.FindElement(By.XPath("//div[@class='ui2-pagination-pages']/span[@class='next disable']"));
                if (element == null)
                {
                    element = driver.FindElement(By.XPath("//div[@class='ui2-pagination-pages']/a[@class='next']"));
                    element.Submit();
                }
                //element = driver.FindElement(By.XPath("//input[@id='fm-login-password']"));
                //element.SendKeys("Ah630615");
                //element = driver.FindElement(By.XPath("//input[@id='fm-login-submit']"));
                //element.Submit();
            }
            catch (System.Exception ex)
            {
                driver = null;
            }

        }

        //static void testOutlook365()
        //{
        //    // Create a folder named "inbox" under current directory
        //    // to save the email retrieved.
        //    string curpath = Directory.GetCurrentDirectory();
        //    string mailbox = String.Format("{0}\\inbox", curpath);

        //    // If the folder is not existed, create it.
        //    if (!Directory.Exists(mailbox))
        //    {
        //        Directory.CreateDirectory(mailbox);
        //    }

        //    // Hotmail/Outlook IMAP4 server is "imap-mail.outlook.com"
        //    //EAGetMail.MailServer oServer = new EAGetMail.MailServer("imap-mail.outlook.com",
        //    //             "qway.inc@hotmail.com", "aH630615", EAGetMail.ServerProtocol.Imap4);
        //    EAGetMail.MailServer oServer = new EAGetMail.MailServer("imap-mail.outlook.com",
        //                 "sales@qway.ca", "@Ah630615", EAGetMail.ServerProtocol.Imap4);

        //    // for office365 account, please use
        //    // MailServer oServer = new MailServer("outlook.office365.com",
        //    //         "yourid@domain", "yourpassword", ServerProtocol.Imap4);

        //    EAGetMail.MailClient oClient = new EAGetMail.MailClient("TryIt");

        //    // Set SSL connection
        //    oServer.SSLConnection = true;

        //    // Set 993 IMAP4 SSL port
        //    oServer.Port = 993;

        //    try
        //    {
        //        oClient.Connect(oServer);
        //        EAGetMail.MailInfo[] infos = oClient.GetMailInfos();
        //        for (int i = 0; i < infos.Length; i++)
        //        {
        //            EAGetMail.MailInfo info = infos[i];
        //            Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
        //                info.Index, info.Size, info.UIDL);

        //            // Download email from Hotmail/MSN IMAP4 server
        //            EAGetMail.Mail oMail = oClient.GetMail(info);

        //            Console.WriteLine("From: {0}", oMail.From.ToString());
        //            Console.WriteLine("Subject: {0}\r\n", oMail.Subject);

        //            // Generate an email file name based on date time.
        //            System.DateTime d = System.DateTime.Now;
        //            System.Globalization.CultureInfo cur = new
        //                System.Globalization.CultureInfo("en-US");
        //            string sdate = d.ToString("yyyyMMddHHmmss", cur);
        //            string fileName = String.Format("{0}\\{1}{2}{3}.eml",
        //                mailbox, sdate, d.Millisecond.ToString("d3"), i);

        //            // Save email to local disk
        //            oMail.SaveAs(fileName, true);

        //            // Mark email as deleted in Hotmail/MSN Live account.
        //            oClient.Delete(info);
        //        }

        //        // Quit and purge emails marked as deleted from Hotmail/MSN Live server.
        //        oClient.Quit();
        //    }
        //    catch (System.Exception ep)
        //    {
        //        Console.WriteLine(ep.Message);
        //    }

        //}
        private static void savingSuppliers()
        {
            BusinessLogic.SavingSuppliers(_ConnectionString, @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Supplier\New");
        }

        private static void pollingBusinessCard()
        {
            //BusinessLogic.PollingBusinessCards(_ConnectionString, @"C:\Users\andre\Documents\Qway\Alibaba\Pages\BusinessCards\CardReceived\New");
        }
        private static void testContact()
        {
            string path_source = @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Contact";

            DirectoryInfo di = new DirectoryInfo(path_source);
            FileInfo[] dir = di.GetFiles("*.html");  //This is the error.
            string txt, fn;

            FileInfo fi = dir[0];
            //foreach (FileInfo fi in dir)
            //{
            fn = Path.GetFileName(fi.FullName);
            TextReader tr = new StreamReader(fi.FullName);
            string markup = tr.ReadToEnd();
            tr.Close();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(markup);
            var myNodes = doc.DocumentNode.SelectNodes("//div[@class='ui2-list-body list-body']/ul/li[@class='ui2-list-item list-item']");
            foreach (HtmlNode node in myNodes)
            {

                //String html = node.InnerHtml;
                //BusinessCard bc = new BusinessCard(path_source, node);

                //Console.WriteLine(bc);
            }
            //}
        }

        //private static void testWebapgesOpenedToday()
        //{
        //    Int32 index = 0;
        //    DataTable dt = Utilities.SQLExtension.GetDataTableFromStoredProcedure(connString: _ConnectionString, storedProcedureName: "[qwi].[AB.GetWebPageLinksOpenedToday]");
        //    //foreach (DataRow row in dt.Rows)
        //    for (int i = 0; i < dt.Rows.Count; ++i)
        //    {
        //        DataRow row = dt.Rows[i];
        //        Supplier supplier = new Supplier(row, "");
        //        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(supplier.ProfileUrl);
        //        request.Credentials = System.Net.CredentialCache.DefaultCredentials;
        //        request.Method = "GET";
        //        HttpWebResponse response = null;
        //        HttpStatusCode code;
        //        try
        //        {
        //            using (response = (HttpWebResponse)request.GetResponse())
        //            {
        //                code = response.StatusCode;
        //                //if (code == HttpStatusCode.OK)
        //                //{
        //                //    Console.WriteLine("[{0}] {1}", ++index, supplier.ProfileUrl);
        //                //}
        //                //else
        //                //{
        //                //    String cc = code.ToString();
        //                //    Console.WriteLine("Status: {0} - {1}", code, supplier.ProfileUrl);
        //                //}
        //            }
        //        }
        //        catch (WebException ex)
        //        {
        //            response = ex.Response as HttpWebResponse;
        //            code = response.StatusCode;
        //        }
        //        Console.WriteLine("[{0}] [{1}]: {2}", ++index, code, supplier.ProfileUrl);

        //    }
        //}

        private static void openSupplierWebPages()
        {
            BusinessLogic.OpenSupplierWebPages(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, 20, "IX");

        }
        private static void test()
        {
            String str1 = @"

                            Jun 4, 2018
                                                                                                &nbsp;From Connections
                                                                                                                ";
            String[] sss = str1.Split(new string[] { "&nbsp;" }, StringSplitOptions.None);

            String fileName1 = @"Yueqing <Hon\gji> Trade Co., Ltd..html";
            String regexSearch1 = new String(Path.GetInvalidFileNameChars());
            Regex r1 = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch1)));
            fileName1 = r1.Replace(fileName1, "");

            char[] invoidFile = System.IO.Path.GetInvalidFileNameChars();
            char[] invoidPath = System.IO.Path.GetInvalidPathChars();
            //String fileFullName = @"C:\Users\andre\Do<cume>nts\Qway\Alibaba\Pages\Supplier\New\Yueqing <Hongji> Trade Co., Ltd..html";
            String fileFullName = @"Yueqing <Hon\gji> Trade Co., Ltd..html";
            Path.GetInvalidFileNameChars().Aggregate(fileFullName, (current, c) => current.Replace(c.ToString(), string.Empty));
            String pathName = Path.GetDirectoryName(fileFullName);
            String fileName = Path.GetFileName(fileFullName);
            String regexSearch = new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            pathName = r.Replace(pathName, "");
            fileName = @"Yueqing <Hon\gji> Trade Co., Ltd..html";
            regexSearch = new String(Path.GetInvalidFileNameChars());
            r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            fileName = r.Replace(fileName, "");

            fileFullName = Path.Combine(pathName, fileName);



            //System.IO.File.CreateText(fileName);

            regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            fileName = r.Replace(fileName, "");
            int i = (int)' ';
            String str = System.Web.HttpUtility.UrlDecode("%7B%22gdc%22%3A%7B%22pageType%22%3A14%2C%22bizId%22%3A-1%2C%22siteId%22%3A5000076005%2C%22title%22%3A%22Contacts%22%2C%22pageId%22%3A5000170026%2C%22lang%22%3A%22en%22%2C%22pageName%22%3A%22default%22%7D%2C%22mds%22%3A%7B%22assetsVersion%22%3A%220.0.9%22%2C%22componentId%22%3A111%2C%22componentType%22%3A1%2C%22config%22%3A%7B%22CACHE_TIME%22%3A%22900%22%2C%22IS_MULTI_END%22%3A%22false%22%2C%22IS_REAL_TIME%22%3A%22true%22%2C%22ONLY_WIRELESS%22%3A%22false%22%7D%2C%22defaultImage%22%3A%22%5C%2F%5C%2Fimg.alicdn.com%5C%2Ftfs%5C%2FTB1zN5XSXXXXXbUXpXXXXXXXXXX-750-376.png%22%2C%22formData%22%3A%7B%7D%2C%22isEmpty%22%3A%22false%22%2C%22isMultiEnd%22%3A%22false%22%2C%22moduleData%22%3A%7B%22data%22%3A%7B%22accountCity%22%3A%22Qing Dao%22%2C%22accountDisplayName%22%3A%22Ms. Elle Yi%22%2C%22accountIsOverSea%22%3Afalse%2C%22accountMobileNo%22%3A%22*%22%2C%22accountAddress%22%3A%22Cheng Yan District%22%2C%22encodeAlitalkId%22%3A%228pctgRBMALOYHFuwy%5C%2Fj7aA%3D%3D%22%2C%22encryptAccountId%22%3A%22IDX1uhv5wjr_wxsRhMtk9pBkH3Xlxd-h4xf8xePRyJjKL6XmD3r-vMk5pzwd5oy61XM5%22%2C%22encryptCompanyId%22%3A%22IDX1ypI3Br7JYCKNdL-X_pF6xg4feiRFbkVJ76WQghY_mYgmzheDAiHLLej0FlrDszwp%22%2C%22accountZip%22%3A%22543002%22%2C%22accountCountry%22%3A%22China %28Mainland%29%22%2C%22umidToken%22%3A%22B%3Ac735945e1e47cfde6c5a40577a54df2f%22%2C%22accountFax%22%3A%22*%22%2C%22accountJobTitle%22%3A%22sales%22%2C%22esiteSubDomain%22%3A%22yeel.fm.alibaba.com%22%2C%22accountProvince%22%3A%22Shandong%22%2C%22companyId%22%3A%2210006262%22%2C%22accountPhone%22%3A%22*%22%2C%22objectDomain%22%3A%222%22%7D%2C%22definition%22%3A%7B%22title%22%3A%22Company Contact%22%2C%22gridWidth%22%3A1200%7D%2C%22config%22%3A%7B%7D%7D%2C%22moduleName%22%3A%22contactPerson%22%2C%22moduleTitle%22%3A%22%E8%81%94%E7%B3%BB%E4%BA%BA%22%2C%22moduleType%22%3A%22component%22%2C%22ownerType%22%3A5%2C%22widgetId%22%3A%225000296075%22%7D%7D");
            str = System.Web.HttpUtility.HtmlDecode("A&amp;B&quot;C&quot;&lt;D&gt;E&apos;F&nbsp;");
        }
        private static void pollingSuppliers()
        {
            //BusinessLogic.PollingSuppliers(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Supplier\New", @"C:\Users\andre\Documents\Qway\Alibaba\Pages\Supplier\Others", true);
        }
        private static void pollingEmails()
        {
            BusinessLogic.PollingEmails(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);
        }
        private static void testReadEmail()
        {
            //ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);

            //Console.WriteLine("*{0}*",(int)' ');
            Application mapiApp = new Application();
            if (Process.GetProcessesByName("OUTLOOK").Count() > 0)
            {
                mapiApp = Marshal.GetActiveObject("Outlook.Application") as Application;
            }

            NameSpace mapiNameSpace = mapiApp.GetNamespace("MAPI");
            mapiNameSpace.Logon("", "", Missing.Value, Missing.Value);
            Folders mapiFolders = mapiNameSpace.Folders;

            foreach (MAPIFolder mapiFolder in mapiFolders)
            {
                Console.WriteLine("***{0}***", mapiFolder.Name);
                if (mapiFolder.Name.ToLower() == "qway.inc@hotmail.com")
                    try
                    {
                        MAPIFolder inboxFolder = mapiFolder.Store.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                        //foreach (MAPIFolder inboxFolder in mapiFolder.Folders)
                        //{
                        Console.WriteLine("Folder Name: {0}", inboxFolder.Name);
                        foreach (Object setting in inboxFolder.Items)
                        {
                            try
                            {
                                MailItem mailItem = setting as MailItem;
                                if (mailItem.SenderEmailAddress == "feedback@service.alibaba.com")
                                {
                                    Console.WriteLine("{0}: {1} - {2} - {3}", mailItem.SentOn, mailItem.SenderEmailAddress, mailItem.To, mailItem.Subject);
                                    String emailId = mailItem.EntryID;
                                    String html = mailItem.HTMLBody;
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(html);
                                    HtmlNodeCollection nodesParent = doc.DocumentNode.SelectNodes("//body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table");
                                    HtmlNodeCollection nodes = nodesParent[3].SelectNodes(".//tbody/tr/td");
                                    String contactName = nodes[1].InnerText.Trim();
                                    String companyName = nodes[2].InnerText.Trim();
                                    //HtmlNode node = nodesParent[5].SelectSingleNode(".//div");
                                    HtmlNode node = nodesParent[5].SelectSingleNode(".//tbody/tr/td");
                                    String body = node.InnerText.Trim();
                                    nodesParent = doc.DocumentNode.SelectNodes("//body/i");
                                    String feedbackid = String.Empty;
                                    if (nodesParent != null)
                                    {
                                        node = nodesParent.Last<HtmlNode>();
                                        feedbackid = node == null ? String.Empty : node.InnerText.Trim().Replace("==", "").Replace("&quot;", "\"");
                                        try
                                        {
                                            JObject json = (JObject)JsonConvert.DeserializeObject(feedbackid);
                                            feedbackid = json["feedbackid"].ToString().Trim();
                                        }
                                        catch (System.Exception ex)
                                        {
                                        }
                                    }
                                    Console.WriteLine("{0}: {1} - {2}\n{3}", feedbackid, contactName, companyName, body);
                                    //break;
                                }

                            }
                            catch (System.Exception ex)
                            {
                            }
                        }
                        //}
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("ERROR: {0}", ex.Message);

                    }
            }

            //MAPIFolder myInbox = mapiNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
            ////myInbox = mapiNameSpace.Folders["qway.inc@hotmail.com"];


            //foreach (Microsoft.Office.Interop.Outlook.MailItem item in myInbox.Items)
            //{
            //    Console.WriteLine("{0}: {1}", item.To, item.Subject);
            //}
            //Microsoft.Office.Interop.Outlook.NameSpace nameSpace = application.GetNamespace("MAPI");
            //nameSpace.Logon("", "", Missing.Value, Missing.Value);

            //inboxFolder = nameSpace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderInbox);
            //Console.WriteLine("Folders: {0}", inboxFolder.Folders.Count);
            //Console.WriteLine("Accounts: {0}", nameSpace.Accounts.Count);
            //Console.WriteLine("Name: {0}", nameSpace.Accounts[1].DisplayName);
            //// my-account@myserver.com is the name of my account
            //// Unsent mails is the name of the folder I wanted to access
            //inboxFolder = nameSpace.Folders["my-account@myserver.com"].Folders["Unsent mails"];

            //foreach (Microsoft.Office.Interop.Outlook.MailItem mailItem in inboxFolder.Items)
            //{
            //    if (mailItem.UnRead) // I only process the mail if unread
            //    {
            //        Console.WriteLine("Accounts: {0}", mailItem.Body);
            //    }
            //}
        }

        private static void testSQL2()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id","1234"),
                new KeyValuePair<String, Object>("@Name","Andrew Huang"),
                new KeyValuePair<String, Object>("@CompanyType",""),
                new KeyValuePair<String, Object>("@Location",""),
                new KeyValuePair<String, Object>("@Tel",""),
                new KeyValuePair<String, Object>("@Mobile",""),
                new KeyValuePair<String, Object>("@Address",""),
                new KeyValuePair<String, Object>("@Zip",""),
                new KeyValuePair<String, Object>("@Province",""),
                new KeyValuePair<String, Object>("@City",""),
                new KeyValuePair<String, Object>("@Website",""),
                new KeyValuePair<String, Object>("@CreditLevel",""),
                new KeyValuePair<String, Object>("@ProfileUrl",""),
                new KeyValuePair<String, Object>("@ContactUrl",""),
                new KeyValuePair<String, Object>("@CompanyUrl",""),
                new KeyValuePair<String, Object>("@CountryCode",""),
                new KeyValuePair<String, Object>("@Country",""),
                new KeyValuePair<String, Object>("@ContactName",""),
                new KeyValuePair<String, Object>("@ContactTitle",""),
                new KeyValuePair<String, Object>("@ContactDepartment","")
            };
            String connString = System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString;
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[qwi].[AB.UpdateSupplier]";
                    cmd.CommandTimeout = 1200;
                    cmd.Parameters.Clear();
                    if (parameters != null)
                        cmd.Parameters.AddRange(getSqlParameters(parameters));
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                catch (System.Exception ex)
                {
                }
            }

        }
        private static SqlParameter[] getSqlParameters(List<KeyValuePair<String, Object>> parameters)
        {
            List<SqlParameter> paraList = new List<SqlParameter>();
            parameters.ForEach(p =>
            {
                paraList.Add(new SqlParameter(p.Key, p.Value));
            });
            return paraList.ToArray<SqlParameter>();
        }

        private static void testJSON()
        {
            string str = "id:2932,sid:3470,mn:'Guangdong Kingwood Electronic Co., Ltd.',pid:50028553";
            JObject os = JObject.Parse(str);
            var j = (JObject)JsonConvert.DeserializeObject(os.ToString());
            str = j["mn"].ToString();
            String json = "{'data':{'companyHasPassAssessment':true,'companyMainMarket':{'authenticated':true,'descMarkets':[{'name':'North America','percentFormat':'50.00%','products':'USB Flash Drive, Power Bank'},{'name':'South America','percentFormat':'10.00%','products':'USB Flash Drive, Power Bank'},{'name':'Eastern Europe','percentFormat':'10.00%','products':'USB Flash Drive, Power Bank'},{'name':'Western Europe','percentFormat':'10.00%','products':'USB Flash Drive, Power Bank'},{'name':'Domestic Market','percentFormat':'10.00%','products':'USB Flash Drive, Power Bank'},{'name':'Southeast Asia','percentFormat':'5.00%','products':'USB Flash Drive, Power Bank'},{'name':'Mid East','percentFormat':'5.00%','products':'USB Flash Drive, Power Bank'}]},'companyName':'Shenzhen Huawanda Electronic Co., Ltd.','companyLogoFileUrl':'//sc02.alicdn.com/kf/HTB1l1F3LXXXXXbIXFXXq6xXFXXXr.jpg','companyRegisterFullCountry':'China (Mainland)','aliMemberEncryptId':'IDX1U4zHWEvTCvWAlTTcKLIwXwLPGQvd45Zyl_siRtL6LHftJ-e_tnRDAnb9_MYunLgj','accountIsPaidMember':true,'companyHasAssessmentVideo':true,'sesameUnsolvedComplaintNum':0,'esiteUrls':{'companyVideoUrl':'/company_profile/video_introdution.html','contactsUrl':'/contactinfo.html','homeUrl':'/','companyProfileUrl':'/company_profile.html','companyFeedbackUrl':'/company_profile/feedback.html','trustPassProfileAvUrl':'/company_profile/trustpass_profile.html?certification_type=intl_av','trustPassProfileOnsiteUrl':'/company_profile/trustpass_profile.html?certification_type=intl_onsite','transactionLevelUrl':'/company_profile/transaction_level.html','tradeCapacityUrl':'/company_profile/trade_capacity.html','creditIndexUrl':'/credit.html','trustPassProfileAssessmentUrl':'/company_profile/trustpass_profile.html?certification_type=intl_assessment','tradeHistoryUrl':'/company_profile/trade_history.html'},'productIsMarketGoods':false,'accountIsVm':null,'isShowFeedbackBuyerNum':true,'companyEncryptId':'IDX1iSIZjbpHkQK2GUio5YIzl5iwLAqwH5ZHg1vPqo2odPiDAajEPm0y4dQIHvJyYp7l','productEncryptId':null,'menus':[{'name':'index','title':'Home','items':[],'url':'/'},{'name':'products','title':'Product Categories','items':[{'name':806571359,'title':'Encrypted U disk','items':[],'url':'/productgrouplist-806571359/Encrypted_U_disk.html'},{'name':805841633,'title':'Usb Flash Drive','items':[],'url':'/productgrouplist-805841633/Usb_Flash_Drive.html'},{'name':803722024,'title':'Type C series','items':[],'url':'/productgrouplist-803722024/Type_C_series.html'},{'name':218569643,'title':'Power bank','items':[],'url':'/productgrouplist-218569643/Power_bank.html'},{'name':805573765,'title':'Facial Steamer','items':[],'url':'/productgrouplist-805573765/Facial_Steamer.html'},{'name':210086779,'title':'Best Selling Products','items':[],'url':'/productgrouplist-210086779/Best_Selling_Products.html'},{'name':'See all categories','title':'See all categories','items':[],'url':'/productlist.html'}],'url':'/productlist.html'},{'name':'companyProfile','title':'Company Profile','items':[{'name':'Company Overview','title':'Company Overview','items':[],'url':'/company_profile.html'},{'name':'Industrial Certification','title':'Industrial Certification','items':[],'url':'/company_profile/trustpass_profile.html'},{'name':'Company Capability','title':'Company Capability','items':[],'url':'/company_profile/trade_capacity.html'},{'name':'Business Performance','title':'Business Performance','items':[],'url':'/company_profile/trade_history.html'}],'url':'/company_profile.html'},{'name':'contacts','title':'Contacts','items':[],'url':'/contactinfo.html'}],'productIsExhibition':false,'companyRegisterCountry':'CN','sesameSolvedComplaintNum':0,'siteType':'esite','tradeHalfYear':{'compId':200070009,'channel':'2','ordAmt':'100,000+','ordAmt6m':null,'ordCnt6m':29},'sesameReplyAvgTimeRange':'<24h','companyHasPassAV':false,'sesameFirstReplyRate':'97.3%','companyType':'MANUFACTURER','supplierRating':6,'companyHasPassOnsite':true,'xsrfToken':'','baoAccountAmount':'159,000','currentMenu':'companyProfile','wapSubDomain':'huawd.m.en.alibaba.com','baoAccountIsDisplayAssurance':true,'companyLocation':'Guangdong, China (Mainland)','companyBusinessType':{'authenticated':false,'value':'Manufacturer'},'tradeIsView':true,'esiteSubDomain':'huawd.en.alibaba.com','companyId':200070009,'companyJoinYears':'10','companyHasPassBaseVerify':false,'supplierMarketSuccessfulProductCount':0,'baoAccountCreditLevel':5,'companyPromotionTag':[],'productOwnMemberAlitalkEncryptId':null,'isShowComplaintsInfo':true,'hasTAOrderYear':true},'hideBottom':false,'definition':{'title':'Shop Sign','gridWidth':1200},'config':{'pcBgImg':'//sc02.alicdn.com/kf/HTB1mEpmobGYBuNjy0Foq6AiBFXa6.jpg','fontFamily':0,'color':'#FFFFFF','isShowDescription':false,'isUseDefaultBg':false,'customNavs':[],'hideBottom':false,'fontSize':'32','isShowBgImage':false,'fontStyle':'blod','isUseDefaultBgMobile':false,'fontWeight':'blod'}}";
            var oo = JsonConvert.DeserializeObject(json);
            //dynamic data = JArray.Parse(json);
            var jobject = (JObject)JsonConvert.DeserializeObject(json);
            String companyName = jobject["data"]["companyName"].ToString();
            //Console.WriteLine(jvalue.Value); // 2098

        }

        private static void testSupplier2()
        {
            string path_source = @"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\Pages\Supplier";

            DirectoryInfo di = new DirectoryInfo(path_source);
            FileInfo[] dir = di.GetFiles("*.html", SearchOption.AllDirectories);


            String connString = System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString;
            DataTable dt = new DataTable();
            Supplier supplier = null;
            String fileName = String.Empty;
            String status = String.Empty;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\Pages\Supplier\Supplier.csv"))
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "[qwi].[AB.UpdateSupplier]";
                        cmd.CommandTimeout = 0;
                        //FileInfo fi = dir[0];
                        foreach (FileInfo fi in dir)
                        {
                            fileName = fi.FullName;
                            //fileName = @"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\Pages\Supplier\Technology\Technology Suppliers - Reliable Technology Suppliers and Manufacturers at Alibaba.com - Page 9.html";
                            TextReader tr = new StreamReader(fileName);
                            string markup = tr.ReadToEnd();
                            tr.Close();
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(markup);
                            HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//div[@class='f-icon m-item  ']");
                            Int32 index = 0;
                            Int32 count = 0;
                            if (nodeList != null)
                            {
                                count = nodeList.Count;
                                Console.WriteLine("[{1}]*****{0}*****[{2}]", fi.Name, count, nodeList == null);
                                foreach (HtmlNode node in nodeList)
                                {

                                    supplier = new Supplier(path_source, fileName, node, true);
                                    if (!supplier.SupplierExists)
                                    {
                                        updateSupplier(conn, cmd, supplier);
                                        Console.WriteLine("{0}/{1} {2}", ++index, count, supplier);
                                        status = "Success";
                                    }
                                    else
                                    {
                                        status = String.IsNullOrEmpty(supplier.Id) ? "Id Empty" : String.Format("[{0}] Exists", supplier.Id);
                                        file.WriteLine(String.Format("{0},{1}/{2},{3}", status, index, count, fileName));
                                    }
                                    //break;
                                }
                            }
                            else
                            {
                                status = "Ignored";
                                Console.WriteLine("Ignored: {0}", fileName);
                            }

                            file.WriteLine(String.Format("{0},{1}/{2},{3}", status, index, count, fileName));
                        }

                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("ERROR in {1}:\n{0}", ex.Message, fileName);
                        file.WriteLine(String.Format("{0},{1},{2}", "Error", ex.Message, fileName));
                    }
                }
            }

        }
        private static void updateSupplier(SqlConnection conn, SqlCommand cmd, Supplier supplier)
        {
            String connString = System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString;
            DataTable dt = new DataTable();
            try
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(getSqlParameters(supplier.GetSQLParameters()));
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR in updateSupplier: {0}", ex.Message);
            }

        }
        //private static void testSaveWebpage()
        //{
        //    WebClient client = new WebClient();
        //    //string downloadString = client.DownloadString("https://huawd.en.alibaba.com/company_profile.html#top-nav-bar");
        //    string downloadString = client.DownloadString("http://huawd.en.alibaba.com/contactinfo.html");
        //}

        private static void testLoadPage()
        {
            string fileName = @"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Test.html";
            TextReader tr = new StreamReader(fileName);
            string markup = tr.ReadToEnd();
            tr.Close();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(markup);
            var myNodes = doc.DocumentNode.SelectNodes("//div[@class='level01']/ul/li[@class='level02']");
            foreach (HtmlNode node in myNodes)
            {
                HtmlNode node1 = node.SelectSingleNode("div[@class='level03']");
                HtmlNode node2 = node1.SelectSingleNode("a[@class='name']");
                Console.WriteLine(node2.InnerText);

            }
        }

    }
}
