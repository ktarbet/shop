using System.Net;
using Reclamation.TimeSeries;
using Reclamation.Core;
using Reclamation.TimeSeries.Hydromet;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.POIFS.FileSystem;
using NPOI.HSSF.UserModel;

namespace Shop
{
    class WorkBench
    {
        static void Main()
        {

            string fn= @"c:\temp\ac_flow1.xls";

            var s = Reclamation.TimeSeries.Import.ImportRioGrandeExcel.ImportSpreadsheet(fn);

            s.WriteToConsole();

        }

        private static void ConvertToDailyAndSaveInDatabase()
        {
            //Date,WY1949,WY1950,WY1951,WY1952,WY1953,WY1954,WY1955,WY1956,WY1957,WY1958,WY1959,WY1960,WY1961,WY1962,WY1963,WY1964,WY1965,WY1966,WY1967,WY1968,WY1969,WY1970,WY1971,WY1972,WY1973,WY1974,WY1975,WY1976,WY1977,WY1978,WY1979,WY1980,WY1981,WY1982,WY1983,WY1984,WY1985,WY1986,WY1987,WY1988,WY1989,WY1990,WY1991,WY1992,WY1993,WY1994,WY1995,WY1996,WY1997,WY1998,WY1999,WY2000,WY2001,WY2002,WY2003,WY2004,WY2005,WY2006,WY2007,WY2008,WY2009,WY2010,WY2011,WY2012,WY2013,WY2014,WY2015

            SQLiteServer svr = new SQLiteServer(@"C:\temp\test.pdb");
            TimeSeriesDatabase db = new TimeSeriesDatabase(svr);
            string fn = @"c:\temp\HEII1_QINE.ESPF10.csv";
            CsvFile csv = new CsvFile(fn);
            Series merged = new Series();
            for (int i = 1; i < csv.Columns.Count; i++)
            {
                DataTableSeries s = new DataTableSeries(csv, TimeInterval.Irregular, "Date", csv.Columns[i].ColumnName);
                s.Read();
                merged.Add(Math.ShiftToYear(s,1995));

                var daily = Reclamation.TimeSeries.Math.DailyAverage(s);
                daily.Table.TableName = csv.Columns[i].ColumnName;
                daily.Name = daily.Table.TableName;
                db.AddSeries(daily);
            }

            var scenarios = db.GetScenarios();
            for (int yr = 1949; yr < 2015; yr++)
            {
                scenarios.AddScenarioRow(yr.ToString(), yr == 1949, yr.ToString());
            }
            db.Server.SaveTable(scenarios);

            //Series s = new ExcelDataReaderSeries(csv, "HEII1_QINE.ESPF10", "Date", col[i], "kcfs");
            //s.Read();
            //s.SiteID = "Snake River nr Heise";
            //s.Table.TableName = col[i];
            //db.AddSeries(s);
        }

        private static void DownloadRatingsFromPnhyd0()
        {
            TextFile tf = new TextFile(@"c:\temp\rtf_list.dat");

            for (int i = 0; i < tf.Length; i++)
            {
                if (tf[i].Length < 14)
                    continue;
                var cbtt = tf[i].Substring(6, 8).ToLower().Trim();
                var pcode = tf[i].Substring(14, 7).ToLower().Trim();

                if (pcode.Trim() == "")
                    continue;


                string path = @"C:\temp\rating_tables";

                var fn = Path.Combine(path, cbtt + "_" + pcode + ".csv");
                var fn2 = Path.Combine(path, cbtt + ".csv");

                if (File.Exists(fn) || File.Exists(fn2))
                {
                    System.Console.WriteLine("File exists <skipping> :" + cbtt + " " + pcode);
                    continue;
                }

                //var rt = HydrometInfoUtility.GetRatingTable(cbtt, pcode);
                string url = "http://www.usbr.gov/pn-bin/expandrtf.pl?site=pali&pcode=q&form=csv";
                url = "http://hydromet.pn.usbr.gov/~dataaccess/expandrtf.exe?site=pali&pcode=q&shift&form=csv";

                url = url.Replace("site=pali", "site=" + cbtt.Trim());
                url = url.Replace("pcode=q", "pcode=" + pcode.Trim());

                string[] data = Web.GetPage(url);


                TextFile tf2 = new TextFile(data);

                if (pcode == "q")
                {
                    fn = fn2; // short filename
                }

                System.Console.WriteLine("Saving: " + fn);
                tf2.SaveAs(Path.Combine(path, fn));

                // test 

                var rt = new TimeSeriesDatabaseDataSet.RatingTableDataTable();
                rt.ReadFile(fn);
            }
        }

        private static void CopyDatabase()
        {
            Logger.EnableLogger();

            //SqlServer local = new SqlServer(".\rbms", "rbms");
            SqlServer gcl = new SqlServer("ibr1gcpdb003", "Pisces");

            TimeSeriesDatabase db = new TimeSeriesDatabase(gcl);

            db.ImportCsvDump(@"C:\TEMP\rbmsdump\sitecatalog.csv", true);
        }
    }
}
