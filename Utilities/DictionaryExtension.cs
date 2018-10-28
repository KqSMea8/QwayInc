/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.06.16
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
    public static class DictionaryExtension
    {
        public static String GetValue(this Dictionary<String, String> dic, String key)
        {
            String value = String.Empty;
            try
            {
                if (dic.ContainsKey(key))
                    value = dic[key];
                else
                {
                    key = dic.Keys.Where(k => k.StartsWith(key)).First();
                    if (!String.IsNullOrEmpty(key))
                        value = dic[key];
                }
            }
            catch (Exception) { }
            return value;
        }

        public static String ToString2(this Dictionary<String, String> dic)
        {
            return String.Join("\n", dic.Select(item => String.Format("[{0}]: {1}", item.Key, item.Value)).ToArray<String>());
        }
    }
}
