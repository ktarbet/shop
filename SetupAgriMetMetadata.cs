using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using Reclamation.Core;
using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;



namespace Shop
{
    class SetupAgriMetMetadata
    {
        /*
"properties": {
			"region": "pnro",
			"title": "Afton, WY (AFTY)",
			"url": "aftyda.html"
         */
        static void Main(string[] args)
        {

            // read Existing JSON file
            var json = File.ReadAllText(@"V:\PN6200\Hydromet\Web\www.usbr.gov\agrimet\agrimetmap\usbr_map.json");

            var collection = JsonConvert.DeserializeObject<FeatureCollection>(json);



            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            var sc = db.GetSiteCatalog("type = 'agrimet'");
            var siteProp = new TimeSeriesDatabaseDataSet.sitepropertiesDataTable(db);

            Console.WriteLine(" Found "+sc.Rows.Count +" agrimet sites ");
            Console.WriteLine("site properties has " + siteProp.Rows.Count + " rows ");

            // update pisces URL form Json

            //InsertMontanaSitesFromJsonToPisces(collection, sc);
            foreach (var item in sc)
            {

              var feature = FindFeature(collection, item.siteid);

              if (feature == null)
              {
                  Console.WriteLine("Site not found in JSON "+item.siteid);
                  continue;
              }

              siteProp.Set("program", "agrimet", item.siteid);
              siteProp.Set("url", feature.Properties["url"].ToString(), item.siteid);
              siteProp.Set("region", feature.Properties["region"].ToString(), item.siteid);
            }

            Console.WriteLine("site properties has "+siteProp.Rows.Count+" rows " );

            int i = svr.SaveTable(siteProp);
            Console.WriteLine("saved "+i+" rows");


        }



        private static void InsertMontanaSitesFromJsonToPisces(FeatureCollection collection, TimeSeriesDatabaseDataSet.sitecatalogDataTable sc)
        {
            // insert montana sites into pisces
            for (int i = 0; i < collection.Features.Count; i++)
            {
                Feature item = collection.Features[i];

                if (item.Properties["title"].ToString().IndexOf(" MT ") < 0)
                    continue;


                var rows = sc.Select("siteid = '" + item.Id.ToLower() + "'");
                if (rows.Length == 0) // not  in Pisces
                {
                    Console.WriteLine(item.Id + " not found in Pisces");
                    // put it Pisces.
                    var desc = item.Properties["title"].ToString();
                    var pt = item.Geometry as GeoJSON.Net.Geometry.Point;
                    var pos = pt.Coordinates as GeoJSON.Net.Geometry.GeographicPosition;
                    var lat = pos.Latitude.ToString();
                    var lo = pos.Longitude.ToString();
                    //sc.AddsitecatalogRow(item.Id.ToLower(), desc, "MT", lat, lo, "", "US/Mountain", "", "", "", 0, "usbr_map.json", "", "", "agrimet", "great_plains");
                }

            }
        }

        static Feature FindFeature(FeatureCollection collection, string id)
        {
            Feature rval=null;

            for (int i = 0; i < collection.Features.Count; i++)
            {
                Feature item = collection.Features[i];
                if (String.Compare(item.Id, id, true) == 0)
                {
                    return item;
                }
            }
            return rval;
        }


    }
}
