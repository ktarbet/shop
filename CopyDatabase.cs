using System.Net;
using Reclamation.TimeSeries;
using Reclamation.Core;

namespace Shop
{
    /// <summary>
    /// Copy everyting from one TimeSeries Database to another.
    /// </summary>
    class CopyDatabase
    {
        static void Main()
        {
            Logger.EnableLogger();

            //SqlServer local = new SqlServer(".\rbms", "rbms");
            SqlServer gcl = new SqlServer("ibr1gcpdb003", "Pisces");

            TimeSeriesDatabase db = new TimeSeriesDatabase(gcl);

            db.ImportCsvDump(@"C:\TEMP\rbmsdump\sitecatalog.csv",true);

        }
    }
}
