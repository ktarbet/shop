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
using Reclamation.TimeSeries.Decodes;
using Reclamation.TimeSeries;


namespace Shop
{
    class AddOWRDToPiscesAndDecodes
    {
        static void Main(string[] args)
        {
            string serverIP = "lrgs1";
            //.Logger.Logger.EnableLogger();

            var fn = @"V:\PN6200\Hydromet\HelpWanted\migrate to linux.xls";
            var tbl = ExcelDB.Read(fn,"owrd_1");

            var cs = PostgreSQL.CreateADConnectionString(serverIP,"timeseries");
            PostgreSQL svr = new PostgreSQL(cs);

            //SQLiteServer svr = new SQLiteServer(@"c:\temp\lrgs1.pdb");
            TimeSeriesDatabase db = new TimeSeriesDatabase(svr);

            AddToPisces(tbl,db);
            AddSitesToDecodes(serverIP, tbl);

        }

        private static void AddToPisces(DataTable tbl, TimeSeriesDatabase db)
        {
            Console.WriteLine("Reading mcf ");
            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);

            var site = mcf.sitemcf;
            var siteCatalog = db.GetSiteCatalog();
            PiscesSeriesLoader loader = new PiscesSeriesLoader(db);

            foreach (DataRow row in tbl.Rows)
            {
                var cbtt = row[0].ToString().ToLower().Trim();
                Console.Write(cbtt + " ");

                //var dbSite = siteCatalog.First(x => x.siteid == cbtt.Trim());

                var site1 = site.First(x => x.SITE.Trim() == cbtt.ToUpper().Trim());

                if (site1.CTYPE.Trim() != "P") // parameter based
                    throw new Exception("not parameter based processing");
                int folderID = loader.SeriesCatalog.GetOrCreateFolder("hydromet", cbtt, "instant");

                var pcodes = mcf.pcodemcf.Where(x => x.PCODE.IndexOf(cbtt.ToUpper()) >= 0
                     && x.ACTIVE == 1 );

                AddPcodesToPisces(loader,folderID, cbtt, pcodes);
               db.Server.SaveTable(loader.SeriesCatalog);
                Console.WriteLine();
            }
        }

        private static void AddPcodesToPisces(PiscesSeriesLoader loader, 
            int parentID,string cbtt, EnumerableRowCollection<McfDataSet.pcodemcfRow> pcodes)
        {
            foreach (var item in pcodes)
            {
                var rtcproc = item.RTCPROC.Trim().Replace("\0", "");
                var pc = item.PCODE.Trim();
                if (pc.Length > 8)
                    pc = pc.Substring(8).Trim();

                pc = pc.ToLower();

                loader.AddToPisces( parentID, cbtt, pc);

                if (rtcproc != "")
                    pc += "[" + rtcproc + "]";
                Console.Write(pc + ",");
            }
        }

        

        private static void AddSitesToDecodes(string serverIP, DataTable tbl)
        {
            var siteFilter = String.Join(",", DataTableUtility.Strings(tbl, "", "cbtt"));
            siteFilter = siteFilter.ToUpper();

            McfToDecodes.Import(serverIP, "hydromet_decodes", siteFilter, "owrd");
        }
    }
}
