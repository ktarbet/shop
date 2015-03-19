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
            var db_vm = new TimeSeriesDatabase(svr_vm, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            var sc = db.GetSeriesCatalog("timeinterval='Daily'","program:agrimet");
            var sc_vm = db_vm.GetSeriesCatalog();

            for (int i = 0; i < sc.Rows.Count; i++)
            {
                var row = sc[i];
                string[] path = {"water.usbr.gov","pn","agrimet",row.siteid,"daily"};
                var folderID = sc_vm.GetOrCreateFolder(path);
                TimeSeriesName tn = new TimeSeriesName(row.TableName);

                var newRow = sc_vm.NewSeriesCatalogRow();
                newRow.ItemArray = row.ItemArray;
                newRow.id = sc_vm.NextID();
                newRow.iconname = "";
                newRow.Provider = "HydrometDailySeries";
                newRow.ConnectionString = "server=PN;cbtt="+tn.siteid +";pcode="+tn.pcode+";";
                newRow.TableName = "pnhydromet_" + row.TableName;
                
                newRow.Units = HydrometInfoUtility.LookupArchiveUnits(tn.pcode);
                newRow.Parameter = HydrometInfoUtility.GetParameterName(tn.pcode);
                newRow.ParentID = folderID;
                sc_vm.Rows.Add(newRow);
            }

            db_vm.Server.SaveTable(sc_vm);


        }

    }
}
