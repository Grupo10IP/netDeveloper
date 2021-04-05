using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BOEAppNS
{
    class BOEEntity
    {
        public int dbKey = -1;
        public String province;
        public String nbo;
        public String name;
        public String anno;
        public String date;
        public String urlpdf;

        public BOEEntity(String province, String nbo, String name, String anno, String date, String urlpdf)
        {
            this.province = province;
            this.nbo = nbo;
            this.name = name;
            this.anno = anno;
            this.date = date;
            this.urlpdf = urlpdf;
        }
    }
}
