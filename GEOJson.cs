using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Reclamation.Core;
using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shop
{
    /// <summary>
    /// GEOJson CGI program for Pisces
    /// </summary>
    class GEOJson
    {

        static void Main(string[] args)
        {
            //  GEOJson --filter=program:agrimet
            Console.Write("Content-Type:  application/json\n\n");

          var svr = PostgreSQL.GetPostgresServer();
          var db = new TimeSeriesDatabase(svr);

          var features = new List<Feature>();
          FeatureCollection fc = new FeatureCollection(features);

          var sites = db.GetSiteCatalog("type = 'agrimet'");

         var siteProp = new TimeSeriesDatabaseDataSet.sitepropertiesDataTable(db);

          foreach (var s in sites)
          {
              var pos = new GeographicPosition(s.latitude,s.longitude);
              var pt = new GeoJSON.Net.Geometry.Point(pos);

              var props = siteProp.GetDictionary(s.siteid);

              props.Add("cbtt", s.siteid);
              props.Add("title", s.description);
              var feature = new Feature(pt,props,s.siteid);

              fc.Features.Add(feature);
          }

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
          var json = Newtonsoft.Json.JsonConvert.SerializeObject(fc, 
              Newtonsoft.Json.Formatting.Indented,settings);

          Console.WriteLine(json);
         //File.WriteAllText(@"c:\temp\test.json", json);
           
        }
    }
}
