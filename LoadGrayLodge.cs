using Reclamation.Core;
using Reclamation.TimeSeries;
using Reclamation.TimeSeries.Excel;
using Reclamation.TimeSeries.Hydromet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    class LoadWaterUsbrGov
    {
        static void Main(string[] args)
        {

            Console.WriteLine("connecting to database");
            var svr = MySqlServer.GetMySqlServer("140.215.104.92", "timeseries", "");
            TimeSeriesDatabase db = new TimeSeriesDatabase(svr);
            Console.WriteLine("reading site catalog");
            var sites = db.GetSiteCatalog();

            sites.AddsitecatalogRow("schwind", "Schwind", "CA",
                "39.34752", "-121.80084", "", "", "", "", "", 0, "", "", "", "", "", "");
            sites.AddsitecatalogRow("cassidy", "Cassidy", "CA",
                "39.33243","-121.78971", "", "", "", "", "", 0, "", "", "", "", "", "");
            sites.AddsitecatalogRow("risingriver", "Rising River", "CA",
                "39.33097","-121.76636", "", "", "", "", "", 0, "", "", "", "", "", "");
            
            svr.SaveTable(sites);


            Console.WriteLine("reading series catalog");
            var series = db.GetSeriesCatalog();

            int grayLodge = series.GetOrCreateFolder("water.usbr.gov","MP","Gray Lodge");
            
            string fn = @"U:\water.usbr.gov\2016\GrayLodgeData.xlsx";

            var s = new ExcelDataReaderSeries(fn, "Rising River", "A", "B", "cfs");
            s.Name = "Flow";
            s.Table.TableName = "mpgraylodge_instant_rising_" + s.Name; ;
            s.Read();
           // db.AddSeries()

            svr.SaveTable(series);

            Console.WriteLine();
        }



    }
}
