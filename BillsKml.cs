using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;



namespace Shop
{
    class BillsKml
    {
        static void Main(string[] args)
        {
            var fn = @"V:\PN6200\Hydromet\Bill's Google Places\a2.kml";

            XPathDocument doc = new XPathDocument(fn);

            var nav = doc.CreateNavigator();

            var query = "/Document/Placemark";

            var nodes = nav.Select(query);


            StreamWriter w = new StreamWriter(@"V:\PN6200\Hydromet\Bill's Google Places\hydromet.kml");
            w.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            w.WriteLine("<Document>");
            w.WriteLine("<name>Bill&apos;s Hydromet Sites</name>");


            var dict = new SortedDictionary<string, string>();

            while (nodes.MoveNext())
            {
                dict.Add(nodes.Current.SelectSingleNode("name").Value,
                    nodes.Current.SelectSingleNode("Point").Value);
                //                Console.WriteLine(nodes.Current);
            }


            foreach (var item in dict)
            {
                w.WriteLine("<Placemark>");
                w.WriteLine("   <name>" + item.Key + "</name>");
                w.WriteLine("   <Point>");
                w.WriteLine("   <coordinates>" + item.Value + "</coordinates>");
                w.WriteLine("   </Point>");
                w.WriteLine("</Placemark>");


                Console.WriteLine(item.Key + " " + item.Value);
            }
            w.WriteLine("</Document>");
            w.Close();
        }
    }
}
