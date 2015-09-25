using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclamation.Core;
using Reclamation.TimeSeries.Hydromet;
using Math = Reclamation.TimeSeries.Math;
using System.IO;
using System.Data;

namespace Shop
{
    class UsgsWillacker
    {
        static void Main(string[] args)
        {

            string dir = @"V:\PN6200\Hydromet\DataRequests\usgs\james_willacker\";
            var fnxls = Path.Combine(dir, "PNW_reservoirs_request.xlsx");
            var pdb = Path.Combine(dir, "willacker.pdb");
            SQLiteServer svr = new SQLiteServer(pdb);
            TimeSeriesDatabase db = new TimeSeriesDatabase(svr);
             //CreatePiscesDatagbase(fnxls, db);

            DataTable tbl = new DataTable("res");
            tbl.Columns.Add("cbtt");
            tbl.Columns.Add("Year");
            tbl.Columns.Add("Average.AF");
            tbl.Columns.Add("Average.FB");
            tbl.Columns.Add("Min.AF");
            tbl.Columns.Add("Min.FB");
            tbl.Columns.Add("Min.date");
            tbl.Columns.Add("Max.AF");
            tbl.Columns.Add("Max.FB");
            tbl.Columns.Add("Max.date");
            tbl.PrimaryKey = new DataColumn[]{ tbl.Columns["cbtt"],tbl.Columns["year"]};

            var sc = db.GetSeriesCatalog();

            foreach (var item in sc)
            {
                if (item.IsFolder)
                    continue;
                var s = db.GetSeries(item.id) as HydrometDailySeries;
                s.Read();
                Console.WriteLine(s.Parameter+ " "+s.SiteID);
                var pc = s.Parameter;
                MonthDayRange rng = new MonthDayRange();
                var avg = Math.AnnualAverage(s, rng, 10);
                var max = Math.AnnualMax(s, rng, 10);
                var min = Math.AnnualMin(s, rng, 10);

                Add(tbl,s.SiteID, avg,s.SiteID, "Average."+pc,"");
                Add(tbl, s.SiteID, max, s.SiteID, "Max."+pc, "Max.date");
                Add(tbl, s.SiteID, min, s.SiteID, "Min." + pc, "Min.date");
            }

            tbl.WriteXml(Path.Combine(dir, "tbl.xml"));
            CsvFile.WriteToCSV(tbl, Path.Combine(dir, "willacker.csv"));
        }

        /// <summary>
        /// For each water year add entry to table.
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="s"></param>
        /// <param name="siteID"></param>
        /// <param name="colName"></param>
        private static void Add(DataTable tbl, string cbtt,Series s, 
            string siteID, string colName, string dateTagColumn="")
        {

            foreach (var pt in s)
            {
                var rows = tbl.Select("cbtt = '" + siteID + "' and year = '" + pt.DateTime.WaterYear() + "'");
                DataRow row;
                if (rows.Length == 0)
                {
                    row = tbl.NewRow();
                    row["cbtt"] = cbtt;
                    row["year"] = pt.DateTime.WaterYear();
                    tbl.Rows.Add(row);
                }
                else
                    row = rows[0];

                row[colName] = pt.Value;

                if( dateTagColumn != "")
                {
                    row[dateTagColumn] = pt.DateTime;
                }

            }
            

        }

        private static void CreatePiscesDatagbase(string fnxls, TimeSeriesDatabase db)
        {
            var xls = new NpoiExcel(fnxls);

            var tbl1 = xls.ReadDataTable(0);
            for (int i = 0; i < tbl1.Rows.Count; i++)
            {
                var cbtt = tbl1.Rows[i]["siteid"].ToString();
                Console.WriteLine(cbtt);
                DateTime t1 = DateTime.Parse("1950-01-01");
                DateTime t2 = DateTime.Now.Date;

                var af = HydrometDailySeries.Read(cbtt, "af", t1, t2, HydrometHost.PN);
                db.AddSeries(af);
                var fb = HydrometDailySeries.Read(cbtt, "fb", t1, t2, HydrometHost.PN);
                db.AddSeries(fb);


            }
        }
    }
}

