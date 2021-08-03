using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using ConnectionDBNS;
using WindowsFormsApp1;

namespace BOEAppNS
{

    static class BOEDayProcessing
    {
        public const String URL_BASE_PATH = @"http://boe.es/";

        static List<Tuple<BOEEntity, List<CompanyEntity>>> list = new List<Tuple<BOEEntity, List<CompanyEntity>>>();

        private static void sendProgressNotification(Form form, String str, int mType=GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_OK)
        {
            if (mType == GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_OK)
                SimpleLogger.Info(str);
            else
                SimpleLogger.Error("Error: " + str);
            GenericFuntions.SendMessageEx(form.Handle, GenericFuntions.WM_PROGRESS_NOTIFICATION, str, mType);
       }

        public static List<Tuple<BOEEntity, List<CompanyEntity>>> process(ConnectionDB conn, DateTime boeday, Form form)
        {

            try
            {
                form.Cursor = Cursors.WaitCursor;

                String boe = "https://www.boe.es/diario_borme/xml.php?id=BORME-S-" + boeday.ToString("yyyyMMdd");

                // intento de descarga del fichero XML conteniendo las URL de los PDF's ...
                String content = "";
                if (RemoteFile.getContent(boe, ref content) == -1)
                {
                    sendProgressNotification(form, "There is no data for that day", GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_ERROR);
                    return null;
                }

                // proceso dicho XML ...
                int errCode = 0;
                XDocument xd = XDocument.Parse(content);
                bool hasIdentifier = xd.Elements("sumario").Any();
                if (!hasIdentifier)
                {
                    sendProgressNotification(form, "There is no data for that day", GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_ERROR);
                    return null;
                }
                String nbo = xd.Element("sumario").Element("diario").Element("sumario_nbo").Attribute("id").Value.ToString();
                String diario = xd.Element("sumario").Element("diario").Attribute("nbo").Value.ToString();
                String anno = xd.Element("sumario").Element("meta").Element("anno").Value.ToString();
                String fecha = xd.Element("sumario").Element("meta").Element("fechaInv").Value.ToString();

                // **** LINQ
                // busca el Nodo principal de trabajo ...
                var xNode = xd.Element("sumario").Element("diario").Element("seccion").Elements("emisor").Where(e => (string)e.Attribute("nombre") == "Actos inscritos").Elements("item").ToList();

                try
                {
                    // procesa sus entradas ...
                    foreach (var item in xNode)
                    {
                        String itemid = item.Attribute("id").Value;
                        Console.WriteLine(item.Attribute("id").Value);
                        Console.WriteLine(item.Element("titulo").Value);    // provincia
                        Console.WriteLine(item.Element("urlPdf").Value);    // fichero pdf

                        // me salto el fichero de resumen final 
                        if (item.Element("urlPdf").Value.IndexOf("-99.pdf") != -1)
                            continue;

                        sendProgressNotification(form, item.Element("urlPdf").Value, GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_OK);

                        // procesa un PDF ...
                        List<CompanyEntity> aux = RemotePdfFile.process(item.Element("titulo").Value, URL_BASE_PATH + item.Element("urlPdf").Value, ref errCode);

                        var l = new Tuple<BOEEntity, List<CompanyEntity>>(new BOEEntity(item.Element("titulo").Value, itemid, diario, anno, fecha, item.Element("urlPdf").Value), aux);

                        // añade a la lista de trabajo ...
                        list.Add(l);
                    }

                    // inserta la lista en BDD ...
                    RemotePdfFile.toDB(conn, list);

                }
                catch (Exception e)
                {
                    sendProgressNotification(form, "Error " + e.Message, GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_ERROR);
                }

                sendProgressNotification(form, "Successfuly!!!");

            } finally
            {
                form.Cursor = Cursors.Default;

            }
            return list;
        }

    }
}
