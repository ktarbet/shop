using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reclamation.Core;
using System.Globalization;

namespace Shop
{
    class IdwrShifts
    {

        static void Main(string[] args)
        
        {

            string idwrFile = "shifts.html";

            if( !File.Exists(idwrFile))
                Web.GetFile("http://www.waterdistrict1.com/SHIFTS.htm", idwrFile);

            string html = File.ReadAllText(idwrFile);
            Console.WriteLine("input html is " + html.Length + " chars");
            html = Web.CleanHtml(html);
            File.WriteAllText("stage1.txt",html);
            html = ConvertHtmlTableToCsv(html);
            
            // Get all shifts from the CBTTs we need in a nice clean format
            html = ConvertCSVToShiftFormat(html);

            var cleanFile = "shifts.csv";


            File.WriteAllText(cleanFile, html);
            Console.WriteLine("cleaned html is " + html.Length + " chars");
            Console.WriteLine(cleanFile);

        }

        private static string ConvertCSVToShiftFormat(string html)
        {

            string[] cbtt = {"AFCI","BFCI","BMCI","CBCI","CRCI","ELCI","ENTI","GWCI","IDCI","LABI","LPPI",
                                "MIII","MLCI","MPCI","NMCI","OSCI","PLCI","RECI","RSDI","SMCI","SNDI","TCNI",
                                "TRCI","WACI"};

            /*
             *  DATE  ,    GAGE HT  ,    CFS  ,    SHIFT  , 
         4/18/2015  ,    1.04  ,    152.16  ,    +0.29  , 
         4/21/2015  ,    1.07  ,    151.5  ,    +0.27  , 
         5/15/2015  ,    1.56  ,    211.26  ,    +0.11  , 
         6/16/2015  ,    1.77  ,    118.36  ,    -0.64  , 
         7/25/2015  ,    1.67  ,    151.3  ,    -0.35  , 
       ,  ,  ,  , 
       ,  ,  ,  , 
             */
            string[] CRLF = { "\r\n" };
            string[] lines = html.Split(CRLF, StringSplitOptions.None);
            string cleanFile = "cbtt,pcode,date_measured,discharge,stage,shift\r\n";


            var tf = new TextFile(html.Split(new char[]{'\n','\r'}, StringSplitOptions.RemoveEmptyEntries));
            for (int i = 0; i < cbtt.Length; i++)
            {
                var idx = tf.IndexOf(cbtt[i]);
                
                if( idx >=0)
                {
                   var idxDate = tf.IndexOf("DATE", idx);
                    if( idxDate > idx+5)// date should be within 5 lines of cbtt
                    {
                        Console.WriteLine("Error: did not find DATE with cbtt ="+cbtt);
                        continue;
                    }
                    // now parse data until it runs out
                    for (int j = idxDate+1; j < tf.Length; j++)
                    {
                        DateTime t;
                        var tokens = tf[j].Split(',');
                        if( tokens.Length < 4)
                            break;
                        if( !DateTime.TryParseExact(tokens[0].Trim(), "M/d/yyyy", CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out t) )
                            break;

                        var x = cbtt[i] + "," + t.ToShortDateString() + "," + tokens[1].Trim() + "," + tokens[2].Trim() + "," + tokens[3].Trim();
                        cleanFile += x + "\r\n";
                        Console.WriteLine(x);

                    }
                }
                else
                {
                    Console.WriteLine("Error: did not find "+cbtt[i]);
                }

            }

            return cleanFile;

        }



        /// <summary>
        /// cleanup and make csv between <table> and </table> tags
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static string ConvertHtmlTableToCsv(string html)
        {
            var mc = Regex.Matches(html, "<table(.*?)>.*?</table>", RegexOptions.Singleline);
            //
            foreach (Match m in mc)
            {// format table one line per table row <tr>
                string t = m.Value;
                t = t.Replace("\r\n", "");
                t = t.Replace("</tr>", "\r\n");
                t = t.Replace("<tr>", "");
                t = ReplaceWithExpression(t, @"</td>", ",");
                t = t.Replace("<p>", "");
                t = t.Replace("</p>", "");
                t = t.Replace("<b>", "");
                t = t.Replace("<td>", "");
                t = t.Replace("</b>", "");
                t = t.Replace("<table>", "\n");
                t = t.Replace("</table>", "\n");
                t = t.Replace("&nbsp;", "");

                html = html.Replace(m.Value, t);
            }
            return html;
        }

        private static string ReplaceWithExpression(string html, string s, string replace)
        {
            RegexOptions o = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
            html = Regex.Replace(html, s, replace, o);
            return html;
        }
        
    }
}
