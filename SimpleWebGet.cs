using System.Net;

namespace Shop
{
    class SimpleWebGet
    {
        static void Main()
        {
            string data;
            //string url = "http://www.google.com";
            string url = "http://ibr3lcrxcn01.bor.doi.net:8080/HDB_CGI.com?sdi=2214&tstp=DY&syer=2015&smon=1&sday=1&eyer=2015&emon=5&eday=8&format=3";

            using (WebClient client = new WebClient())
            {
                data = client.DownloadString(url);
            }

            System.Console.WriteLine(data);
        }
    }
}
