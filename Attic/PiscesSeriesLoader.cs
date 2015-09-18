using Reclamation.Core;
using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    class PiscesSeriesLoader
    {

        //PostgreSQL svr;
        TimeSeriesDatabase db;
        TimeSeriesDatabaseDataSet.SeriesCatalogDataTable seriesCatalog;

        public TimeSeriesDatabaseDataSet.SeriesCatalogDataTable SeriesCatalog
        {
            get { return seriesCatalog; }
            //set { seriesCatalog = value; }
        }
        TimeSeriesDatabaseDataSet.seriespropertiesDataTable seriesProperties;

        string m_program;
        public PiscesSeriesLoader(TimeSeriesDatabase db, string program="")
        {
            this.db = db;
            seriesCatalog = db.GetSeriesCatalog();
            seriesProperties = db.GetSeriesProperties();
            m_program = program;
        }


        /// <summary>
        /// Adds cbtt/pcode to pisces, with appropirate equations.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="cbtt"></param>
        /// <param name="pc"></param>
        public void AddToPisces(int parentID, string cbtt, string pc)
        {
            if (Array.IndexOf(new string[] { "ch", "gh" }, pc) >= 0)
                AddInstantRow(cbtt, parentID, "feet", pc);
            else
                if (Array.IndexOf(new string[] { "q" }, pc) >= 0)
                {
                    AddInstantRow(cbtt, parentID, "cfs", "q", "FileRatingTable(%site%_gh,\"%site%.csv\")");
                    AddInstantRow(cbtt, parentID, "feet", "hj", "FileRatingTable(%site%_gh,\"%site%_shift.csv\")");
                }
                else
                    if (Array.IndexOf(new string[] { "qc" }, pc) >= 0)
                    {
                        AddInstantRow(cbtt, parentID, "cfs", "qc", "FileRatingTable(%site%_ch,\"%site%.csv\")");
                        AddInstantRow(cbtt, parentID, "feet", "hh", "FileRatingTable(%site%_ch,\"%site%_shift.csv\")");
                    }
                    else
                        if (Array.IndexOf(new string[] { "wf", "wf2" }, pc) >= 0)
                        {
                            AddInstantRow(cbtt, parentID, "degF", pc, "");
                        }
                        else
                            if (Array.IndexOf(new string[] { "bv" }, pc) >= 0)
                            {
                                AddInstantRow(cbtt, parentID, "volt", pc, "");
                            }
                            else
                                if (Array.IndexOf(new string[] { "ob" }, pc) >= 0)
                                {
                                    AddInstantRow(cbtt, parentID, "degF", pc, "");
                                }
                                else
                                    if (Array.IndexOf(new string[] { "zs" }, pc) >= 0)
                                    {
                                        AddInstantRow(cbtt, parentID, "", pc, "");
                                    }
                                    else
                                        if (Array.IndexOf(new string[] { "fb" }, pc) >= 0)
                                        {
                                            AddInstantRow(cbtt, parentID, "feet", pc, "");
                                            // AF??
                                        }
                                        else
                                            if (Array.IndexOf(new string[] { "pc" }, pc) >= 0)
                                            {
                                                AddInstantRow(cbtt, parentID, "inches", pc, "");
                                                // AF??
                                            }
                                            else
                                            {
                                                Console.WriteLine(pc + "not defined");
                                            }
        }

        public void AddInstantRow(string siteID, int parentid, string units, string pcode, string expression = "")
        {
            var id = this.seriesCatalog.AddInstantRow(siteID, parentid, units, pcode, expression);
            if (m_program != "")
                seriesProperties.AddseriespropertiesRow(seriesProperties.NextID(), id, "program", m_program);

        }
    }
}
