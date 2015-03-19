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

           // Logger.EnableLogger();
            var svr = PostgreSQL.GetPostgresServer("timeseries", "lrgs1");
            var db = new TimeSeriesDatabase(svr, Reclamation.TimeSeries.Parser.LookupOption.TableName);

            var sc = db.GetSeriesCatalog();
            SortFoldersByName(db, "agrimet");

           //RenameUntitled(svr, db);
         //   FixSiteID(svr, sc);
          //  FixBlankInterval(db);
           // FixFolderStructure(db,sc);

           

        }

        private static void SortFoldersByName(TimeSeriesDatabase db, string parentFolderName)
        {
            var parent = db.GetOrCreateFolder(parentFolderName);

            var sc = db.GetSeriesCatalog("parentid = "+ parent.ID+" and isfolder = 1 ",""," order by name");
            int sortOrder = 10;
            for (int i = 0; i < sc.Rows.Count; i++)
            {
                if( sc[i].siteid == "")
                 sc[i].siteid = sc[i].Name; // fix... siteid might be handy on a folder
                sc[i].SortOrder = sortOrder;
                sortOrder += 10;
            }
            db.Server.SaveTable(sc);
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
                    item.TimeInterval = TimeInterval.Irregular.ToString();
                }
            }

            int i = db.Server.SaveTable(sc);
            Console.WriteLine(i+" rows saved");
        }

        private static void FixFolderStructure( TimeSeriesDatabase db, TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc)
        {

            var siteCatalog = db.GetSiteCatalog();
            for (int i = 0; i < sc.Count; i++)
            {
                var row = sc[i];
                if (row.IsFolder || row.TimeInterval != TimeInterval.Irregular.ToString()
                    || row.Provider != "Series")
                    continue;
                TimeSeriesName tn = new TimeSeriesName(row.TableName);
                var s = db.GetSeries(row.id);
                var program = GetProgramName(siteCatalog, tn, s);
                if(program == "" || ( program != "hydromet" && program != "agrimet") )
                {
                    Console.WriteLine("Error: no program defined in series or type in sitecatalog");
                }
                else
                {
                    var myPath = sc.GetPath(row.id);
                    var myPathJoin = String.Join("/", myPath);

                    
                    string[] path = {"timeseries",program,tn.siteid,"instant"};
                    var expectedPath =String.Join("/", path);

                    if (myPathJoin != expectedPath)
                    {
                        Console.Write("existing: " + tn.GetTableName() + " " + myPathJoin);
                        Console.WriteLine(" new " + expectedPath);

                        //.String.int id = sc.GetOrCreateFolder(path);
                        //row.ParentID = id;
                    }
                    
                    
                }
                
            }

           db.Server.SaveTable(sc);

        }

        private static string GetProgramName(TimeSeriesDatabaseDataSet.sitecatalogDataTable siteCatalog, TimeSeriesName tn, Series s)
        {
            var program = s.Properties.Get("program");
            if (program == "")
            {
                Console.WriteLine("program not defined" + tn.GetTableName());
                // try site name
                var site = siteCatalog.FindBysiteid(tn.siteid);
                if (site.type != "")
                {
                    Console.WriteLine("Using type from site list: " + site.type);
                    program = site.type;
                    //  s.Properties.Set("program", site.type);
                    //s.Properties.Save(); .. not now.. might create extra network traffic to pnhyd0 during daily updates
                }
            }
            return program;
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
                if (row.siteid == "" && tn.siteid != "")
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
