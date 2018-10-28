/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.07.21
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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Data;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Utilities
{
    public static class JsonExtension
    {
        private static Dictionary<String, String> _KeyMapping = getSpecialKeyMappings();
        private static Dictionary<String, String> _KeyMappingReverse = _KeyMapping.ToDictionary(x => x.Value, x => x.Key);
        private static Dictionary<String, String> _ValueMapping = getSpecialValueMappings();
        private static Dictionary<String, String> _ValueMappingReverse = _ValueMapping.ToDictionary(x => x.Value, x => x.Key);

        public static Dictionary<String, String> GetValuesFromJson(this String jsonString)
        {
            StatusExtension.Initialize();
            Dictionary<String, String> dic = new Dictionary<String, String>();
            try
            {
                jsonString = convertToJsonString(jsonString);
                Dictionary<String, String> dicJson = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonString);
                foreach (String key in dicJson.Keys)
                {
                    dic[key] = dicJson[key];
                    foreach (String k in _ValueMappingReverse.Keys)
                        dic[key] = dic[key].Replace(k, _ValueMappingReverse[k]);
                }
            }
            catch (Exception ex) { StatusExtension.ErrorMessage = ex.Message; }
            return dic;
        }
        private static String convertToJsonString(String jsonString)
        {
            if (!jsonString.StartsWith("{"))
                jsonString = String.Concat("{", jsonString, "}");
            foreach (String key in _KeyMapping.Keys)
                jsonString = jsonString.Replace(key, _KeyMapping[key]);
            foreach (String key in _ValueMapping.Keys)
                jsonString = jsonString.Replace(key, _ValueMapping[key]);
            foreach (String key in _KeyMappingReverse.Keys)
                jsonString = jsonString.Replace(key, _KeyMappingReverse[key]);
            return jsonString;
        }
        public static Dictionary<String, String> getSpecialKeyMappings()
        {
            return new Dictionary<String, String>() {
                {":'","@a@" },
                {"',","@b@" },
                {"'}","@c@" },
            };
        }
        public static Dictionary<String, String> getSpecialValueMappings()
        {
            return new Dictionary<String, String>() {
                {"'","@A@" },
                {@"\","@B@" },
                {"/","@C@" },
            };
        }
    }
}
