using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using BOEAppNS;
using ConnectionDBNS;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using MySql.Data.MySqlClient;


namespace WindowsFormsApp1
{

    public partial class MainForm : Form
    {
        private ConnectionDB con = new ConnectionDB();

        TreeNode lastSelectedNode;

        System.Threading.Timer timer = null;

        private const int WS_MAXIMIZEBOX = 0x00010000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_MAXIMIZEBOX;
                return cp;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            this.monthCalendar1.AddAnnuallyBoldedDate(new System.DateTime(2021, 1, 1, 0, 0, 0, 0));
            this.monthCalendar1.UpdateBoldedDates();

            SimpleLogger.Init();

            dataGridView3.Font = new Font("Tahoma", 8);
            dataGridView2.Font = new Font("Tahoma", 8);

            treeView1.HideSelection = false;
            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;

            button2.Enabled = false;

            timer = null;

        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == GenericFuntions.WM_PROGRESS_NOTIFICATION)
            {
                button4.Visible = true;
                // do whatever with your message
                if (((int)m.LParam==GenericFuntions.WM_PROGRESS_NOTIFICATION_MSG_OK))
                {
                    lblProcess.ForeColor = Color.Green;
                    button4.ImageIndex = 6;
                } else
                {
                    lblProcess.ForeColor = Color.Red;
                    button4.ImageIndex = 5;
                }
                lblProcess.Text = Marshal.PtrToStringUni(m.WParam);
                lblProcess.Refresh();
                button4.Refresh();

            } 
            base.WndProc(ref m);
        }


        private void SetTimer(int interval)
        {
            // CheckForIllegalCrossThreadCalls = false;
            if (timer != null) timer = null;
            timer = new System.Threading.Timer(x =>
            {
               Thread.Sleep(interval);
               panProcess.Visible = false;
            }, null, 0, Timeout.Infinite);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panProcess.Visible = true;
            lblProcessing.Text = "Processing:";
            lblProcess.Text = "";
            lblProcessing.Refresh();
            button4.Visible = false;
            BOEAppNS.BOEDayProcessing.process(con, dateTimePicker1.Value, this);
            MainForm_Shown(null, null);

            panProcess.Visible = true;
            lblProcessing.Text = "Processed:";

            //SetTimer(5000);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            updateDatagrid();

            if (treeView1.Nodes.Count > 0)
            {
                treeView1.Nodes[0].Expand();
                if (treeView1.Nodes[0].Nodes.Count > 0)
                    treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0];
            }
        }


        private void updateDatagrid()
        {
            try
            {
                treeView1.Nodes.Clear();
                var rootNode = treeView1.Nodes.Add("BOE Collection");

                con.Open();
                string query = "select b.provinceid bprov, p.name pname, b.date bdate, b.name bname,  b.anno banno, b.urlpath burl, b.id bid " +
                               " from boes b, provinces p " +
                               " WHERE b.provinceid = p.id " +
                               " ORDER BY b.provinceid, b.date desc";

                MySqlDataReader row;
                row = con.ExecuteReader(query);
                if (row.HasRows)
                {
                    bool red = row.Read();
                    while (red)
                    {
                        String prov = row["bprov"].ToString();

                        var nd = rootNode.Nodes.Add(row["pname"].ToString());
                        nd.SelectedImageIndex = 1;
                        // guardo la provincia
                        nd.Tag = Convert.ToInt32(row["bprov"].ToString());

                        while ((prov == row["bprov"].ToString()) && (red))
                        {
                            var ndc = nd.Nodes.Add(row["bdate"].ToString() + " - " + row["bname"].ToString());
                            ndc.SelectedImageIndex = 1;
                            // guardo el BOE
                            ndc.Tag = new BOEAppNS.BOEEntity(
                                 row["pname"].ToString(),
                                 row["bname"].ToString(),
                                 row["bname"].ToString(),
                                 row["banno"].ToString(),
                                 row["bdate"].ToString(),
                                 row["burl"].ToString());
                            ((BOEEntity)ndc.Tag).dbKey = Convert.ToInt32(row["bid"].ToString());
                            red = row.Read();
                        }

                    }

                }
               
            }
            catch (Exception err)
            {
                SimpleLogger.Error("MainForm(): updateDatagrid(): "+err.Message);
            }
            finally
            {
                con.Close();
                DrawingControl.ResumeDrawing(dataGridView2);
            }
        }

        private void updateDatagrid2(int provinceid, int boeid)
        {
            try
            {
                DrawingControl.SuspendDrawing(dataGridView2);

                dataGridView2.Rows.Clear();
                con.Open();
                string query = "select bd.id bdid, bd.enterprise bdent, bd.capital bdcapital from boedetails bd where boeid = " + boeid.ToString();

                MySqlDataReader row;
                row = con.ExecuteReader(query);
                if (row.HasRows)
                {
                    while (row.Read())
                    {
                        DataGridViewRow newRow = new DataGridViewRow();

                        newRow.CreateCells(dataGridView2);
                        newRow.Cells[0].Value = row["bdid"].ToString();
                        newRow.Cells[1].Value = row["bdent"].ToString();
                        newRow.Cells[2].Value = row["bdcapital"].ToString();
                        dataGridView2.Rows.Add(newRow);
                    }
                }

               
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            finally
            {
                con.Close();
                DrawingControl.ResumeDrawing(dataGridView2);
            }

        }

        private void updateDatagrid3(int provinceid)
        {
            try
            {
                DrawingControl.SuspendDrawing(dataGridView3);

                dataGridView3.Rows.Clear();
                con.Open();
                string query = "select b.name bname, b.anno banno, b.date bdate, bd.id bdid, bd.enterprise bdent, bd.capital bdcapital from boes b, boedetails bd " +
                               "where b.id = bd.boeid and b.provinceid = " + provinceid + " ORDER BY boeid";

                MySqlDataReader row;
                row = con.ExecuteReader(query);
                if (row.HasRows)
                {
                    while (row.Read())
                    {
                        DataGridViewRow newRow = new DataGridViewRow();

                        newRow.CreateCells(dataGridView3);
                        newRow.Cells[0].Value = row["bdid"].ToString();
                        newRow.Cells[1].Value = row["bdate"].ToString() + " - " + row["bname"].ToString();
                        newRow.Cells[2].Value = row["bdent"].ToString();
                        newRow.Cells[3].Value = row["bdcapital"].ToString();
                        //newRow.Cells[3].Value = row["bddate"].ToString();
                        dataGridView3.Rows.Add(newRow);
                    }
                }

                con.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            finally
            {
                DrawingControl.ResumeDrawing(dataGridView3);
            }

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            button2.Enabled = false;

            try
            {
                // Select new node
                e.Node.BackColor = SystemColors.Highlight;
                e.Node.ForeColor = SystemColors.HighlightText;
                if (lastSelectedNode != null)
                {
                    // Deselect old node
                    lastSelectedNode.BackColor = SystemColors.Window;
                    lastSelectedNode.ForeColor = SystemColors.WindowText;
                }
                lastSelectedNode = e.Node;

                var node = treeView1.SelectedNode;

                if (node != treeView1.Nodes[0])
                {

                    if ((node.Tag) is BOEEntity)
                    {
                        button2.Enabled = true;
                        int key = ((BOEEntity)node.Tag).dbKey;
                        tabControl1.SelectedTab = tabControl1.TabPages[1];
                        updateDatagrid2(0, key);
                        // la provincia?
                        updateCalendar(Convert.ToInt32(node.Parent.Tag));
                    }
                    else
                    {
                        int key = (int)node.Tag;
                        updateCalendar(key);
                        tabControl1.SelectedTab = tabControl1.TabPages[0];
                        node.ExpandAll();
                        updateDatagrid3(key);
                    }
                }
            } 
            finally
            {
            }

        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            var id = -1;
            if (dataGridView2.SelectedCells.Count > 0)
            {
                id = dataGridView2.SelectedCells[0].RowIndex;
                updateInfoPanel(Convert.ToInt32(dataGridView2.Rows[id].Cells[0].Value));
            }

        }
        private void updateInfoPanel(int boedetailid)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();

            con.Open();
            try
            {
                string query = "select * from boedetails WHERE id=" + boedetailid.ToString();
                MySqlDataReader row;
                row = con.ExecuteReader(query);
                if (row.HasRows)
                {
                    if (row.Read())
                    {
                        textBox1.Text = row["enterprise"].ToString();
                        textBox2.Text = row["address"].ToString();
                        textBox3.Text = row["date"].ToString();
                        textBox4.Text = row["socialobject"].ToString();
                        textBox5.Text = row["unicadmin"].ToString();
                        textBox6.Text = row["unicpartner"].ToString();
                        textBox7.Text = row["registraldata"].ToString();
                    }
                }
            }
            finally
            {
                con.Close();
            }
        }


        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            var id = -1;
            if (dataGridView3.SelectedCells.Count > 0)
            {
                id = dataGridView3.SelectedCells[0].RowIndex;
                updateInfoPanel(Convert.ToInt32(dataGridView3.Rows[id].Cells[0].Value));
            }

        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            TreeNodeStates state = e.State;
            Font font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            Color foreColor;
            Color backColor;

            // node is selected
            // if you want to see the color of a selected node, too,
            // you can use inverted fore/back colors instead of system selection colors 
            if ((state & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                bool isFocused = (state & TreeNodeStates.Focused) == TreeNodeStates.Focused;
                backColor = SystemColors.Window;// Highlight;
                foreColor = isFocused ? SystemColors.HighlightText : SystemColors.InactiveCaptionText;

                var r = new Rectangle(e.Bounds.X, e.Bounds.Y+1, e.Bounds.Width, e.Bounds.Height);
                //e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
               // if (isFocused)
               //     ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, foreColor, backColor);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, r, foreColor, TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            // node is not selected
            else
            {
                backColor = Color.White;
                foreColor = Color.Black;
                if (e.Node.Level == 2)
                    foreColor = Color.Gray;
                using (Brush background = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(background, e.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, foreColor, TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if ((e.Node.Tag) is BOEEntity)
                {
                    int key = ((BOEEntity)e.Node.Tag).dbKey;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((treeView1.SelectedNode.Tag) is BOEEntity)
            {
                int key = ((BOEEntity)treeView1.SelectedNode.Tag).dbKey;
                String url = BOEDayProcessing.URL_BASE_PATH+((BOEEntity)treeView1.SelectedNode.Tag).urlpdf;
                PDFViewer.openPDF(url);
            }

        }

        private void updateCalendar(int provinceid)
        {            
            con.Open();
            try
            {
                string query = "select count(distinct date) cnt from boes where provinceid=" + provinceid.ToString();
                MySqlDataReader row;
                int cnt = 0;
                row = con.ExecuteReader(query);
                if (row.HasRows)
                {
                    if (row.Read())
                    {
                        cnt = Convert.ToInt32(row["cnt"].ToString());
                    }
                }

                //DateTime[] bolds = new DateTime[cnt];
                List<DateTime> boldlist = new List<DateTime>();
                if (cnt > 0)
                {
                    row.Close();
                    query = "select distinct date dat from boes where provinceid=" + provinceid.ToString();
                    row = con.ExecuteReader(query);
                    if (row.HasRows)
                    {
                        while (row.Read())
                        {
                            DateTime myDate = DateTime.ParseExact(row["dat"].ToString(), "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
                            boldlist.Add(myDate);
                        }
                    }
                }

                int i = 0;
                DateTime datefirst=DateTime.MaxValue;
                while (i < boldlist.Count)
                {
                    if (boldlist[i] < datefirst)
                    {
                        datefirst = boldlist[i];
                    }
                    i++;
                }
                if (datefirst< DateTime.MaxValue) 
                { 
                    monthCalendar1.SetDate(datefirst);
                }

                monthCalendar1.AnnuallyBoldedDates = boldlist.ToArray();
                monthCalendar1.UpdateBoldedDates();
            }
            finally
            {
                con.Close();
            }

        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            dateTimePicker1.Value = monthCalendar1.SelectionStart;
        }

        private void dataGridView3_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            var charsforremoving = new HashSet<char>() {',', '.'};
            if (e.Column.Name.IndexOf("ColCapital")>=0)
            {
                var s1 = new String(e.CellValue1.ToString().Where(c => !charsforremoving.Contains(c)).ToArray());
                var s2 = new String(e.CellValue2.ToString().Where(c => !charsforremoving.Contains(c)).ToArray());
                e.SortResult = s1.PadLeft(15, '0').CompareTo(s2.PadLeft(15, '0'));
            } else 
            {
                e.SortResult = System.String.Compare(e.CellValue1.ToString(), e.CellValue2.ToString());
            }

            e.Handled = true;

        }
    }
 }
