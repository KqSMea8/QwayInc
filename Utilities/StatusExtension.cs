/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.05.03
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

namespace Utilities
{
    public static class StatusExtension
    {
        #region Public Properties
        public static Boolean IsErrorDisplayed = true;
        public static String Status = String.Empty;
        public static String ErrorMessage = String.Empty;
        public static System.Exception Exception = null;
        public static List<String> ErrorMessages = new List<String>();
        public readonly static Boolean HasError = !String.IsNullOrEmpty(ErrorMessage) || (ErrorMessages != null && ErrorMessages.Count > 0);
        #endregion
        static StatusExtension()
        {
            Initialize();
        }
        public static void Initialize()
        {
            IsErrorDisplayed = true;
            ErrorMessage = String.Empty;
            Status = String.Empty;
            ErrorMessages = new List<String>();
            Exception = null;
        }
    }
}
