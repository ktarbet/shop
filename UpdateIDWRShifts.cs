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
    class UpdateIDWRShifts

    {

        static string[] idwrSiteList = new string[] {
            "AFCI",
"ANCI",
"ASCI",
"AUCI",
"BFCI",
"BMCI",
"BOOI",
"BURI",
"CBCI",
"CECI",
"CFCI",
"CRCI",
"CXCI",
"CXMI",
"DNCI",
"EGCI",
"ENTI",
"ERCI",
"FARI",
"FFCI",
"FMCI",
"FRCI",
"GWCI",
"HARI",
"IDCI",
"INCI",
"ISCI",
"LABI",
"LACI",
"MIII",
"MRYI",
"MXCI",
"NLCI",
"OSCI",
"PECI",
"PLCI",
"POCI",
"PRKI",
"RDWI",
"RECI",
"RGCI",
"RICI",
"RSDI",
"RXRI",
"SNDI",
"SUCI",
"TCNI",
"TCSI",
"TGCI",
"TICI",
"TITI",
"TLCI",
"TRCI",
"VLCI",
"WACI",
"WFCI",
"WVCI"

            };

        static void Main(string[] args)
        {
            Logger.EnableLogger();

            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);
            var sc = db.GetSeriesCatalog();
            var prop = db.GetSeriesProperties(true);
            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);
            Console.WriteLine("processing ");

            for (int i = 0; i < idwrSiteList.Length; i++)
            {
                var cbtt = idwrSiteList[i];
                Console.WriteLine(cbtt);
                var rows = mcf.pcodemcf.Select("PCODE='" + cbtt.PadRight(8) + "CH'");
                if( rows.Length ==1)
                {
                    var shift = Convert.ToDouble(rows[0]["SHIFT"]);
                    Console.WriteLine(shift);

                    TimeSeriesName tn = new TimeSeriesName(cbtt.ToLower() + "_" + "ch", "instant");
                    Reclamation.TimeSeries.TimeSeriesDatabaseDataSet.seriespropertiesDataTable.Set("shift", shift.ToString("F4"), tn, svr);

                }



            }
            return;

            foreach (var pcode in mcf.pcodemcf)
            {

                if (pcode.IsRTCPROCNull())
                    continue;

                Console.WriteLine(pcode.PCODE);
                string cbtt = pcode.PCODE.Substring(0, 8).Trim().ToLower();
                string pc = pcode.PCODE.Substring(8).Trim().ToLower();
                
                var width_factor = pcode.SCALE.ToString("F4");
                var exponent = pcode.BASE.ToString("F4");
                var offset = pcode.OFFSET.ToString("F4");
                var shift = pcode.SHIFT.ToString("F4");


            }

            Console.WriteLine();
        }

         

    }
}
