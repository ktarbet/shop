using Reclamation.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateRFCList
{
    class CreateRFCList
    {

        static void Main(string[] args)
        {
            var pcode = new CsvFile(@"v:\PN6200\Hydromet\ConfigurationData\pcode.csv", CsvFile.FieldTypes.AllText);
            DataTable site = new CsvFile(@"v:\PN6200\Hydromet\ConfigurationData\site.csv", CsvFile.FieldTypes.AllText);
            /*
              RENAME/LOG custom$:[crohms.data]BOIA.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]BURL.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]IDWD.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]UMAT.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]YAKR.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]ROGU.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]WILM.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]DESU.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]MCOL.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]FLAT.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]WQAL.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]SUGS.CRO custom$:[crohms.data]*.SN1
$       RENAME/LOG custom$:[crohms.data]WMCO.CRO custom$:[crohms.data]*.SN1
$       COPY
            */
            site = DataTableUtility.Select(site, @" (STATYPE = 'YAKR' or STATYPE = 'BURL'   
            or STATYPE = 'UMAT'   
            or STATYPE = 'YAKR'   
            or STATYPE = 'ROGU'   
            or STATYPE = 'WILM'   
            or STATYPE = 'DESU'   
            or STATYPE = 'FLAT')   
            and CATID <> 'OFF' ", 
                                                     
                                                     "");
            //Console.WriteLine(site.Rows.Count+" sites " );
            DataRow[] rows;
            foreach (DataRow s in site.Rows)
            {
                string siteName = s["site"].ToString();
                int i = -1;
                foreach (var pc in GetPcodes(pcode, siteName,out rows))
	            {
                    i++;
                    string timeZone = "M";
                    if (s["TZONE"].ToString().Trim() == "8")
                        timeZone = "P";

                    var sc = GetShefcode(pc);
                    if (sc == "" )
                        continue;
                    Console.WriteLine(siteName.Trim() + "\t" + pc + "\t" +timeZone+"\t"+ rows[i]["DESCR"].ToString() + "\t" + s["DESCR"].ToString());
                   
                }

            }


        }

           static string[] hydrometCode = new String[]{"PC","OB","GH","FB","AF","Q","QC","QT","SE","QE","QZ","CH"};

           static string[] shefCode =     new string[]{"PC","TA","HG","HP","LS","QR","QD","QT","SI","QG","QU","HG"};

           private static string GetShefcode(string pcode)
           {
               var idx = Array.IndexOf(hydrometCode, pcode);
               if (idx >= 0)
                   return shefCode[idx];
               return "";
           }

        private static string[] GetPcodes(CsvFile pcode, string siteName, out DataRow[] rows)
        {
            List<string> rval = new List<string>();
            
             rows = pcode.Select("PCODE like '" + siteName + " %'");


            for (int i = 0; i < rows.Length; i++)
            {
                string a = rows[i]["pcode"].ToString();
                if (a.Substring(0, 8).Trim() != siteName)
                    rval.Add("");

                rval.Add(a.Substring(8).Trim());
            }

            return rval.ToArray();

        }
    }
}
