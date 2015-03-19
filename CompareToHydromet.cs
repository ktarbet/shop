using Reclamation.Core;
using Reclamation.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop
{
    class CompareToHydromet
    {

        static void Main()
        {
            string serverIP = "lrgscma";
            var cs = PostgreSQL.CreateADConnectionString(serverIP, "timeseries");
            PostgreSQL svr = new PostgreSQL(cs);
            TimeSeriesDatabase db = new TimeSeriesDatabase(svr);


        }
    }
}
