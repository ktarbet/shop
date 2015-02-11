using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclamation.Core;
using Reclamation.TimeSeries.Nrcs;
using Reclamation.TimeSeries.Hydromet;
namespace Shop
{
    class SnowCourse
    {
        static void Main(string[] args)
        {
            Logger.EnableLogger();
            //var svr = new SQLiteServer(@"C:\temp\snowcourse.pdb");
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1.pn.usbr.gov");
            var db = new TimeSeriesDatabase(svr);


            var sc = db.GetSeriesCatalog();
            var sites = db.GetSiteCatalog();

            CsvFile csv = new CsvFile(@"C:\Users\KTarbet\Documents\project\Hydromet\ConfigurationData\su_cbtt.csv");

            for (int i = 0; i < csv.Rows.Count; i++)
            {
                var row = csv.Rows[i];
                string triplet = "16F02:ID:SNOW";
                triplet = row["nrcs code"].ToString();

                if (triplet.Trim() == "")
                    continue;
                triplet += ":" + row["state"].ToString();
                triplet += ":SNOW";

                string cbtt = row["cbtt"].ToString().ToLower();
                if (!sites.Exists(cbtt))
                {
                    sites.AddsitecatalogRow(cbtt, row["name"].ToString(), row["state"].ToString());
                }
                var monthlyFolder = db.GetOrCreateFolder(null, "hydromet", cbtt, "monthly");
                var m = new CalculationSeries(db);
                m.Name = row["name"].ToString() + "  " + cbtt + "_su";
                m.Table.TableName = "monthly_" + cbtt + "_" + row["type"].ToString();
                m.TimeInterval = TimeInterval.Monthly;
                m.Expression = "DailySnowCourseToMonthly(\""+triplet+"\")";
                var id = db.AddSeries(m,monthlyFolder);
                m.Properties.Set("program", "hydromet",id);
                m.Properties.Set("agency","nrcs", id);
                m.Properties.Set("nrcs_type", "SNOW", id);
                m.Properties.Save();
            }
            svr.SaveTable(sc);
            

        }
    }
}
