using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

using System.Linq;


namespace SendMailService.Web
{
    public static class Utility
    {

      
        public static string PassNull(this string tmp)
        {
            if (tmp == null)
                return "";
            return tmp;
        }
        public static string PassNull(this object tmp)
        {
            if (tmp == null)
                return string.Empty;
            return tmp.ToString();
        }
     
    }
}
