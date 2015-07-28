using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reclamation.Core;

namespace Shop
{
    class IdwrShifts
    {

        static void Main(string[] args)
        
        {


// from        http://blog.codinghorror.com/cleaning-words-nasty-html/


            //if (args.Length == 0 || String.IsNullOrEmpty(args[0]))
            //{
            //    Console.WriteLine("No filename provided.");
            //    return;
            //}
            //string filepath = args[0];
            //if (Path.GetFileName(filepath) == args[0])
            //{
            //    filepath = Path.Combine(Environment.CurrentDirectory, filepath);
            //}
            //if (!File.Exists(args[0]))
            //{
            //    Console.WriteLine("File doesn't exist.");
            //}
            string filepath = FileUtility.GetTempFileName(".html");
            Web.GetFile("http://www.waterdistrict1.com/SHIFTS.htm", filepath);

            string html = File.ReadAllText(filepath);
            Console.WriteLine("input html is " + html.Length + " chars");
            html = CleanWordHtml(html);
            html = FixEntities(html);
            filepath = Path.GetFileNameWithoutExtension(filepath) + ".modified.htm";
            File.WriteAllText(filepath, html);
            Console.WriteLine("cleaned html is " + html.Length + " chars");
            Console.WriteLine(filepath);
        }
        static string CleanWordHtml(string html)
        {
            StringCollection sc = new StringCollection();
            // get rid of unnecessary tag spans (comments and title)
            sc.Add(@"<!--(w|W)+?-->");
            sc.Add(@"<title>(w|W)+?</title>");
            // Get rid of classes and styles
            sc.Add(@"s?class=w+");
            sc.Add(@"s+style='[^']+'");
            // Get rid of unnecessary tags
            sc.Add(
            @"<(meta|link|/?o:|/?style|/?div|/?std|/?head|/?html|body|/?body|/?span|![)[^>]*?>)");
            // Get rid of empty paragraph tags
            sc.Add(@"(<[^>]+>)+&nbsp;(</w+>)+");
            // remove bizarre v: element attached to <img> tag
            sc.Add(@"s+v:w+=""[^""]+""");
            // remove extra lines
            sc.Add(@"(nr){2,}");
            foreach (string s in sc)
            {
                html = Regex.Replace(html, s, "", RegexOptions.IgnoreCase);
            }
            return html;
        }
        static string FixEntities(string html)
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("\"", "&ldquo;");
            nvc.Add("\"", "&rdquo;");
            nvc.Add("Ã¢â‚¬â€œ", "&mdash;");
            foreach (string key in nvc.Keys)
            {
                html = html.Replace(key, nvc[key]);
            }
            return html;
        }
    }
}
