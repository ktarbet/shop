using Reclamation.Core;
using Reclamation.TimeSeries;
using Reclamation.TimeSeries.Hydromet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    /// <summary>
    /// Loads PN data in to a series catalog in a Pisces database for use reclamation wide
    /// 
    /// how?
    /// Use agrimet as is, with pn_prefix in table names
    /// use hydromet reservoirs tagged public_visible="true"
    /// 
    /// Get Site catalog all public_visible....
    /// 
    /// </summary>
    class LoadMySqlPisces
    {
        static void Main(string[] args)
        {

            var svr_vm = MySqlServer.GetMySqlServer("vm", "timeseries");
            svr_vm.RunSqlCommand("truncate seriescatalog");
            svr_vm.RunSqlCommand("truncate sitecatalog");

            var db_vm = new TimeSeriesDatabase(svr_vm, Reclamation.TimeSeries.Parser.LookupOption.TableName);
            var sites_vm = db_vm.GetSiteCatalog();

            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);
            var sites = db.GetSiteCatalog();

            string program = "agrimet";
            var sc = db.GetSeriesCatalog("timeinterval='Daily'","program:"+program);
            var sc_vm = db_vm.GetSeriesCatalog();

            LoadDailyUsbrCatalog(sc, sc_vm,program);
            //db_vm.Server.SaveTable(sc_vm);

            LoadUpperSnakeHydromet(sc_vm);

            db_vm.Server.SaveTable(sc_vm);
            // install all sites that are referenced in series catalog

            var tmp = db_vm.Server.Table("tmp", "select distinct siteid from seriescatalog");
            for (int i = 0; i < tmp.Rows.Count; i++)
            {
                string siteid = tmp.Rows[i]["siteid"].ToString();
                var rows = sites.Select("siteid='" + siteid + "'");
                if (rows.Length > 0)
                {
                    var newRow = sites_vm.NewsitecatalogRow();
                    newRow.ItemArray = rows[0].ItemArray;
                    sites_vm.Rows.Add(newRow);
                }

            }

            db_vm.Server.SaveTable(sites_vm);
        }

        private static void LoadUpperSnakeHydromet(TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc_vm)
        {
            // load hydromet/upper snake
            var lines = upperSnakeDaily.Split('\n');
            foreach (var item in lines)
            {
                var tokens = item.Trim().Split(' ', '\t');
                var cbtt = tokens[0].Trim().ToLower();
                var pcode = tokens[1].Trim().ToLower();
                string[] path = { "water.usbr.gov", "pn", "hydromet", cbtt, "daily" };
                var folderID = sc_vm.GetOrCreateFolder(path);
                HydrometDailySeries s = new HydrometDailySeries(cbtt, pcode);
                s.SiteID = cbtt;
                s.Parameter = HydrometInfoUtility.LookupDailyParameterName(pcode);
                sc_vm.AddSeriesCatalogRow(s, sc_vm.NextID(), folderID, "pnhydromet_" + s.Table.TableName);
            }
        }

        private static void LoadDailyUsbrCatalog(TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc, 
            TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc_vm, string program)
        {
            for (int i = 0; i < sc.Rows.Count; i++)
            {
                if (i == 55)
                    return;
                var row = sc[i];
                string[] path = { "water.usbr.gov", "pn", program, row.siteid, "daily" };
                var folderID = sc_vm.GetOrCreateFolder(path);
                TimeSeriesName tn = new TimeSeriesName(row.TableName);

                var newRow = sc_vm.NewSeriesCatalogRow();
                newRow.ItemArray = row.ItemArray;
                newRow.id = sc_vm.NextID();
                newRow.iconname = "";
                newRow.Provider = "HydrometDailySeries";
                newRow.ConnectionString = "server=PN;cbtt=" + tn.siteid + ";pcode=" + tn.pcode + ";";
                newRow.TableName = "pnhydromet_" + row.TableName;

                newRow.Units = HydrometInfoUtility.LookupDailyUnits(tn.pcode);
                newRow.Parameter = HydrometInfoUtility.LookupDailyParameterName(tn.pcode);
                newRow.ParentID = folderID;
                sc_vm.Rows.Add(newRow);

            }
        }

        static string upperSnakeDaily=
@"MIL	AF
MIN	AF
AMF	AF
RIR	AF
PAL	AF
JCK	AF
GRS	AF
HEN	AF
ISL	AF
WOD	AF
MIL	FB
MIN	FB
AMF	FB
RIR	FB
PAL	FB
JCK	FB
GRS	FB
HEN	FB
ISL	FB
WOD	FB
JCK	QD
RIRI	QD
REXI	QD
LORI	QD
HEII	QD
TEAI	QD
SIFI	QD
SHYI	QD
SNAI	QD
BFTI	QD
GREY	QD
ALPY	QD
LWOI	QD
WODI	QD
MILI	QT
MINI	QD
AMFI	QD
ISLI	QD
HENI	QD
PALI	QD
SALY	QD
FLGY	QD
JKSY	QD";


    }
}
