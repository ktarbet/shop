using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reclamation.Core;

namespace Shop
{
    class IdwrShifts
    {

        static void Main(string[] args)
        
        {

            var tmpDir = FileUtility.GetTempPath();
            string idwrFile = Path.Combine(tmpDir, "shifts.html");

            if( !File.Exists(idwrFile))
                Web.GetFile("http://www.waterdistrict1.com/SHIFTS.htm", idwrFile);

            string html = File.ReadAllText(idwrFile);
            Console.WriteLine("input html is " + html.Length + " chars");
            html = Web.CleanHtml(html);

            html = ConvertHtmlTableToCsv(html);
            
            // Get all shifts from the CBTTs we need in a nice clean format
            html = ConvertCSVToShiftFormat(html);
            
            var cleanFile = Path.Combine(tmpDir, "shifts.csv");


            File.WriteAllText(cleanFile, html);
            Console.WriteLine("cleaned html is " + html.Length + " chars");
            Console.WriteLine(cleanFile);

        }

        private static string ConvertCSVToShiftFormat(string html)
        {
            // Some strings had weird issues ignor these for now LPPI,MLCI,MPCI

            //string[] cbtt = {"AFCI","BFCI","BMCI","CBCI","CRCI","ELCI","ENTI","GWCI","IDCI","LABI","LPPI",
            //                    "MIII","MLCI","MPCI","NMCI","OSCI","PLCI","RECI","RSDI","SMCI","SNDI","TCNI",
            //                    "TRCI","WACI"};

            string[] cbtt = {"AFCI","BFCI","BMCI","CBCI","CRCI","ELCI","ENTI","GWCI","IDCI","LABI",
                                "MIII","NMCI","OSCI","PLCI","RECI","RSDI","SMCI","SNDI","TCNI",
                                "TRCI","WACI"};
          
            string[] CRLF = { "\r\n" };
            string[] lines = html.Split(CRLF, StringSplitOptions.None);
            string cleanFile = "cbtt,pcode,date_measured,discharge,stage,shift\r\n";

            for (int i = 0; i < cbtt.Count(); i++)
            {
                var line = Array.Find(lines, s => s.Contains(cbtt[i]));
                string[] str = line.ToString().Split(',');
                int num = (str.Count() - 5)/4;
                int idx = 5;
                for (int j = 0; j < num; j++)
                {
                    cleanFile = cleanFile + cbtt[i] + ",CH," + str[idx] + "," +
                        str[idx + 2] + "," + str[idx + 1] + "," + str[idx + 3] + "\r\n";
                    idx = idx + 4;
                }
                
            }

            return cleanFile;

        }

        private static string ConvertHtmlTableToCsv(string html)
        {
            string[] dltText = { "<p>&nbsp;</p>", "<td>", "</td>", "<tr>", "</tr>", "<p>", "<table>", "</table>"
                               ,"<div>","</div>","<body>","</body>","<html>","</html>","<head>"
                               ,"</head>","</b>","&nbsp;","&amp;"};

            for (int i = 0; i < dltText.Count(); i++)
            {
                html = html.Replace(dltText[i], "");
            }

            html = html.Replace("</p>", ",");
            html = html.Replace("<b>,", "");
            html = ReplaceWithExpression(html, @"[\s\r\n]+", " ");
            html = html.Replace("<b>", "\r\n");

            return html;

        }

        private static string ReplaceWithExpression(string html, string s, string replace)
        {
            RegexOptions o = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            bool isMatch = Regex.IsMatch(html, s, o);
            if (isMatch)
            {
                Console.WriteLine(s);
                var mc = Regex.Matches(html, s, o);
            }
            html = Regex.Replace(html, s, replace, o);
            return html;
        }
        
        
    }
}
