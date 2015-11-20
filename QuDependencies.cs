using Reclamation.Core;
using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    class QuDependencies
    {
        static void Main(string[] args)
        {
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            TimeSeriesCalculator c = new TimeSeriesCalculator(db, TimeInterval.Daily,"","");
            
            CsvFile csv = new CsvFile(@"T:\PN6200\Hydromet\Data\rivers_canals_old.csv", CsvFile.FieldTypes.AllText);

            for (int i = 0; i < csv.Rows.Count; i++)
            {
                string siteID = csv.Rows[i]["cbtt"].ToString();
                string pcode = csv.Rows[i]["pcode"].ToString();
                var dep = c.GetDependentCalculations(siteID,pcode );

                string msg = "";
                foreach (var item in dep)
                {
                    msg +=" "+ item.Name;
                }
                if( msg != "")
                Console.WriteLine(siteID+"_"+pcode+": "+  msg);
            }

        }
    }
}
