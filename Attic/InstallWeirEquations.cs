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
    class InstallWeirEquations
    {
        static void Main(string[] args)
        {
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs3");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);
            var sc = db.GetSeriesCatalog();
            var prop = db.GetSeriesProperties(true);
           var hydromet = db.GetOrCreateFolder("hydromet");
            Console.WriteLine("Reading mcf ");
            var mcf = McfUtility.GetDataSetFromCsvFiles(Globals.LocalConfigurationDataPath);
            Console.WriteLine("processing ");
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

                string[] genericWeir = { "ch_weir", "gh_weir", "gh_weirx" };

                if ( Array.IndexOf(genericWeir, pcode.RTCPROC.ToLower()) >=0 )
                {
                    AddGenericWeir(sc, prop, cbtt, pc, width_factor, exponent, offset, shift);
                }
                else if (pcode.RTCPROC.ToLower() == "r_weir")
                {
                    // Rectangular Weir
                }

            }

            Console.WriteLine();
        }

        /// <summary>
        /// Add the following
        /// 
        /// cbtt_ch|gh = ""
        /// cbtt_hh|hj = ConstantShift(...)
        /// cbtt_(q|qc) = GenericWeir(....)
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="prop"></param>
        /// <param name="cbtt"></param>
        /// <param name="pc"></param>
        /// <param name="width_factor"></param>
        /// <param name="exponent"></param>
        /// <param name="offset"></param>
        /// <param name="shift"></param>
        private static void AddGenericWeir(TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc, 
            TimeSeriesDatabaseDataSet.seriespropertiesDataTable prop,
            string cbtt, string pc, 
            string width_factor, string exponent, string offset, string shift)
        {
            
            string shiftCode = "";
            string flowCode = "";
            if (pc == "ch")
            {
                shiftCode = "hh";
                flowCode = "qc";
            }
            else
            {
                shiftCode = "hj";
                flowCode = "q";
            }
            int parentID = 0; // TO DO.
            // afci_qc = GenericWeir(afci_ch,width_factor,exponent) // smart to look for shift... in afci_ch.Properties.shift
            // afci_hh = ConstantShift(afci_ch); // lookup shift from properties...
            // afci_ch.shift=-0.41 


            var id = sc.AddInstantRow(cbtt, parentID, "feet", pc, "");
            prop.Set("shift", shift, id); // save current shift in properties.
            prop.Set("program", "hydromet", id);
            sc.AddInstantRow(cbtt, parentID, "feet", shiftCode, "ConstantShift(%site%_" + pc + ")");
            prop.Set("program", "hydromet", id);

            string expression = "GenericWeir(%site%_"+pc+","+offset + "," + width_factor + "," + exponent + ")";
            id = sc.AddInstantRow(cbtt, parentID, "cfs", flowCode, expression);
            prop.Set("program", "hydromet", id);


            
        }
    }
}
