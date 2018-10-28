using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using System.Net;

namespace TestWebdriver
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));

            //testOpenChrome();
            //testOpenChromeAlibaba();
            //testOpenTabInChrome();
            testOpenSupplier();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            Console.ReadLine();

        }


        private static void testOpenSupplier()
        {
            String supplier = @"Yueqing Daier Electron Co., Ltd.";
            System.Windows.Forms.Clipboard.SetText(supplier);
            Console.WriteLine("Openning ...");
            ChromeDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.alibaba.com/");
            IWebElement element;
            element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-type']"));
            element.Click();
            element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-type']/div/ul/li/a[@data-value='suppliers']"));
            element.Click();
            element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-body']/form/div[@class='ui-searchbar-main']/input"));
            element.Clear();
            element.SendKeys(supplier);
            element = driver.FindElement(By.XPath("//div[@class='ui-searchbar-body']/form/input[@class='ui-searchbar-submit']"));
            element.Submit();

            WebClient client = new WebClient();
            client.DownloadFile(driver.Url, String.Format(@"C:\Users\andre\Documents\Qway\Alibaba\Pages\Supplier\New\{0}.html", supplier));
            //System.Windows.Forms.Clipboard.SetText("qway.inc@hotmail.com");
            //element = driver.FindElement(By.ClassName("email-input-container"));
            //element = element.FindElement(By.TagName("div"));
            //element = element.FindElement(By.TagName("input"));
            //element.Click();
            //element.SendKeys(Keys.Control + "V");


            //element = driver.FindElement(By.ClassName("send-action"));
            //element = element.FindElement(By.TagName("div"));
            //element = element.FindElement(By.TagName("input"));
            //element.Click();

            Thread.Sleep(10000);
            Console.WriteLine("Closed");
            driver.Close();
        }

        static void testOpenChromeAlibaba()
        {
            //String returnHtmlText = System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.Html);
            //String postString = returnHtmlText.Replace("{@Name}", "Andrew");
            String postString = @"
Hi {@Name},
 
In the end of May, the annual large-scale distributor/retailer talks will be held in Toronto. If your company has plan to become a supplier to the following major Canadian distributors/retailers, the fair is the best opportunity, please contact.
Home Depot, Hudson's Bay‎, Best Buy, Lowe's, Rona, Canadian Tire, Home Hardware etc.
Founded in 2003, Qway Inc. is located in Toronto. It mainly serves the connection between suppliers and Canadian distributors/retailers, including: application, referral, negotiation, translation, reception, market data collection/analysis and Search Engine Marketing(SEM).

[Chinese]
今年五月底，一年一度的大型分销/零售商洽谈会将在多伦多召开，也是成为加拿大供应商的极好时机。如果贵公司有计划成为以下加拿大大型分销/零售商的供应商，请与我们联络。
Home Depot, Hudson's Bay‎, Best Buy, Lowe's, Rona, Canadian Tire, Home Hardware etc.
成立于2003年的Qway Inc. 位于多伦多，具有10+年、多渠道与加拿大零售商洽谈的丰富经验，将贵公司的产品由加拿大分销/零售商销售，包括：申请、引荐、洽谈、翻译、接待、市场数据收集与分析以及搜索引擎营销(SEM)等服务。

Thanks,
Andrew Huang
CEO, Qway Canada. Toronto, ON
Email: qway.inc@hotmail.com
";
            postString = postString.Replace("{@Name}", "Andy");

            System.Windows.Forms.Clipboard.SetText(postString);
            Console.WriteLine("Openning ...");
            //ChromeDriver driver = new ChromeDriver(@"C:\Users\andre\Documents\Qway\Alibaba\Apps\chromedriver.exe");
            ChromeDriver driver = new ChromeDriver();
            //driver.Navigate().GoToUrl("https://message.alibaba.com/message/ma.htm");

            //driver.ExecuteScript("window.open('_blank', 'tab2');");
            //driver.SwitchTo().Window("tab2");

            //driver.SwitchTo().Window(driver.WindowHandles.Last());
            driver.Navigate().GoToUrl("http://www.alibaba.com/member/kingso/company_profile.html#top-nav-bar");
            IWebElement element;
            element = driver.FindElement(By.ClassName("supplier-feedback"));
            element = element.FindElement(By.TagName("a"));
            element.Click();


            element = driver.FindElement(By.XPath("//iframe[@id='inquiry-content_ifr']"));
            element.Click();
            element.SendKeys(Keys.Control + "V");

            try
            {

                element = driver.FindElement(By.Id("respond-in-oneday"));
                if (element.Selected)
                {
                    element.Click();
                }

                element = driver.FindElement(By.Id("agree-share-bc"));
                if (!element.Selected)
                {
                    element.Click();
                }
            }
            catch (Exception ex)
            {

            }
            System.Windows.Forms.Clipboard.SetText("qway.inc@hotmail.com");
            element = driver.FindElement(By.ClassName("email-input-container"));
            element = element.FindElement(By.TagName("div"));
            element = element.FindElement(By.TagName("input"));
            element.Click();
            element.SendKeys(Keys.Control + "V");


            element = driver.FindElement(By.ClassName("send-action"));
            element = element.FindElement(By.TagName("div"));
            element = element.FindElement(By.TagName("input"));
            element.Click();

            //IWebElement element = driver.FindElement(By.ClassName("message-send ui-button ui-button-primary"));
            //element = element.FindElement(By.TagName("a"));
            //element.Click();
            Thread.Sleep(2000);
            Console.WriteLine("Closed");
            driver.Close();
        }
        static void testOpenChrome()
        {
            Console.WriteLine("Openning ...");
            //ChromeDriver driver = new ChromeDriver(@"C:\Users\andre\Documents\Qway\Alibaba\Apps\chromedriver.exe");
            ChromeDriver driver = new ChromeDriver();

            driver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + "t");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            driver.Navigate().GoToUrl("https://www.facebook.com/");

            //driver.SwitchTo().Window(driver.WindowHandles.Last());
            //driver.FindElement(By.LinkText("https://www.facebook.com/")).SendKeys(Keys.Control + "t");

            //Actions action = new Actions(_driver);
            //action.KeyDown(Keys.Control).MoveToElement(body).Click().Perform();

            //driver.Navigate().GoToUrl("https://www.facebook.com/");
            //driver.Url = "https://www.facebook.com/";
            //driver.Manage().Window.Maximize();
            //driver.Manage().Timeouts().ImplicitltWait();
            driver.FindElement(By.Id("email")).SendKeys("andrewhuang@hotmail.com");
            driver.FindElement(By.Id("pass")).SendKeys("aH630615" + Keys.Enter);
            Thread.Sleep(2000);
            Console.WriteLine("Closed");
            driver.Close();
        }

        static void testOpenTabInChrome()
        {
            Console.WriteLine("Openning ...");

            //ChromeDriver driver = new ChromeDriver(@"C:\Users\andre\Documents\Qway\Alibaba\Apps\chromedriver.exe");
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("start-maximized");
            chromeOptions.AddArgument("disable-infobars");
            chromeOptions.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromSeconds(150));
            driver.Navigate().GoToUrl("https://message.alibaba.com/message/ma.htm");

            driver.ExecuteScript("window.open('_blank', 'tab2');");
            driver.SwitchTo().Window("tab2");

            driver.Navigate().GoToUrl("https://kingso.fm.alibaba.com/company_profile.html#top-nav-bar");

            IWebElement element;
            element = driver.FindElement(By.TagName("form"));
            element = element.FindElement(By.Id("fm-login-id"));
            element.SendKeys("qway.inc.d@hotmail.com");
            driver.FindElement(By.Id("fm-login-password")).SendKeys("Ah630615");
            driver.FindElement(By.Id("fm-login-submit")).Click();


            ReadOnlyCollection<string> handles = driver.WindowHandles;
            foreach (string handle in handles)
            {
                driver.SwitchTo().Window(handle);
            }
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.open();");

            //driver.Url = "http://www.alibaba.com/member/kingso/company_profile.html#top-nav-bar";

            //driver.Navigate().GoToUrl("http://www.alibaba.com/member/kingso/company_profile.html#top-nav-bar");

            // open a new tab and set the context
            Thread.Sleep(2000);
            Console.WriteLine("Closed");
            driver.Close();
        }

    }
}
