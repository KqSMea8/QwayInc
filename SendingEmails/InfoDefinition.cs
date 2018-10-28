/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.09.22
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

using Utilities;

using System.Text.RegularExpressions;

namespace SendingEmails
{
    public class EmailInfo
    {
        public Int32 Id { get; set; } = -1;
        public String Code { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public String CompanyName { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }

        public EmailInfo() { }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Code",this.Code),
                new KeyValuePair<String, Object>("@Comments",this.Comments)
            };
            return parameters;
        }
    }
}
