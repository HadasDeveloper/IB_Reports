﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using IB_Reports.Model;
using Logger;

namespace IB_Reports.Helper
{
    public class DataHelper
    {
        readonly FileLogWriter logger = new FileLogWriter();

        readonly string connectionFormat = ConfigurationManager.AppSettings["ConnectionFormat"];

        readonly string dataSource = ConfigurationManager.AppSettings["DataSource"];
//        string Password = ConfigurationManager.AppSettings["Password"];
        readonly string userId = ConfigurationManager.AppSettings["UserId"];
        readonly string defaultDB = ConfigurationManager.AppSettings["DefaultDB"];
        readonly int connectionTimeout = Convert.ToInt16(ConfigurationManager.AppSettings["connectionTimeout"]);
        readonly bool trastedConnection = Convert.ToBoolean(ConfigurationManager.AppSettings["TrastedConnection"]);

        private SqlConnection connection;
        private bool isConnected;

        public bool IsConnected
        {
            get { return isConnected; }
        }

        public void Connect(string initialCatalog)
        {
            if (isConnected) return;
            if (connection != null && connection.State == ConnectionState.Connecting)
            {
                return;
            }

            lock (new object())
            {
                connection = new SqlConnection { ConnectionString = string.Format(connectionFormat, userId, dataSource, trastedConnection, initialCatalog, connectionTimeout) };

                if (connection.State != ConnectionState.Open)
                {
                    try
                    {
                        connection.Open();
                        isConnected = true;
                    }
                    catch (Exception e)
                    {
                        logger.WriteToLog(DateTime.Now, "DataHelper.Connect(): " + e.Message, ConfigurationManager.AppSettings["logFileName"]);
                        if (connection.State != ConnectionState.Open)
                            isConnected = false;
                    }
                }
            }
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    connection.Close();
                    isConnected = false;
                }
                catch (Exception e)
                {
                    if (connection.State != ConnectionState.Open)
                        isConnected = false;
                    logger.WriteToLog(DateTime.Now, "DataHelper.Disconnect(): " + e.Message, ConfigurationManager.AppSettings["logFileName"]);
                }
                finally
                {
                    connection = null;
                }
            }
        }

        private SqlConnection GetConnection()
        {
            if (connection == null || connection.State != ConnectionState.Open)
                Connect(defaultDB);

            return connection;
        }


        public void InsertDataTable(DataTable table, string storedProcedureName, string tableValueParameterName)
        {
            SqlCommand insertCommand = new SqlCommand(storedProcedureName, GetConnection()) { CommandType = CommandType.StoredProcedure };

            SqlParameter tvpParam = insertCommand.Parameters.AddWithValue(tableValueParameterName, table);
            tvpParam.SqlDbType = SqlDbType.Structured;

            try
            {
                insertCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.WriteToLog(DateTime.Now, "DataHelper.InsertDataTable(): " + e.Message,ConfigurationManager.AppSettings["logFileName"]);
            }
        }

        //------------------------ User Functions ----------------------------------------

        public void InsertProcessResult(string accountName, DateTime date, string success)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertProcessResult, accountName, date, success));
        }

         public void InsertData(PerformanceReport data)
         {
             List<SqlParameter> parameters = new List<SqlParameter>
                                                 {
                                                     new SqlParameter("@ReportData", data.ReportDataTable),
                                                     new SqlParameter("@ActivitesData", data.ActivityDataTable)
                                                 };

             ExecuteSQL(string.Format(StoredProcedures.SqlInsertDate), CommandType.StoredProcedure, parameters);
         }

        public DataTable GetProcessSuccessAccountsNames()
        {
            return ExecuteSqlForData(string.Format(StoredProcedures.SqlGetProcessSuccessAccountsNames, DateTime.Today)) ?? new DataTable();
        }

        public void TrancateActivitysDataRows(string accountName, DateTime date)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlTrancateActivitysDataRows, date, accountName));
        }

        public void CalcualteDailyChanges()
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlCalculateDailyChanges));

        }
        internal DataTable GetDailyChanges()
        {
            return ExecuteSqlForData(string.Format(StoredProcedures.SqlGetGetDailyChanges)) ?? new DataTable();
        }

        public DataTable GetDailyChangesData(string accounts)
        {
            return ExecuteSqlForData(string.Format(StoredProcedures.SqlGetDailyChangesData, accounts)) ?? new DataTable();
        }

        public DataTable GetDailyPerformanceData(string accounts)
        {
            return ExecuteSqlForData(string.Format(StoredProcedures.SqlGetDailyPerformanceData, accounts)) ?? new DataTable();
        }
   
   
        // ----------------------  Core Functions ---------------------------------

        public bool ExecuteSQL(string sql)
        {
            return ExecuteSQL(sql, CommandType.Text, null);
        }

        public bool ExecuteSQL(string sql, CommandType commandType, List<SqlParameter> parameters)
        {
            SqlCommand command;

            if (!IsConnected)
                Connect(defaultDB);

            try
            {
                command = new SqlCommand(sql, GetConnection()) { CommandType = commandType };

                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var sqlParameter in parameters)
                    {
                        command.Parameters.Add(sqlParameter);
                    }
                }

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSQL() executing: " + e.Message, ConfigurationManager.AppSettings["logFileName"]);
                return false;
            }
            return true;
        }

        public DataTable ExecuteSqlForData(string sql)
        {
            logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSqlForData executing: " + sql, ConfigurationManager.AppSettings["logFileName"]);
            
            if (!IsConnected || connection == null)
                Connect(defaultDB);

            System.Diagnostics.Debug.WriteLine(sql);

            DataTable result = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                command = new SqlCommand(sql, GetConnection()) {CommandType = CommandType.Text, CommandTimeout = 0};
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (result == null)
                    {
                        result = CreateResultTable(reader);
                    }
                    DataRow row = result.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.IsDBNull(i)
                            && !(reader.GetFieldType(i) == typeof(string))
                            && !(reader.GetFieldType(i) == typeof(DateTime)))
                        {
                            row[i] = 0;
                        }
                        else
                        {
                            row[i] = reader.GetValue(i);
                        }
                    }
                    result.Rows.Add(row);
                }

                System.Diagnostics.Debug.WriteLine(reader.FieldCount);

                return result;
            }
            catch (SqlException e)
            {
                logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSqlForData(): " + e.Message, ConfigurationManager.AppSettings["logFileName"]);
                return result;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (command != null)
                    command.Dispose();
            }
        }

        private static DataTable CreateResultTable(IDataRecord reader)
        {
            DataTable dataTable = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataColumn dataColumn = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                dataTable.Columns.Add(dataColumn);
            }

            return dataTable;
        }

    }
}
