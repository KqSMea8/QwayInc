using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Data;

using Microsoft.Win32;
using System.Security.Cryptography;
using System.Xml;
using System.Net.Mail;

//using WIPLibrary;
//using TestConsoleApplication.SRCorrigo;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
//using Starksoft.Cryptography.OpenPGP;
//using Starksoft.Aspen.GnuPG;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//using EAGetMail;

using System.Text.RegularExpressions;
//using mshtml;
//using HtmlAgilityPack;
using System.Reflection;
using System.Runtime.CompilerServices;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Google.Apis.Customsearch.v1.Data;

namespace TestGoogle
{
    class Program
    {
        static void Main(string[] args)
        {

            #region Start info
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            String str = new String('=', 5);
            Console.WriteLine($"{str}{DateTime.Now}{str}");
            #endregion

            testAPI();
            //testBigQuery();

            //Google.Search();

            #region End Info
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine($"{str}{DateTime.Now}{str}");
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            Console.ReadLine();
            #endregion
        }


        private static void testAPI()
        {
            const string apiKey = "AIzaSyCwFvHnS7ps107BmBFo4Tr09QFbRbMz7IU";
            const string searchEngineId = "015538826641141501626:59gwbfycqn0";
            const string query = "\"bike equipment\", distributor, Canada";
            CustomsearchService customSearchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = apiKey });

            CseResource.ListRequest listRequest = customSearchService.Cse.List(query);
            listRequest.Cx = searchEngineId;
            listRequest.Filter = CseResource.ListRequest.FilterEnum.Value1;
            listRequest.HighRange = "";
            Console.WriteLine("Start...");


            IList<Result> paging = new List<Result>();
            var count = 0;
            var index = 0;
            while (paging != null)
            {
                Console.WriteLine($"Page [{count}]");
                listRequest.Start = count * 10 + 1;
                paging = listRequest.Execute().Items;
                if (paging != null)
                    foreach (Result item in paging)
                        if (item.FileFormat == null)
                            Console.WriteLine($"\t[{++index}] Title : {item.Link}");
                count++;
            }
        }
    }
}
