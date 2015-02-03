using Reclamation.Core;
using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    class CleanupSeriesCatalog
    {
        static void Main(string[] args)
        {

         //   Console.WriteLine(DateTime.Now.Ticks);


            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            var sc = db.GetSeriesCatalog();

           //RenameUntitled(svr, db);
            // FixSiteID(svr, sc);
            FixBlankInterval(db);
           

        }

        private static void FixBlankInterval(TimeSeriesDatabase db)
        {
            var sc = db.GetSeriesCatalog("timeinterval = '' and isfolder = 0");
            Console.WriteLine(sc.Count+" series with no interval" );
            foreach (var item in sc)
            {
                
                TimeSeriesName tn = new TimeSeriesName(item.TableName);

                if (tn.interval == "instant")
                {
                    Console.Write(item.Name+" "+item.TableName);
                    Console.WriteLine(" Setting interval to "+TimeInterval.Irregular);
                }
            }

            db.Server.SaveTable(sc);
        }

        private static void FixFolderStructure(TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc)
        {

            for (int i = 0; i < sc.Count; i++)
            {
                var row = sc[i];
                if (row.IsFolder || row.TimeInterval != TimeInterval.Irregular.ToString()
                    || row.Provider != "Series")
                    continue;

                var parent = sc.GetParent(row);
                var grandParent = sc.GetParent(parent);
                // parent should be instant, then grandparent should be sitename
                TimeSeriesName tn = new TimeSeriesName(row.TableName);

                if (parent.Name != "instant" || grandParent.Name != row.siteid)
                {
                    //Console.WriteLine(i + "  " + row.Name + " parent = " + parent.Name + " grandparent = " + grandParent.Name);
                    // find grandparent (siteid)
                    var site = sc.Select("name = '" + tn.siteid + "'");
                    if (site.Length != 1)
                        Console.WriteLine("Error: folder " + tn.siteid + " not found");
                    else
                    {// put items in the folder.
                        // look for instant and daily folders
                        var site2 = site[0] as Reclamation.TimeSeries.TimeSeriesDatabaseDataSet.SeriesCatalogRow;
                        var instant = sc.Select("parentid =" + site2.id + " and name = 'instant' ");
                        int instantid = -1;
                        if (instant.Length == 0)
                        {
                            Console.WriteLine("creating folder " + site2.Name + "/instant");
                            instantid = sc.AddFolder("instant", site2.id);
                        }
                        else
                        {
                            instantid = Convert.ToInt32(instant[0]["id"]);
                            if (row.ParentID != instantid)
                            {
                                Console.WriteLine(i + "  " + row.Name + " parent = " + parent.Name + " grandparent = " + grandParent.Name);

                                Console.WriteLine(" instant id = " + instantid);
                                row.ParentID = instantid;
                            }
                        }


                    }
                }

            }

            //svr.SaveTable(sc);

        }


        private static void FixSiteID(BasicDBServer svr, TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc)
        {
            for (int i = 0; i < sc.Count; i++)
            {
                var row = sc[i];
                if (row.IsFolder || row.TimeInterval != "" //TimeInterval.Irregular.ToString()
                    || row.Provider != "Series")
                    continue;

                TimeSeriesName tn = new TimeSeriesName(row.TableName);
                if (row.siteid == "")
                {
                    Console.WriteLine("Site ID is blank. chaning to "+ "  " + tn.siteid);
                    row.siteid = tn.siteid;
                }

            }

            svr.SaveTable(sc);
        }

        private static void RenameUntitled(BasicDBServer svr, TimeSeriesDatabase db)
        {
            var untitled = db.GetSeriesCatalog("name = 'untitled'");
            for (int i = 0; i < untitled.Count; i++)
            {
                var item = untitled[i];
                
                TimeSeriesName tn = new TimeSeriesName(item.TableName);
                Console.WriteLine("Renaming '"+item.Name+"'  to "+tn.siteid + "_" + tn.pcode);

                item.Name = tn.siteid + "_" + tn.pcode;
                Console.WriteLine(item.Name + " " + item.TableName + " " + item.TimeInterval);
            }

            svr.SaveTable(untitled);
        }
    }
}
