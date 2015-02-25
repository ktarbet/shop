using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Reclamation.Core;
using Reclamation.TimeSeries.Hydromet;
using System.Data;


namespace Shop
{
    class OWRDInventoryHelper
    {
        static void Main(string[] args)
        {
            // work through migrate to linux sheet
            // and find sites that have alarms, and shared nessid's
            var fn = @"V:\PN6200\Hydromet\HelpWanted\migrate to linux.xls";
            var tbl = ExcelDB.Read(fn, 1);
            Console.WriteLine("Reading mcf ");
            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);

            var site = mcf.sitemcf;

            foreach (DataRow row in tbl.Rows)
            {
                var agency = row[2].ToString();
                if (agency != "OWRD")
                    continue;

                var cbtt = row[0].ToString().ToUpper().Trim();
                Console.Write(cbtt+ " ");

                var site1 = site.First(x => x.SITE.Trim() == cbtt.Trim() );

                Console.Write(site1.NESSID+" " );

                // look for shared nessid...
                var others = site.Where(x => x.NESSID == site1.NESSID && x.NESSID != "0");
                Console.Write("shared:");
                if (others.Count() > 1)
                {
                    foreach (McfDataSet.sitemcfRow item in others)
                    {
                        if( item.SITE != site1.SITE)
                          Console.Write(item.SITE+ ",");    
                    }
                }
                    // check for alarms...

                    var pcodes = mcf.pcodemcf.Where(x => x.PCODE.IndexOf(cbtt) >= 0
                         && x.ACTIVE == 1 && x.ALMSW == 1);

                    Console.Write(" ALARM:");
                    foreach (var item in pcodes)
                    {
                        var pc = item.PCODE.Trim();
                        if (pc.Length > 8)
                            pc = pc.Substring(8);
                        Console.Write(pc+",");
                    }

                Console.WriteLine();
            }


        }
    }
}
