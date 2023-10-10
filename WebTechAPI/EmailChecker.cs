using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebTechAPI
{
    internal class EmailChecker
    {
        static Regex regex = new Regex(@"^[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        public static bool Check(string email)
        {
            return regex.IsMatch(email);
        } 
    }
}
