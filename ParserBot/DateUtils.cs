using System;
using System.Collections.Generic;
using System.Text;

namespace ParserBot
{
    public static class DateUtils
    {
        public static DateTime fromDate(string year, string month, string day)
        {
            return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
        }
    }
}
