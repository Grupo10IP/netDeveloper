using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using ConnectionDBNS;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto;
using WindowsFormsApp1;

namespace BOEAppNS
{
    static class RemotePdfFile
    {

        const String TEXT_TO_FIND =  "Constitución.  Comienzo de operaciones: ";

        public static int toDB(ConnectionDB dbConn, List<Tuple<BOEEntity, List<CompanyEntity>>> list)
        {
            
            List<MySqlParameter> parms=new List<MySqlParameter>();
            List<MySqlParameter> parms2=new List<MySqlParameter>();
            List<MySqlParameter> parms3 = new List<MySqlParameter>();

            parms.Add(new MySqlParameter("name", MySqlDbType.VarChar));

            parms2.Add(new MySqlParameter("name", MySqlDbType.VarChar));
            parms2.Add(new MySqlParameter("anno", MySqlDbType.Int32));
            parms2.Add(new MySqlParameter("date", MySqlDbType.VarChar));
            parms2.Add(new MySqlParameter("urlpath", MySqlDbType.VarChar));
            parms2.Add(new MySqlParameter("provinceid", MySqlDbType.Int32));
            parms2.Add(new MySqlParameter("filecontent", MySqlDbType.VarChar));

            parms3.Add(new MySqlParameter("enterprise", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("date", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("address", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("capital", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("socialobject", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("unicpartner", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("unicadmin", MySqlDbType.VarChar));
            parms3.Add(new MySqlParameter("registraldata", MySqlDbType.VarChar));

            dbConn.Open();

            var trans = dbConn.conn.BeginTransaction();
            try
            {

                for (int i = 0; i < list.Count; i++)
                {

                    BOEEntity boe = list[i].Item1;
                    MySqlDataReader red = dbConn.ExecuteReader("SELECT id FROM provinces WHERE name='" + boe.province.ToString().Trim() + "'");
                    int id;
                    if (red.Read())
                    {
                        id = (int)red.GetUInt32(0);
                        red.Close();
                    }
                    else
                    {
                        red.Close();
                        parms[0].Value = boe.province.ToString();
                        MySqlCommand com = dbConn.ExecuteNonQueryParams("INSERT INTO provinces (name) values (@name)", parms);
                        id = (int)com.LastInsertedId;
                    }


                    // mira si ya existe este BOE
                    MySqlDataReader red2 = dbConn.ExecuteReader("SELECT id FROM boes WHERE name='" + boe.nbo.ToString().Trim() + "'");
                    if (red2.HasRows)
                    {
                        red2.Close();
                        continue;
                    }
                    red2.Close();

                    // añado el nuevo BOE
                    parms2[0].Value = boe.nbo;
                    parms2[1].Value = Convert.ToInt32(boe.anno);
                    parms2[2].Value = boe.date;
                    parms2[3].Value = boe.urlpdf.ToString();
                    parms2[4].Value = id;
                    parms2[5].Value = "";
                    MySqlCommand com2 = null;
                    try
                    {
                        com2 = dbConn.ExecuteNonQueryParams("INSERT INTO boes (name, anno, date, urlpath, provinceid, filecontent) values (@name, @anno, @date, @urlpath, @provinceid, @filecontent)", parms2);
                    }
                    catch (Exception e)
                    {
                        SimpleLogger.Error("remotePdfFile() toB() ExecuteNonQueryParams() Error: " + e.Message);
                        return -1;
                    }
                    int id2 = (int)com2.LastInsertedId;

                    for (int h = 0; h < list[i].Item2.Count; h++)
                    {
                        // añado el nuevo BOE Details
                        parms3[0].Value = list[i].Item2[h].Name;
                        parms3[1].Value = list[i].Item2[h].Date; //boe.date;
                        parms3[2].Value = list[i].Item2[h].Address;
                        parms3[3].Value = list[i].Item2[h].Capital;
                        parms3[4].Value = list[i].Item2[h].SocialObject;
                        parms3[5].Value = list[i].Item2[h].UnicPartner;
                        parms3[6].Value = list[i].Item2[h].UnicAdmin;
                        parms3[7].Value = list[i].Item2[h].RegistralData;
                        try
                        {
                            MySqlCommand com3 = dbConn.ExecuteNonQueryParams(
                                "INSERT INTO boedetails (enterprise, date, address, capital, socialobject, unicpartner, unicadmin, registraldata, boeid) " +
                                " values " +
                                "(@enterprise, @date, @address,  @capital, @socialobject, @unicpartner, @unicadmin, @registraldata, " + Convert.ToString(id2) + ")",
                                parms3);
                            int id4 = (int)com3.LastInsertedId;
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }

                }
                trans.Commit();

            }
            catch(Exception ex)
            {
                SimpleLogger.Error("remotePdfFile() toDB() ExecuteNonQueryParams() Error: " + ex.Message);
                trans.Rollback();
                return -1;
            }
            finally
            {
                dbConn.Close();
            }

            return 0;
        }

        private static String parse(String url, ref int errorCode)
        {
            String content = "";
            errorCode = -1;

            String tempFile = Path.GetTempPath() + @"\tempfile";
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, tempFile);
                }

                PDFParser pdfParser = new PDFParser();
                if (pdfParser.ExtractTextToString(tempFile, ref content))
                    errorCode = 0;
            }
            catch (Exception e)
            {
                SimpleLogger.Error("remotePdfFile() parse() Error: " + e.Message);
            }

            return content;
        }

        private static String copyTo(ref int pos, int next, String source, String toString)
        {
            String res = "";
            try
            {
                if ((source.IndexOf(toString, pos)==-1) || (source.IndexOf(toString, pos)>=next))
                {
                    pos = pos + 1;
                    return res;
                }
                String aux = source.Substring(pos, source.IndexOf(toString, pos) - pos);
                pos = pos + aux.Length + toString.Length;
                res = aux.Trim();
            } 
            catch(Exception e)
            {
                SimpleLogger.Error("remotePdfFile() copyTo() Error: " + e.Message);
            }

            return res;
        }

        private static String getNextEntry(ref int pos, int next, String source, String entry, String[] toString)
        {
            String res = "";
            try
            {
                int ps = source.IndexOf(entry, pos);

                if (ps==-1) 
                    source.IndexOf(entry.ToUpper(), pos);

                if ((ps == -1) || ((ps >= next) && (next != -1)))
                {
                    return "";
                }

                int j = 0;
                int ps2 = -1;

                if (toString==null)
                    ps2 = source.Length;
                else 
                    while (j < toString.Length)
                    {
                        ps2 = source.IndexOf(toString[j], ps + 1);
                        if (ps2 != -1) break;
                        j++;
                    }

                if (ps2 == -1) return "";

                String aux = source.Substring(ps + entry.Length, ps2 - (ps + entry.Length));
                pos = pos + aux.Length;
                res = aux.Trim();
            }
            catch (Exception e)
            {
                res = "";
                SimpleLogger.Error("getNextEntry() parse() Error: " + e.Message);
            }

            return res;
        }

        public static List<CompanyEntity> processOldDeprecated(String url, ref int errorCode)
        {
            var content = parse(url, ref errorCode);
            List<CompanyEntity> list = new List<CompanyEntity>();
            int pos = 0;
            do
            {
                try
                {
                    int first = content.IndexOf(TEXT_TO_FIND, pos);
                    if (first == -1) break;

                    int next = content.IndexOf(TEXT_TO_FIND, first+TEXT_TO_FIND.Length);

                    pos = first;
                    // busco el Name
                    while (content.Substring(pos-1,3)!= " - ")
                    {
                        pos = pos - 1;
                        if (pos == 0) 
                            break;
                    }

                    string name = content.Substring(pos + 1, first - pos - 1).Trim();

                    // busco el Id
                    pos = pos-2;
                    int pos2 = pos;
                    while (Enumerable.Range(48, 57).Contains(content[pos]))
                    {
                        pos = pos - 1;
                        if (pos == 0)
                            break;
                    }

                    int id = System.Convert.ToInt32(content.Substring(pos + 1, pos2 - pos).Trim());

                    pos = first;

                    CompanyEntity client = new CompanyEntity(
                        id, name,
                        // Comienzo de operaciones
                        getNextEntry(ref pos, next, content, "Comienzo de operaciones:", new String[] { "Objeto social:" }),
                        // Objeto Social
                        getNextEntry(ref pos, next, content, "Objeto social:", new String[] { "Domicilio:" }),
                        // Domicilio
                        getNextEntry(ref pos, next, content, "Domicilio:", new String[] { "Capital:" }),
                        // Capital
                        getNextEntry(ref pos, next, content, "Capital:", new String[] { "Euros." }),
                        // Socio único
                        getNextEntry(ref pos, next, content, "Socio único:", new String[] { "cve:", "Nombramientos"}),
                        // Admin único
                        getNextEntry(ref pos, next, content, "Adm. Unico:", new String[] { "Datos registrales" }),
                        // Datos registrales
                        getNextEntry(ref pos, next, content, "Datos registrales", new String[] { ")." }) + ')'
                    );

                    list.Add(client);

                } 
                catch (Exception e)
                {
                    SimpleLogger.Error("remotePdfFile() process() Error: " + e.Message);
                    break;
                }

            }
            while (true);

            return list;

        }
      
        // lamentablemente este proceso no se puede hacer más simple pues el creadod el PDF no ha seguido patrones limpios en sus Tags, ...
        public static List<CompanyEntity> process(String prov, String url, ref int errorCode)
        {
            var content = parse(url, ref errorCode);
            List<CompanyEntity> list = new List<CompanyEntity>();

            int pos = 0;
            int first = content.IndexOf("Actos inscritos", pos);
            String fordeleting = content.Substring(1, first);

            // voy a crear un patrón para la cabacera del PDF para poder ir eliminandola de las sucesivas páginas ...
            int pag1 = content.IndexOf("Pág. ");
            fordeleting = content.Substring(1, pag1+4);
            String spag1 = "";
            while (Enumerable.Range(48, 57).Contains(content[pag1+5]))
            {
                spag1 += content[pag1+5];
                pag1++;
                if (pag1+5 >= content.Length)
                    break;
            }
            int iPag = Convert.ToInt32(spag1);
            String pattern = fordeleting + "{0}";// + content.Substring(pag1 + 5, first + "Actos inscritos".Length + prov.Length - (pag1 + 5) + 1);
            int pp = 0;

            // remuevo las cabeceras de los PDF ...
            while ((pp = content.IndexOf(String.Format(pattern, iPag))) != -1)
            {
                content = content.Remove(pp, String.Format(pattern, iPag).Length);
                iPag++;
            }

            // scrapping ...
            pos = 0;
            first = 0;
            int id = 0;
            if (first != -1)
            {
                pos = first;
                // busco el elemento diferenciador <id> - <company>
                while (content.Substring(pos, 3) != " - ")
                {
                    pos = pos + 1;
                    if (pos >= content.Length)
                        break;
                }
                // busco el "primer" Id
                pos = pos - 2;
                int pos2 = pos;
                while (Enumerable.Range(48, 57).Contains(content[pos]))
                {
                    pos = pos - 1;
                    if (pos == 0)
                        break;
                }
                try
                {
                    id = System.Convert.ToInt32(content.Substring(pos + 1, pos2 - pos + 1).Trim());
                }
                catch
                {
                    id = 0;
                }
            }
            id--;

            // si hay un "primer id" ...
            if (id>=0) do
            {
                id++;

                try
                {
                        
                    // busco ese "<id> - "
                    pos = content.IndexOf(id.ToString()+ " - ", first);
                    if (pos == -1) 
                        break;

                    String thiscontent = "";
                    int next = content.IndexOf((id+1).ToString()+" - ");
                    if (next != -1) 
                        thiscontent = content.Substring(pos, next - pos);
                    else
                        thiscontent = content.Substring(pos, content.Length);

                    first = next;
                        // busco el Tag de constitucion ...
                    pp = thiscontent.IndexOf("Constitución.");
                    if (pp == -1) {
                        continue;
                    }

                    // proceso ...
                    string name = thiscontent.Substring((id.ToString() + " - ").Length, pp- (id.ToString() + " - ").Length).Trim();

                    String detail = thiscontent.Substring(pp + "Constitución.".Length);
                    int pcons = 0;
                    CompanyEntity client = new CompanyEntity(
                        id, name,
                        // Comienzo de operaciones
                        getNextEntry(ref pcons, 9999, detail, "Comienzo de operaciones:",   new String[] { "Objeto social:" }),
                        // Objeto Social
                        getNextEntry(ref pcons, 9999, detail, "Objeto social:",             new String[] { "Domicilio:" }),
                        // Domicilio
                        getNextEntry(ref pcons, 9999, detail, "Domicilio:",                 new String[] { "Capital", "Nombramientos" }),
                        // Capital
                        getNextEntry(ref pcons, 9999, detail, "Capital:",                   new String[] { "Euros.  Declaración de unipersonalidad.", "Euros.", "Nombramientos" }),
                        // Socio único
                        getNextEntry(ref pcons, 9999, detail, "Socio único:",               new String[] { ".", "Adm. Unico:", "Nombramientos" }),
                        // Admin único
                        getNextEntry(ref pcons, 9999, detail, "Adm. Unico:",                new String[] { "Datos registrales" }),
                        // Datos registrales
                        getNextEntry(ref pcons, 9999, detail, "Datos registrales",          new String[] { ")." })+")"
                    );

                    list.Add(client);

                }
                catch (Exception e)
                {
                    SimpleLogger.Error("remotePdfFile() process() Error: " + e.Message);
                    break;
                }

            }
            while (true);

            return list;

        }
    }
}
