using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclamation.TimeSeries.Hydromet;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            // compare daily data in linux server with vms

            var items = File.ReadAllLines("list.txt");
/*
daily_bsei_qj
daily_bppi_qj
daily_bouy_pp
*/
            for (int i = 0; i < items.Length; i++)
            {
                var tokens = items[i].Split('_');
                if( tokens.Length != 3)
                {
                    Console.WriteLine("Error: "+items[i]+" not valid");
                }

                string cbtt = tokens[1];
                string p = tokens[2];

                Console.WriteLine(cbtt +" " + p);

                DateTime t = DateTime.Now.AddDays(-1).Date;
                HydrometDailySeries hyd = new HydrometDailySeries(cbtt, p);
                hyd.Read(t, t);
                hyd.WriteToConsole();


                HydrometDailySeries lrgs = new HydrometDailySeries(cbtt, p, HydrometHost.PNLinux);
                lrgs.Read(t,t);
                lrgs.WriteToConsole();

            }



        }
    }
}
