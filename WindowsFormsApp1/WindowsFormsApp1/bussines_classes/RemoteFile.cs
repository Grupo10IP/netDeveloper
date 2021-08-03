using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WindowsFormsApp1;

namespace BOEAppNS
{
    static class RemoteFile
    {
        static public int getContent(String url, ref String content)
        {
            content = "";

            // **** WCF
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    using (System.IO.Stream stream = wc.OpenRead(url))
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                        {
                            content = reader.ReadToEnd();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    SimpleLogger.Error("RemoteFile(): getContent(): Error " + ex.Message);
                    return -1;
                }

                return 0;
            }

        } 
        
    }
}
