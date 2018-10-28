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
using System.Text.RegularExpressions;

namespace Utilities
{
    public static class StringExtension
    {
        public static String Right(this String value, Int32 length)
        {
            String returnValue = String.Empty;
            if (!String.IsNullOrEmpty(value))
                if (value.Length >= length)
                    returnValue = value.Substring(value.Length - length, length);
            return returnValue;
        }
        public static String GetDomain(this String email)
        {
            String domain = String.Empty;
            String[] items = email.Split('@');
            if (items.Length > 1)
                domain = items[1];
            return domain;
        }
        public static String GetKeyBetween(this String str, String startWith, String endWith, Int32 length = 0)
        {
            String value = String.Empty;
            String pattern = $@"({startWith})(?<Key>[a-zA-Z0-9]@@@)({endWith})";
            pattern = pattern.Replace("@@@", length > 0 ? String.Concat("{", length, "}") : "");
            MatchCollection matches = Regex.Matches(str, pattern);
            if (matches != null && matches.Count > 0)
                value = matches[0].Groups["Key"].Value;
            return value;
        }

        /// <summary>
        /// [OK] Convert To Boolean from String
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>
        /// Boolean.Parse(value)
        /// true if value is equivalent to the value of the Boolean.TrueString field; 
        /// false if value is equivalent to the value of the Boolean.FalseString field.
        /// Only parse: true/false.
        /// cannot parse:
        ///     null
        ///     ''
        ///     '0'
        ///     '1'
        ///     '-1'
        ///     any other string
        /// </remarks>
        public static Boolean ToBoolean(this String str, Boolean defaultValue = false)
        {
            //return Boolean.Parse(str);
            Boolean result = false;
            str = String.IsNullOrEmpty(str) ? "" : str.Trim().ToUpper();
            switch (str.Length)
            {
                case 1:
                    result = (str == "1" || str == "Y" || str == "T");
                    break;
                case 2:
                    result = (str != "NO"); // || (str == "ON");
                    break;
                case 3:
                    result = (str == "YES");
                    break;
                case 4:
                    result = (str == "TRUE");
                    break;
                case 5:
                    result = (str != "FALSE");
                    break;
                case 6:
                    result = (str != "FAILED");
                    break;
                case 7:
                    result = (str == "SUCCESS");
                    //result = (str != "FAILURE");
                    break;
                default:
                    result = defaultValue;
                    break;
            }
            return result;
        }
        public static Int32 ConvertToInt32(this String str, Int32 defaultValue = 0)
        {
            Int32 value = defaultValue;
            str = str.Replace(",", "");
            Int32.TryParse(str, out value);
            return value;
        }
        public static DateTime ConvertToDate(this String date, String[] removeList = null)
        {
            DateTime dateTime = new DateTime();
            if (!String.IsNullOrEmpty(date))
            {
                if (removeList != null && removeList.Length > 0)
                    foreach (String str in removeList)
                        date = date.Replace(str, "");
                if (!String.IsNullOrEmpty(date))
                    try
                    {
                        dateTime = Convert.ToDateTime(date);
                    }
                    catch (System.Exception ex)
                    {
                    }
            }
            return dateTime;
        }

        public static String GetLastWord(this String str, Char delimiter = ' ')
        {
            String[] values = str.Split(delimiter);
            return values.LastOrDefault();
        }
        public static String RemoveInvalidChars(this String fileName)
        {
            String regexSearch = new String(System.IO.Path.GetInvalidFileNameChars());
            Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return regex.Replace(fileName, "");
        }

        public static Boolean IsWebSiteAvailable(String Url)
        {
            StatusExtension.Initialize();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.Method = "GET";
            HttpWebResponse response = null;
            HttpStatusCode code;
            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    code = response.StatusCode;
                }
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
                code = response.StatusCode;
                StatusExtension.ErrorMessage = ex.Message;
            }
            return code == HttpStatusCode.OK;
        }
    }
}
