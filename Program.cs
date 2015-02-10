using Reclamation.Core;
using Reclamation.TimeSeries;
using Reclamation.TimeSeries.Hydromet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Shop
{
    class Program
    {
        static void Main(string[] args)
        {
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            // type= as Reservoir if has AF.
            // type as streamgage if has Q
            // type = weather station if agrimet.
            // type = snotel if nrcs
            // type = water quality
            // type as canal if has QC
            // property.public_visible = true
            // property.program = "hydromet|agrimet";
            // property.streamgaging = USGS|OWRD|OWRD|IdahoPower|IDWR|Weir
            // property.basin = Boise|Idaho Falls|Bend|Flathead|Yakima|Umatilla|Rogue
            // property.snotel_id = XX123
            // property.usgs_id = 12345678
            // property.owrd_id = 12345678
            // property.idahopower_id = 12345678
            // property.source = GOES|Manual Entry|USGS Web Service|Irrigation District|NRCS Web Service


            //UpdateUSGSLatLong(db);
            //AddUSGSIDProperty(db);
            SetReservoirs(db);
            
            //AddSitesFromMCF(db);

           // ReadHydroMapForLatLong();

        }

        private static void SetReservoirs(TimeSeriesDatabase db)
        {
            var c = db.GetSiteCatalog("elevation_method like 'cma_google%'", "");
            var p = db.GetSiteProperties();
            for (int i = 0; i < c.Count; i++)
            {
                var row = c[i];
                if (row.elevation_method == "cma_google_1")
                {
                    row.type = "weather station";
                    p.Set("public_visible", "true", row.siteid);
                }

                if (row.elevation_method == "cma_google")
                {
                    row.type = "reservoir";
                    p.Set("public_visible", "true",row.siteid);
                }

                p.Set("program", "hydromet",row.siteid);

            }
            db.Server.SaveTable(c);
            db.Server.SaveTable(p);
        }

        private static void AddUSGSIDProperty(TimeSeriesDatabase db)
        {
            var c = db.GetSiteCatalog();
            Console.WriteLine("found " + c.Rows.Count + " sites in Pisces ");
            var fn = FileUtility.GetFileReference("USGS StreamGages 201109.xls");
            var xls = new NpoiExcel(fn);
            var usgsXLS = xls.ReadDataTable(0);

            var prop = db.GetSiteProperties();

        }


        /// <summary>
        /// add sites that are not in Pisces
        /// </summary>
        /// <param name="db"></param>
        private static void AddSitesFromMCF(TimeSeriesDatabase db)
        {
            var c = db.GetSiteCatalog();
            Console.WriteLine("found "+c.Rows.Count+" sites in Pisces " );

            var pnMcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);
            Console.WriteLine(" found "+pnMcf.sitemcf.Rows.Count+" sites in MCF");

            for (int i = 0; i < pnMcf.sitemcf.Count; i++)
            {
                var row = pnMcf.sitemcf[i];

                string cbtt = row.SITE.ToLower();
                var crow = c.FindBysiteid(cbtt);
                if (crow == null)
                {
                    Console.WriteLine("Site "+cbtt+" not found in Pisces");
                    c.AddsitecatalogRow(cbtt, row.DESCR, row.STATE);
                }
            }
            db.Server.SaveTable(c);
        }

        /// <summary>
        /// Updae and USGS sites that have the DDMMSS type of Lat/Long without decimal places
        /// using Lat Long from USGS
        /// </summary>
        private static void UpdateUSGSLatLong(TimeSeriesDatabase db)
        {
            var fn = FileUtility.GetFileReference("USGS StreamGages 201109.xls");
            var xls = new NpoiExcel(fn);
            var usgsXLS = xls.ReadDataTable(0);

            var c = db.GetSiteCatalog();
            Console.WriteLine("found " + c.Rows.Count + " sites in Pisces ");
            var pnMcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);

            for (int i = 0; i < c.Count; i++)
            {
                if (c[i].latitude.IndexOf(".") < 0 && c[i].longitude.IndexOf(".") < 0 
                    && c[i].longitude.Trim().Length > 2)
                {
                    // check for USGS
                    var rows = pnMcf.sitemcf.Select("site = '" + c[i].siteid + "'");
                    if (rows.Length > 0)
                    {
                        var altid = rows[0]["altid"].ToString();
                        if (altid.Trim().Length > 6)
                        {
                            // lookup new lat/long from XLS
                            var usgsRows = usgsXLS.Select("STAID = '" + altid + "'");
                            if (usgsRows.Length > 0)
                            {
                                var LAT_GAGE =usgsRows[0]["LAT_GAGE"].ToString();
                                var LNG_GAGE = usgsRows[0]["LNG_GAGE"].ToString();

                                Console.WriteLine(altid + " " + c[i].siteid + " " + c[i].latitude + "/"+LAT_GAGE+" "  + c[i].longitude + "/"+LNG_GAGE);
                                c[i].longitude = LNG_GAGE;
                                c[i].latitude = LAT_GAGE;
                            }
                        }
                    }

                }
            }
            var j = db.Server.SaveTable(c);
            Console.WriteLine("saved "+j+" rows ");
        }

        private static void ReadHydroMapForLatLong()
        {

            TextFile tf = new TextFile(@"V:\PN6200\Hydromet\Web\www.usbr.gov\hydromet\hydrometmap.html");
            for (int i = 0; i < tf.Length; i++)
            {

                if (tf[i].IndexOf(@"GLatLng(") >= 0)
                {
                    var m1 = Regex.Match(tf[i], @"GLatLng\(([0-9\.]*)\,\s*(-[0-9\.]*)\)");

                    var m = Regex.Match(tf[i + 1], @"GMarker\(.*\(([A-Z]{3,4})\)");

                    if (m.Success)
                    {
                        Console.WriteLine(m.Groups[1].Value + " , " + m1.Groups[1] + ", " + m1.Groups[2]);
                        //Console.WriteLine(m.Value);
                    }

                }
            }
        }
    }
}
