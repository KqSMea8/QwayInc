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
