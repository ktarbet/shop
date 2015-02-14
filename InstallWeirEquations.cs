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
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
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

                if (pcode.RTCPROC.ToLower() == "ch_weir")
                {
                    //afci_ch+%property%.shift+1.2
                    sc.AddInstantRow(cbtt, "feet", "ch", "");
                    var id = sc.AddInstantRow(cbtt, "cfs", pc, "GenericWeir(%site%_ch+%property%.shift+"+ offset+ "," + width_factor + "," + exponent + ")");
                    // TO DO.. SAVE SHIFT may need new function
                    //var id = sc.AddInstantRow(cbtt, "feet", "hh", "%property%.shift" + width_factor + "," + exponent + ")");
                    prop.Set("shift", shift, id); // save current shift in properties.
                }
                if (pcode.RTCPROC.ToLower() == "gh_weir")
                {
                    sc.AddInstantRow(cbtt, "feet", "gh", "");
                    var id = sc.AddInstantRow(cbtt, "cfs", pc, "GenericWeir(%site%_gh+%property%.shift+" + offset + "," + width_factor + "," + exponent + ")");
                    // TO DO.. SAVE SHIFT may need new function
                    //var id = sc.AddInstantRow(cbtt, "feet", "hh", "%property%.shift" + width_factor + "," + exponent + ")");
                    prop.Set("shift", shift, id); // save current shift in properties.
                }
                if (pcode.RTCPROC.ToLower() == "gh_weirx")
                {
                    sc.AddInstantRow(cbtt, "feet", "gh", "");
                    var id = sc.AddInstantRow(cbtt, "cfs", pc, "GenericWeir(%site%_gh+%property%.shift+" + offset + "," + width_factor + "," + exponent + ")");
                    // TO DO.. SAVE SHIFT may need new function
                    //var id = sc.AddInstantRow(cbtt, "feet", "hh", "%property%.shift" + width_factor + "," + exponent + ")");
                    prop.Set("shift", shift, id); // save current shift in properties.
                }
            }

            Console.WriteLine();
        }
    }
}
