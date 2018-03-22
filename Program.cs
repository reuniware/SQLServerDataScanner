using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

namespace SQLServerDataScan
{
    class Program
    {
        /// <summary>
        /// Email : investdatasystems@yahoo.com
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            log("SQLServerDataScan v0.1 (by Reunisoft/DVA Software)");
            log("");

            string connstr = ConfigurationManager.AppSettings["ConnectionString"];

            log("Connection String = [" + connstr + "]");
            string dbname = getDbNameFromConnectionString(connstr);
            log("Initial Catalog (Database Name) = [" + dbname + "]");

            SqlConnection conn = new SqlConnection(connstr);

            try
            {
                log("trying to connect...");
                conn.Open();
                log("connected.");
            }
            catch (SqlException sqlex)
            {
                log("SqlException = " + sqlex.Message);
                log("Press ENTER to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                log("Exception = " + ex.Message);
                log("Press ENTER to continue...");
                Console.ReadLine();
            }

            List<string> lstUserTables = new List<string>();
            List<string> lstObjectId = new List<string>();
            SqlCommand com = new SqlCommand("SELECT * FROM [" + dbname + "].[sys].[all_objects] where type_desc = 'USER_TABLE';", conn);
            SqlDataReader dr = com.ExecuteReader();
            while (dr.Read())
            {
                lstUserTables.Add(dr["name"].ToString().Trim());
                lstObjectId.Add(dr["object_id"].ToString().Trim());
            }
            dr.Close();

            int i = 0;
            foreach (string usertable in lstUserTables)
            {
                //log(">> Scanned User Table = [" + usertable + "]    ---->   Object Id = [" + lstObjectId[i] + "]");
                i++;
            }

            log("");
            Console.Write("Enter string value to scan : ");
            string valToSearch = Console.ReadLine();
            log("");

            List<string> finalResult = new List<string>();

            Console.WriteLine("Searching...");
            for (int j = 0; j < lstUserTables.Count; j++)
            {
                List<string> lstColumns = new List<string>();
                com = new SqlCommand("select * from [" + dbname + "].[sys].all_columns where object_id = '" + lstObjectId[j].ToString() + "';", conn);
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    //log(">> Scanned table.column = " + lstUserTables[j] + "." + dr["name"].ToString().Trim());
                    lstColumns.Add(dr["name"].ToString().Trim());
                }
                dr.Close();
                log(">> Searching in table [" + lstUserTables[j] + "]...");

                for (int k = 0; k < lstColumns.Count; k++)
                {
                    string sql = "select * from [" + dbname + "].[dbo].[" + lstUserTables[j] + "] where cast(" + lstColumns[k] + " as char) = '" + valToSearch + "'";

                    //log(">>>> " + sql);
                    com = new SqlCommand(sql, conn);
                    dr = com.ExecuteReader();
                    int currentline = 0;
                    while (dr.Read())
                    {
                        finalResult.Add(">>>> one line found : TABLE = [" + lstUserTables[j] + "] ; COLUMN = [" + lstColumns[k] + "] ; LINE = [" + currentline.ToString() + "]");
                        //log(">>>> one line found : TABLE = [" + lstUserTables[j] + "] ; COLUMN = [" + lstColumns[k] + "] ; LINE = [" + currentline.ToString() + "]");
                        currentline++;
                        //Console.ReadLine();
                    }
                    dr.Close();
                }

                log("");
            }

            foreach (string result in finalResult)
            {
                log(result);
            }

            try
            {
                log("trying to disconnect...");
                conn.Close();
                log("disconnected.");
            }
            catch (SqlException sqlex)
            {
                log("SqlException = " + sqlex.Message);
                log("Press ENTER to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                log("Exception = " + ex.Message);
                log("Press ENTER to continue...");
                Console.ReadLine();
            }

            Console.ReadLine();
        }

        private static string getDbNameFromConnectionString(string connstr)
        {
            string[] str = connstr.Split(new char[] { ';' });
            for (int i = 0; i < str.GetLength(0); i++)
            {
                if (str[i].Trim().ToLower().StartsWith("initial catalog"))
                {
                    string[] str2 = str[i].Split(new char[] { '=' });
                    return(str2[1].ToString().Trim());
                }
            }
            return string.Empty;
        }


        private static void log(string p)
        {
            Console.WriteLine(p);
        }
    }
}
