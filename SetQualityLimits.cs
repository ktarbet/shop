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
    class SetQualityLimits
    {
        static void Main(string[] args)
        {
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);
            var sc = db.GetSeriesCatalog();
            var quality = db.Quality;
            var prop = db.GetSeriesProperties(true);
           var hydromet = db.GetOrCreateFolder("hydromet");
            Console.WriteLine("Reading mcf ");
            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);
            Console.WriteLine("processing ");
            foreach (var pcode in mcf.pcodemcf)
            {

                if (pcode.PCODE.Length < 8)
                {
                    Console.WriteLine("skipping invalid record " + pcode.PCODE);
                    continue;
                }

                if (pcode.QCSW == 0)
                    continue;

                string cbtt = pcode.PCODE.Substring(0, 8).Trim().ToLower();
                string pc = pcode.PCODE.Substring(8).Trim().ToLower();
                string[] keepers = {"q","qc","gh","ch","fb","af" };

                if (Array.IndexOf(keepers, pc) < 0)
                    continue;

                var high = pcode.QHILIM;
                var low = pcode.QLOLIM;
                var roc = pcode.QROCLIM;

                if (high == 998877 && low == 998877 && roc == 998877)
                    continue;

                if( roc == 998877)
                    roc = 0;

                Console.WriteLine(pcode.PCODE+" ("+low+","+high+") "+roc );
                var tn = "instant_"+cbtt+"_"+pc;
                quality.SaveLimits(tn, high, low, roc);
                
              
                


            }

            Console.WriteLine();
        }
    }
}
