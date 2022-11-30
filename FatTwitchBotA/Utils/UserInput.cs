using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FatTwitchBotA.Utils
{
  public class UserInput
  {
    // (untested with # !)
    public static string cleanString(string dirty)
    { 
        // returns a string after stripping out all nonalphanumeric characters except ' ' (whitespace) #, @, - (a dash), and . (a period)
        Console.WriteLine("DIRTY> " + dirty);
        Console.WriteLine("CLEAN> " + Regex.Replace(dirty, @"[^\w\. #!@-]", ""));
        return Regex.Replace(dirty, @"[^\w\.#!@-]", "");     
    }

  }
}
