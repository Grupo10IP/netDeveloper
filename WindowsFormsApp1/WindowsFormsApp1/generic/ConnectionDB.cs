using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data;
using WindowsFormsApp1;

namespace ConnectionDBNS
{
    class ConnectionDB
    {

        static string host =        WindowsFormsApp1.ConfigIniFile.config.Read("host", "DBConnection");
        static string database =    WindowsFormsApp1.ConfigIniFile.config.Read("database", "DBConnection");
        static string userDB =      WindowsFormsApp1.ConfigIniFile.config.Read("user", "DBConnection");
        static string password =    WindowsFormsApp1.ConfigIniFile.config.Read("password", "DBConnection");
        public static string strProvider = "server=" + host + ";database=" + database + ";user id=" + userDB + ";pwd=" + password;

        public MySql.Data.MySqlClient.MySqlConnection conn;

        public bool Open(String connectionString=null)
        {
            try
            {
                if (connectionString == null)
                    strProvider = "server=" + host + ";database=" + database + ";user id=" + userDB + ";pwd=" + password;
                else
                    strProvider = connectionString;

                conn = new MySqlConnection(strProvider);
                conn.Open();
                return true;
            }
            catch (Exception er)
            {
                SimpleLogger.Error("ConnectionDB(): Connection Error ! " + er.Message);
                throw er;
            }
            return false;
        }

        public void Close()
        {
            conn.Close();
            conn.Dispose();
        }

        public DataSet ExecuteDataSet(string sql)
        {
            try
            {
                DataSet ds = new DataSet();
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                da.Fill(ds, "result");
                return ds;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

        public MySqlDataReader ExecuteReader(string sql)
        {
            try
            {
                MySqlDataReader reader;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                reader = cmd.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

        public int ExecuteNonQuery2(string sql)
        {
            try
            {
                int affected;
                MySqlTransaction mytransaction = conn.BeginTransaction();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                affected = cmd.ExecuteNonQuery();
                mytransaction.Commit();
                return affected;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1;
        }


        public MySqlCommand ExecuteNonQueryParams(string sql, List<MySqlParameter> parms)
        {
            MySqlCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                MySqlParameter prm;
                for (int i = 0; i < parms.Count; i++)
                {
                    prm = cmd.Parameters.Add(parms[i].ParameterName, parms[i].MySqlDbType);
                    prm.Value = parms[i].Value;//.ToString();
                }
                var ln = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return cmd;
        }
    }
}