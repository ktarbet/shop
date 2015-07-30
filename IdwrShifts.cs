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
            /*
             cbtt	idwr_shift
AFCI
BFCI
BMCI
CBCI
CRCI
ELCI
ENTI
GWCI
IDCI
LABI
LPPI
MIII
NLCI
NMCI
OSCI
PLCI
RECI
RSDI
SMCI
SNDI
TCNI
TRCI
WACI


             */

            var tmpDir = FileUtility.GetTempPath();
            string idwrFile = Path.Combine(tmpDir, "shifts.html");

            if( !File.Exists(idwrFile))
                Web.GetFile("http://www.waterdistrict1.com/SHIFTS.htm", idwrFile);

            string html = File.ReadAllText(idwrFile);
            Console.WriteLine("input html is " + html.Length + " chars");
            html = Web.CleanHtml(html);

            var cleanFile = Path.Combine(tmpDir, "shifts.txt");

            // 


            File.WriteAllText(cleanFile, html);
            Console.WriteLine("cleaned html is " + html.Length + " chars");
            Console.WriteLine(cleanFile);

        }
        
        
    }
}
