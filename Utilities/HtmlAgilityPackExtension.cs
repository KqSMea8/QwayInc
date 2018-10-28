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

using System.Data.SqlClient;
using System.Data;
using HtmlAgilityPack;

namespace Utilities
{
    public static class HtmlAgilityPackExtension
    {
        #region Private Constants
        #endregion Private Constants
        public static Dictionary<String, String> GetValues2(this HtmlDocument htmlDocument, String xPath, String xPathKey, String xPathValue, String attribute = "")
        {
            StatusExtension.Initialize();
            Dictionary<String, String> dic = new Dictionary<String, String>();
            try
            {
                HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode(xPath);
                if (node != null)
                {
                    HtmlNodeCollection nodesKey = htmlDocument.DocumentNode.SelectNodes(xPathKey);
                    HtmlNodeCollection nodesValue = htmlDocument.DocumentNode.SelectNodes(xPathValue);
                    for (Int32 index = 0; index < Math.Min(nodesKey.Count, nodesValue.Count); ++index)
                        dic[nodesKey[index].getValue(attribute)] = nodesValue[index].getValue(attribute);
                }
            }
            catch (Exception) { }
            return dic;
        }
        public static Dictionary<String, String> GetValues(this HtmlDocument htmlDocument, String xPath, String xPathKey, String xPathValue, String attribute = "")
        {
            StatusExtension.Initialize();
            Dictionary<String, String> dic = new Dictionary<String, String>();
            try
            {
                HtmlNodeCollection nodes = htmlDocument.DocumentNode.SelectNodes(xPath);
                if (nodes != null)
                    foreach (HtmlNode node in nodes)
                        dic[node.GetValue(xPathKey, attribute)] = node.GetValue(xPathValue, attribute);
            }
            catch (Exception) { }
            return dic;
        }
        public static String GetValue(this HtmlDocument htmlDocument, String xPath, String attribute = "", String defaultValue = "")
        {
            return htmlDocument.DocumentNode.GetValue(xPath, attribute, defaultValue);
        }

        public static String GetValue(this HtmlNode nodeParent, String xPath, String attribute = "", String defaultValue = "", Boolean removeReturn = true)
        {
            String returnValue = defaultValue;
            HtmlNode node = String.IsNullOrEmpty(xPath) ? nodeParent : nodeParent.SelectSingleNode(xPath);
            if (node != null)
                returnValue = WebExtension.DecodeHtml(node.getValue(attribute), removeReturn: removeReturn);
            return returnValue;
        }
        private static String getValue(this HtmlNode node, String attribute = "")
        {
            return String.IsNullOrEmpty(attribute) ? WebExtension.DecodeHtml(node.InnerText.Trim()) : node.GetAttributeValue(attribute, "").Trim();
        }
        public static String GetInnerText(HtmlNode nodeParent, String xPath)
        {
            String returnValue = String.Empty;
            HtmlNode node = nodeParent.SelectSingleNode(xPath);
            if (node != null)
                returnValue = WebExtension.DecodeHtml(node.InnerText.Trim());
            return returnValue;
        }

        public static string GetAttributeValue(HtmlNode nodeParent, string xPath, string attribute, String defaultValue)
        {
            String returnValue = String.Empty;
            HtmlNode node = nodeParent.SelectSingleNode(xPath);
            if (node != null)
                returnValue = node.GetAttributeValue(attribute, defaultValue).Trim();
            return returnValue;
        }
    }
}
