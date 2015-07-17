using Reclamation.Core;
using Reclamation.TimeSeries;
using Reclamation.TimeSeries.Hydromet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;


namespace Shop
{
    class MergePcodeFromPisces
    {
        static void Main(string[] args)
        {
            // merge pisces database info into pcode.csv

            // Read CSv file.

            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);

            // Read Series catalog
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);
            var sc = db.GetSeriesCatalog();

            // insert records not allready in pcode.csv
            for (int i = 0; i < sc.Rows.Count; i++)
            {

                TimeSeriesDatabaseDataSet.SeriesCatalogRow r = sc[i];
                if (r.IsFolder)
                    continue;

            }


        }
    }
}
