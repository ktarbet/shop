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

               string  tag = cbtt +" " + p;

                DateTime t1 = DateTime.Now.AddDays(-2).Date;
                DateTime t2 = t1;

                HydrometDailySeries hyd = new HydrometDailySeries(cbtt, p);
                hyd.Read(t1, t2);
                hyd.RemoveMissing();

                HydrometDailySeries lrgs = new HydrometDailySeries(cbtt, p, HydrometHost.PNLinux);
                lrgs.Read(t1,t2);
                lrgs.RemoveMissing();

                if( hyd.Count != lrgs.Count )
                {
                    Console.WriteLine(tag + ": hyd.Count = "+hyd.Count+"   linux.count ="+lrgs.Count );
                }
                else
                if( hyd.Count == 1 && lrgs.Count == 1)
                {// take difference
                    var diff = hyd[0].Value - lrgs[0].Value;

                    if( Math.Abs(diff) > 0.3)
                    {
                        Console.WriteLine(i+"," +tag+", "+ hyd[0].Value.ToString("F2")+",  "+lrgs[0].Value.ToString("F2")+", " + diff.ToString("F2"));
                    }
                }



            }



        }
    }
}
