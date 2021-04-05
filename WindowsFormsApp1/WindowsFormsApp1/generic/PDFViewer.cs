using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class PDFViewer : Form
    {

        public static void openPDF(String src)
        {
            PDFViewer viewer = new PDFViewer();
            viewer.axAcroPDF1.src = src;
            viewer.ShowDialog();
        }

        private PDFViewer()
        {
            InitializeComponent();
        }
    }
}
