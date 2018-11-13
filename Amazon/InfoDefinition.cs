/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.09.15
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
using System.Data;
using System.Drawing;
using System.Net;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Utilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//using Microsoft.Office.Interop.Outlook;
//using EAGetMail;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Amazon
{
    public class ProductInfo
    {
        private String _Price = String.Empty;

        public Int32 Id { get; set; } = -1;
        public String CategoryId { get; set; } = String.Empty;
        public String ProductId { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public Decimal Price { get; set; } = 0.0m;
        public Decimal ReviewStar { get; set; } = 0.0m;
        public String Shipment { get; set; } = String.Empty;
        public String SellerName { get; set; } = String.Empty;
        public String Review { get; set; } = String.Empty;
        public Int32 Page { get; set; } = 0;
        public Int32 Index { get; set; } = 0;
        public Int32 ReviewCount { get; set; } = 0;
        public String Comments { get; set; } = String.Empty;
        public String Prime { get; set; } = String.Empty;
        public String QId { get; set; } = String.Empty;
        public String Currency { get; set; } = String.Empty;
        public String RNId { get; set; } = String.Empty;
        public String KeyWords { get; set; } = String.Empty;
        public List<String> RHIds { get; set; } = new List<String>();
        public DateTime AddedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime UpdateDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime CompletedDate { get; set; } = new DateTime(1753, 1, 1);
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public ProductInfo() { }
        public ProductInfo(ChromeDriver driver, IWebElement element, Int32 page, Int32 index, Boolean fastMode = false) : this()
        {
            this.Page = page;
            this.Index = index;
            this.ProductId = element.GetValue(driver: driver, attribute: "data-asin");
            this.Url = element.GetValue(xPath: ".//div/div[@class='a-row a-spacing-mini']/div/a", attribute: "href", driver: driver);
            this.Name = element.GetValue(xPath: ".//div/div[@class='a-row a-spacing-mini']/div/a/h2", attribute: "", driver: driver);
            if (String.IsNullOrEmpty(this.Name))
            {
                this.Name = element.GetValue(xPath: ".//div/div/div/div/div/div[@class='a-row a-spacing-none']/a/h2", attribute: "", driver: driver);
                this.SellerName = element.GetValue(xPath: ".//div/div/div/div/div/div[@class='a-row a-spacing-none']/span[2]", attribute: "", driver: driver);
                this._Price = element.GetValue(xPath: ".//div/div/div/div/div/div[@class='a-column a-span7']/div/div/a/span", attribute: "", driver: driver);
                this.Prime = element.GetValue(xPath: ".//div/div/div/div/div/div[@class='a-column a-span7']/div/div/i", attribute: "aria-label", driver: driver);
                this.Shipment = element.GetValue(xPath: ".//div/div/div/div/div/div[@class='a-column a-span7']/div/div[@class='a-row a-spacing-none']/span", attribute: "", driver: driver);
                this.Review = element.GetValue(xPath: $".//div/div/div/div/div/div/div/span[@name='{this.ProductId}']/span/span/a/i/span", attribute: "", driver: driver); ;
                this.ReviewCount = element.GetValue(xPath: $".//div/div/span[@name='{this.ProductId}']/following-sibling::a", attribute: "", driver: driver).ConvertToInt32(0);
            }
            else
            {
                this.SellerName = element.GetValue(xPath: ".//div/div[@class='a-row a-spacing-mini']/div/span[2]", attribute: "", driver: driver);
                this._Price = element.GetValue(xPath: ".//div/div[@class='a-row a-spacing-mini'][2]/div/a/span[2]", attribute: "", driver: driver);
                if (!fastMode)
                {
                    this.Prime = element.GetValue(xPath: ".//div/div[@class='a-row a-spacing-mini'][2]/div/i/span", attribute: "", driver: driver);
                    this.Shipment = element.GetValue(xPath: ".//div/div[last()-1]/div/span", attribute: "", driver: driver);
                }
                this.Review = element.GetValue(xPath: $".//div/div/span[@name='{this.ProductId}']/span/a/i/span", attribute: "", driver: driver);
                this.ReviewCount = element.GetValue(xPath: $".//div/div/span[@name='{this.ProductId}']/following-sibling::a", attribute: "", driver: driver).ConvertToInt32(0);
            }
            setProperties();
        }

        private void setProperties()
        {
            setPropertiesForId();
            setPropertiesForPrice();
            setPropertiesForReviewStar();
        }
        private void setPropertiesForReviewStar()
        {
            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(this.Review, @"[0-9\.]+");
            if (matches.Count > 0)
                this.ReviewStar = Convert.ToDecimal(matches[0].Value);
        }

        private void setPropertiesForPrice()
        {
            string[] arr = this._Price.Split(' ');
            if (arr.Length > 1)
            {
                this.Currency = arr[0];
                this.Price = Convert.ToDecimal(arr[1]);
            }
        }

        private void setPropertiesForId()
        {
            try
            {

                Uri uri = new Uri(this.Url);
                this.KeyWords = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("keywords");
                this.RNId = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("rnid");
                this.QId = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("qid");

                String value = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("rh");
                if (!String.IsNullOrEmpty(value))
                {
                    string[] rhs = value.Split(',');
                    foreach (String rh in rhs)
                    {
                        string[] strs = rh.Split(':');
                        if (strs[0] == "n")
                            this.RHIds.Add(strs[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Comments = ex.Message;
            }
        }

        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>()
            {
                new KeyValuePair<String, Object>("@CategoryId",this.CategoryId),
                new KeyValuePair<String, Object>("@Comments",this.Comments),
                new KeyValuePair<String, Object>("@Currency",this.Currency),
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Page",this.Page),
                new KeyValuePair<String, Object>("@Index",this.Index),
                new KeyValuePair<String, Object>("@KeyWords",this.KeyWords),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Price",this.Price),
                new KeyValuePair<String, Object>("@Prime",this.Prime),
                new KeyValuePair<String, Object>("@ProductId",this.ProductId),
                new KeyValuePair<String, Object>("@QId",this.QId),
                new KeyValuePair<String, Object>("@Review",this.Review),
                new KeyValuePair<String, Object>("@ReviewCount",this.ReviewCount),
                new KeyValuePair<String, Object>("@ReviewStar",this.ReviewStar),
                new KeyValuePair<String, Object>("@RHIds0",this.RHIds.Count>0?this.RHIds[0]:""),
                new KeyValuePair<String, Object>("@RHIds1",this.RHIds.Count>1?this.RHIds[1]:""),
                new KeyValuePair<String, Object>("@RHIds2",this.RHIds.Count>2?this.RHIds[2]:""),
                new KeyValuePair<String, Object>("@RHIds3",this.RHIds.Count>3?this.RHIds[3]:""),
                new KeyValuePair<String, Object>("@RNId",this.RNId),
                new KeyValuePair<String, Object>("@SellerName",this.SellerName),
                new KeyValuePair<String, Object>("@Shipment",this.Shipment),
                new KeyValuePair<String, Object>("@Url",this.Url)
            };
            return parameters;
        }
    }
    public class ProductToSearchInfo
    {
        public Int32 Id { get; set; } = -1;
        public String CategoryId { get; set; } = String.Empty;
        public String ProductName { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public DateTime AddedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime UpdateDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime CompletedDate { get; set; } = new DateTime(1753, 1, 1);
        public List<KeyValuePair<String, Object>> SQLParametersStatus { get { return getSQLParametersStatus(); } }
        public ProductToSearchInfo() { }
        private List<KeyValuePair<String, Object>> getSQLParametersStatus()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>()
            {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Comments",this.Comments),
            };
            return parameters;
        }
    }
    public class SellerInfo
    {
        private List<Int32> _IdLengths = new List<Int32>() { 14, 13, 21, 12, 20 };
        public Int32 Id { get; set; } = -1;
        public String SellerId { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public Int32 SaleItem { get; set; } = -1;
        public String Url { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public String SoldBy { get; set; } = String.Empty;
        public String CategoryId { get; set; } = String.Empty;
        public String Category { get; set; } = String.Empty;
        public Int32 Sold30Days { get; set; } = -1;
        public Int32 Sold90Days { get; set; } = -1;
        public Int32 Sold12Months { get; set; } = -1;
        public Int32 SoldLifetime { get; set; } = -1;
        public Boolean IsTopSeller { get; set; } = false;
        public DateTime AddedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime PostedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime UpdateDate { get; set; } = new DateTime(1753, 1, 1);
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public String ErrorMessage { get { return String.IsNullOrEmpty(this.SellerId) ? $"Seller Id is Empty." : String.Empty; } }
        public Boolean HasError { get { return !String.IsNullOrEmpty(this.ErrorMessage); } }
        public SellerInfo() { }
        public SellerInfo(CategoryInfo category, IWebElement element, Boolean isTopSeller) : this()
        {
            this.CategoryId = category.CategoryId;
            this.Category = category.Name;
            this.Name = element.GetValue("span[@class='refinementLink']", "");
            this.SaleItem = element.GetValue("span[@class='narrowValue']", "").Replace("(", "").Replace(")", "").ConvertToInt32();
            this.Url = element.GetValue("", "href");
            foreach (Int32 idLength in this._IdLengths)
            {
                this.SellerId = this.Url.GetKeyBetween(":", "&", idLength);
                if (!String.IsNullOrEmpty(this.SellerId))
                    break;
            }
            if (String.IsNullOrEmpty(this.SellerId))
            {
                this.SellerId = this.Url.GetKeyBetween(":", "&", 23);
            }
        }

        internal void Update(List<IWebElement> elements)
        {
            if (elements != null)
            {
                this.Sold30Days = elements[0].GetValue("span", "").ConvertToInt32();
                this.Sold90Days = elements[1].GetValue("span", "").ConvertToInt32();
                this.Sold12Months = elements[2].GetValue("span", "").ConvertToInt32();
                this.SoldLifetime = elements[3].GetValue("span", "").ConvertToInt32();
            }
            else
            {
                this.Sold30Days = 0;
                this.Sold90Days = 0;
                this.Sold12Months = 0;
                this.SoldLifetime = 0;
                this.Comments = "Not available";
            }
        }

        private string getCategoryId(string imgUrl, String startWith, String endWith)
        {
            String value = String.Empty;
            MatchCollection matches = Regex.Matches(imgUrl, $@"{startWith}(?<Id>.*){endWith}");
            if (matches != null && matches.Count > 0)
                value = matches[0].Groups["Id"].Value;
            else
                value = "";
            return value;
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Category",this.Category),
                new KeyValuePair<String, Object>("@CategoryId",this.CategoryId),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@SaleItem",this.SaleItem),
                new KeyValuePair<String, Object>("@SellerId",this.SellerId),
                new KeyValuePair<String, Object>("@Sold12Months",this.Sold12Months),
                new KeyValuePair<String, Object>("@Sold30Days",this.Sold30Days),
                new KeyValuePair<String, Object>("@Sold90Days",this.Sold90Days),
                new KeyValuePair<String, Object>("@SoldBy",this.SoldBy),
                new KeyValuePair<String, Object>("@SoldLifetime",this.SoldLifetime),
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@Comments",this.Comments),
                new KeyValuePair<String, Object>("@IsTopSeller",this.IsTopSeller),
            };
            return parameters;
        }
    }
    public class CategoryInfo
    {
        public Int32 Id { get; set; } = -1;
        public String CategoryId { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String ToSellersUrl { get; set; } = String.Empty;
        public Int32 Level { get; set; } = -1;
        public Int32 Parent { get; set; } = -1;
        public DateTime AddedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime PostedDate { get; set; } = new DateTime(1753, 1, 1);
        public Boolean Active { get; set; } = false;
        public Boolean Completed { get; set; } = false;
        public String Comments { get; set; } = String.Empty;
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public String ErrorMessage { get { return String.IsNullOrEmpty(this.CategoryId) ? String.Empty : $"Category Id is Empty."; } }
        public Boolean HasError { get { return !String.IsNullOrEmpty(this.ErrorMessage); } }
        public CategoryInfo() { }
        public CategoryInfo(String code, CategoryInfo parent, IWebElement element) : this()
        {
            this.Parent = parent.Id;
            this.Level = parent.Level + 1;
            switch (code)
            {
                case "A":
                    this.Name = element.GetValue("span", "");
                    this.Url = element.GetValue("", "href");
                    this.CategoryId = getCategoryId(this.Url, "&node=", "&ref_");
                    break;
                case "B":
                    this.Name = element.GetValue("span", "");
                    this.Url = element.GetValue("", "href");
                    this.CategoryId = getCategoryId(this.Url, "&rnid=", "");
                    break;
                default:
                    this.Name = element.GetValue("", "");
                    this.Url = element.GetValue("", "href");
                    this.Description = element.GetValue("", "title");
                    this.CategoryId = getCategoryId(this.Url, "&node=", "");
                    break;
            }
        }

        public CategoryInfo(IWebElement element) : this()
        {
            this.Name = element.GetValue("h2", "");
            this.Level = 1;
            this.Url = element.GetValue("img", "src");
            this.CategoryId = getCategoryId(this.Url, "_CB", "_.");
        }

        private string getCategoryId(string imgUrl, String startWith, String endWith)
        {
            String value = String.Empty;
            MatchCollection matches = Regex.Matches(imgUrl, $@"{startWith}(?<Id>.*){endWith}");
            if (matches != null && matches.Count > 0)
                value = matches[0].Groups["Id"].Value;
            else
                value = "";
            return value;
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@CategoryId",this.CategoryId),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@ToSellersUrl",this.ToSellersUrl),
                new KeyValuePair<String, Object>("@Parent",this.Parent),
                new KeyValuePair<String, Object>("@Completed",this.Completed),
                new KeyValuePair<String, Object>("@Level",this.Level),
                new KeyValuePair<String, Object>("@Comments",this.Comments)
            };
            return parameters;
        }
    }
}
