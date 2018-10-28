/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.07.22
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

namespace Utilities
{
    public static class DateTimeExtension
    {
        public static TimeSpan Elapse(this DateTime dateTimeStart, DateTime dateTimeEnd, Int32 count = 1)
        {
            TimeSpan timeSpanStart = TimeSpan.FromTicks(dateTimeStart.Ticks);
            TimeSpan timeSpanEnd = TimeSpan.FromTicks(dateTimeEnd.Ticks);
            return TimeSpan.FromTicks((timeSpanEnd - timeSpanStart).Ticks / count);
        }
    }
}
