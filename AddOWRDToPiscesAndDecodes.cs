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
    class AddOWRDToPiscesAndDecodes
    {
        static void Main(string[] args)
        {
            string serverIP = "140.218.6.20";
            //.Logger.Logger.EnableLogger();

            var fn = @"V:\PN6200\Hydromet\HelpWanted\migrate to linux.xls";
            var tbl = ExcelDB.Read(fn,"owrd_1");

            AddSitesToDecodes(serverIP, tbl);

            return;

            Console.WriteLine("Reading mcf ");
            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);

            var site = mcf.sitemcf;

            foreach (DataRow row in tbl.Rows)
            {
                var cbtt = row[0].ToString().ToUpper().Trim();
                Console.Write(cbtt+ " ");

                var site1 = site.First(x => x.SITE.Trim() == cbtt.Trim() );

                Console.Write(site1.NESSID+" " );

                

                // look for shared nessid...
                var others = site.Where(x => x.NESSID == site1.NESSID && x.NESSID != "0");
                Console.Write("shared:");
                if (others.Count() > 1)
                {
                    throw new Exception("Error:  nessid is shared in multiple sites");
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

        private static void AddSitesToDecodes(string serverIP, DataTable tbl)
        {
            var siteFilter = String.Join(",", DataTableUtility.Strings(tbl, "", "cbtt"));
            siteFilter = siteFilter.ToUpper();

            MrdbToDecodes.McfToDecodes.Import(serverIP, "hydromet_decodes", siteFilter, "owrd");
        }
    }
}
