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


            CheckForDuplicates(db);
            var sc = db.GetSeriesCatalog();
            SortFoldersByName(db, "agrimet");
            //SortFoldersByName(db, "hydromet");

           //RenameUntitled(svr, db);
           // FixBlankInterval(db); // should be fixed in HydrometServer
            //FixSiteID(svr, sc);  //. should be fixed in HydrometServer

           // SetSeriesPropertyBasedOnSiteCatalog(db);

         FixFolderStructure(db,sc);

           //AssignProgramToInstant(db); // agrimet currently uses this to import
         
  



        }


        private static void CheckForDuplicates(TimeSeriesDatabase db)
        {
            string sql = @"
            select tablename,count(*) from seriescatalog where isfolder =0
group by tablename
having count(*)>1";
            var tbl = db.Server.Table("test", sql);

            if (tbl.Rows.Count > 0)
            {
                Console.WriteLine("Error, duplicates ");
                
                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    Console.WriteLine(tbl.Rows[i]["tablename"]);
                }
                throw new Exception("Error: there are "+tbl.Rows.Count+" duplicates");
            }
            
        }


        /// <summary>
        /// We are working through sereis catalog to check for program set to hydromet
        /// but the actual site in the site catalog is type=agrimet
        /// </summary>
        /// <param name="db"></param>
        private static void SetSeriesPropertyBasedOnSiteCatalog(TimeSeriesDatabase db)
        {
            var siteCatalog = db.GetSiteCatalog("type='agrimet'");
            var seriesCatalog = db.GetSeriesCatalog("timeinterval = 'Irregular' and isfolder = 0");

            for (int i = 0; i < seriesCatalog.Count; i++)
            {
                var row = seriesCatalog[i];
                var s = db.GetSeries(row.id);

                var siteRow = siteCatalog.FindBysiteid(s.SiteID);
                if (siteRow == null)
                    continue;

                var program = s.Properties.Get("program", "");
                if( program != "agrimet")
                    Console.WriteLine(s.Table.TableName+ "program='"+program+"'  site.type='"+siteRow.type+"'");

            }

            Console.WriteLine();

        }

        private static void AssignProgramToInstant(TimeSeriesDatabase db)
        {
            var siteCatalog = db.GetSiteCatalog();
            var sc = db.GetSeriesCatalog("provider = 'Series' and isfolder =0 and timeinterval='Irregular'");
          for (int i = 0; i < sc.Count; i++)
          {
              var row = sc[i];
              var s = db.GetSeries(row.id);

              var program = s.Properties.Get("program", "");

              if ( program == "")
              {
                  program = EstimateProgramName(siteCatalog, s);
                  Console.WriteLine(s.Table.TableName + " : program=" + program);
                  s.Properties.Set("program", program);
                  s.Properties.Save();
              }
          }
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
                var program = EstimateProgramName(siteCatalog, s);
                if(program == "" || ( program != "hydromet" && program != "agrimet") )
                {
                    Console.WriteLine("Error: will skip,  no program defined in series or type in sitecatalog");
                    continue;
                }

                if (!IsQualityParameter(tn.pcode))
                    continue;

                    var myPath = sc.GetPath(row.id);
                    var myPathJoin = String.Join("/", myPath);
 
                    string[] path = {"timeseries",program,tn.siteid,"instant"};
                   
                    if( IsQualityParameter( tn.pcode))
                        path = new string[]{"timeseries",program,tn.siteid,"quality"};

                    var expectedPath =String.Join("/", path);

                    if (myPathJoin != expectedPath)
                    {
                        Console.WriteLine(tn.pcode+": "+ myPathJoin+ " --> "+expectedPath );
                        var id = sc.GetOrCreateFolder(path);
                        row.ParentID = id;
                    }
            }

           db.Server.SaveTable(sc);

        }

        private static bool IsQualityParameter(string p)
        {
            string[] quality = {"power","msglen","parity","batvolt","timeerr","lenerr"};
            return Array.IndexOf(quality, p) >= 0;
        }


        private static string EstimateProgramName(TimeSeriesDatabaseDataSet.sitecatalogDataTable siteCatalog,  Series s)
        {
            var program = s.Properties.Get("program");

            if (program == "")
            {
               // Console.WriteLine("program not defined: " + tn.GetTableName());
                // try site name
                var site = siteCatalog.FindBysiteid(s.SiteID);

                if (site == null)
                {
                    Console.WriteLine("Null site "+s.SiteID);
                    return "";
                }

                if (site.type == "" || site.type == "hydromet" || site.type == "reservoir" || site.type == "weather station")
                {
                    program = "hydromet";
                }
                if (site.type == "agrimet" )
                {
                    program = "agrimet";
                }
            }
            return program;
        }


        private static void FixSiteID(BasicDBServer svr, TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc)
        {
            for (int i = 0; i < sc.Count; i++)
            {
                var row = sc[i];
                if (row.IsFolder || row.TimeInterval != TimeInterval.Irregular.ToString()
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
