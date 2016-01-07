using Reclamation.Core;
using Reclamation.TimeSeries;
using Reclamation.TimeSeries.Hydromet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    class UpdateWaterUsbr
    {
        static void Main(string[] args)
        {

            Console.WriteLine("connecting to database");
            var svr = MySqlServer.GetMySqlServer("test.water.usbr.gov", "timeseries", "");
            TimeSeriesDatabase db = new TimeSeriesDatabase(svr);
            Console.WriteLine("reading sitecatalog");
            var sites = db.GetSiteCatalog();

            var sc = db.GetSeriesCatalog("isfolder=0");

            var prop = db.GetSeriesProperties(true);
            for (int i = 0; i < sc.Count; i++)
            {
                var s = db.GetSeries(sc[i].id);
                var por = s.GetPeriodOfRecord();
                
               if(por.Count >0)
               {
                   s.Properties.Set("t1",por.T1.ToString("yyyy-MM-dd"));
                   s.Properties.Set("t2",por.T2.ToString("yyyy-MM-dd"));
                   Console.WriteLine(s.Name);
               }
            }

            db.Server.SaveTable(prop);

            //SetRegioninSiteTable(db, sites);
           // UpdateGPSiteInfo(sites);

            svr.SaveTable(sites);

            Console.WriteLine();
        }

        private static void SetRegioninSiteTable(TimeSeriesDatabase db, TimeSeriesDatabaseDataSet.sitecatalogDataTable sites)
        {
            var sc = db.GetSeriesCatalog("isfolder=0");

            for (int i = 0; i < sites.Rows.Count; i++)
            {
                var siteid = sites[i].siteid;
                var rows = sc.Select("connectionString like 'server=PN;cbtt=" + siteid + ";%'");
                if (rows.Length > 0 && sites[i].agency_region == "")
                {
                    sites[i].agency_region = "PN";
                }

                rows = sc.Select("connectionString like 'server=LCHDB2%' and siteid ='" + siteid + "'");
                if (rows.Length > 0 && sites[i].agency_region == "")
                {
                    sites[i].agency_region = "LC";
                }

                rows = sc.Select("connectionString like 'server=UCHDB2%' and siteid ='" + siteid + "'");
                if (rows.Length > 0 && sites[i].agency_region == "")
                {
                    sites[i].agency_region = "UC";
                }

            }
        }

        private static void UpdateGPSiteInfo(TimeSeriesDatabaseDataSet.sitecatalogDataTable sc)
        {
            Console.WriteLine("reading gp excel");
            var fn = @"U:\water.usbr.gov\data\GPsitesTESSELAPP_kt.xlsx";
            var xls = new NpoiExcel(fn);
            var tbl = xls.ReadDataTable(0);
            //var tbl = ExcelDB.Read(fn, 0);

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                var desc = tbl.Rows[i]["Description"].ToString();
                var lat = tbl.Rows[i]["Latitude"].ToString().Trim();
                var lon = tbl.Rows[i]["Longitude"].ToString().Trim();
                var state = tbl.Rows[i]["State"].ToString().Trim();
                if (lon[0] != '-')
                    lon = "-" + lon;
                //var office = tbl.Rows[i]["office"].ToString();
                var siteid = tbl.Rows[i]["Site Name"].ToString().ToLower().Trim();
                var siteType = tbl.Rows[i]["type"].ToString().ToLower().Trim();
                

                var siteRow = sc.FindBysiteid(siteid);

                if (siteRow == null)
                {
                    Console.WriteLine("new site : " + siteid);
                    siteRow = sc.NewsitecatalogRow();
                    siteRow.siteid = siteid;
                    sc.AddsitecatalogRow(siteRow);

                }
                else
                {
                    Console.WriteLine("existing site : " + siteRow.siteid);
                }

                siteRow.description = desc;
                siteRow.state = state;
                siteRow.latitude = lat;
                siteRow.longitude = lon;
                //siteRow.responsibility = office;
                siteRow.type = siteType;
                siteRow.agency_region = "GP";


            }
        }


    }
}
