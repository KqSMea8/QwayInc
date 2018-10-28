using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
namespace TestGoogle
{
    public static class Google
    {
        internal static void Search()
        {
            List<String> listToSearch = new List<String>() { "Maps", "Maps, ON" };
            IWebElement element = null;
            for (int i = 0; i < 20; ++i)
            {
                ChromeDriver driver = new ChromeDriver();
                driver.Manage().Window.Maximize();
                String keyWord = $"Maps, {i}";
                Console.Write($"[{keyWord}] > ");
                driver.Navigate().GoToUrl("https://www.google.ca");
                element = driver.FindElement(By.Id("lst-ib"));
                element.SendKeys(keyWord);
                element.SendKeys(Keys.Enter);
                if (!getElement(driver, "//form[@id='captcha-form']", ref element))
                    while (true)
                    {
                        if (getElement(driver, "//td[@class='cur']", ref element))
                        {
                            String page = element.Text.Replace("\"", "");
                            Console.Write($"[{page}]");
                            if (getElement(driver, "//table[@id='nav']/tbody/tr/td[last()]/a/span[text()='Next']/..", ref element))
                            {
                                element.Click();
                                Thread.Sleep(6000);
                            }
                            else break;
                        }
                        else break;
                    }
                Console.WriteLine("[Done]");
                driver.Quit();
                Thread.Sleep(60000);
            }
        }
        private static Boolean getElement(ChromeDriver driver, String xPath, ref IWebElement element)
        {
            Boolean success = false;
            try
            {
                element = driver.FindElement(By.XPath(xPath));
                success = true;
            }
            catch (NoSuchElementException) { }
            return success;
        }
    }
}
