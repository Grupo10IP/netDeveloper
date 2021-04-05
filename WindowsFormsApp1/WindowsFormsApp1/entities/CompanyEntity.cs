using System;
using System.Linq;

namespace BOEAppNS
{
    class CompanyEntity
    {
        private int id;
        private String name;
        private String date="";
        private string socialObject="";
        private string address="";
        private string capital = "";
        private string unicPartner = "";
        private string unicAdmin = "";
        private string registralData = "";

        public int Id { get => id; }
        public String Name { get => name; }
        public String Date { get => date; }
        public String SocialObject { get => socialObject; }
        public String Address { get => address; }
        public String Capital { get => capital; }
        public String UnicPartner { get => unicPartner; }
        public String UnicAdmin { get => unicAdmin; }
        public String RegistralData { get => registralData; }

        public CompanyEntity(int id, String name, String date, String socialObject, String address, String capital, String unicPartner, String unicAdmin, String registralData)
        {
            this.id = id;
            this.name = name;
            this.date = "";
            try
            {
                // filtra "bad chars"
                date = String.Join("", date.Where(c => (c=='.') || (Char.IsDigit(c))));
                string[] subs = date.Split('.');
                if (subs.Length>2)
                    this.date = new DateTime(2000+Convert.ToInt32(subs[2]), Convert.ToInt32(subs[1]), Convert.ToInt32(subs[0])).ToString("yyyy/MM/dd");
            }
            catch {
            }
            this.socialObject =     socialObject;
            this.address =          address;
            this.capital =          capital;
            this.unicPartner =      unicPartner;
            this.unicAdmin =        unicAdmin;
            this.registralData =    registralData;
        }
    }
}
