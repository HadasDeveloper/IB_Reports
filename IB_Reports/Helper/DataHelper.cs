using System;
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
                        logger.WriteToLog(DateTime.Now, "DataHelper.Connect(): " + e.Message,"Log");
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
                    logger.WriteToLog(DateTime.Now, "DataHelper.Disconnect(): " + e.Message, "IB_Log");
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
                logger.WriteToLog(DateTime.Now, "DataHelper.InsertDataTable(): " + e.Message,"IB_Log");
            }
        }

        //run stored procedures
        // ----------------------  Example of running no return query ---------------------------------

        //public void DeleteCompanyAddress(string companyId, string streetAddressId)
        //{
        //    ExecuteSQL(string.Format(StoredProcedures.SqlDeleteCompanyAddress, companyId, streetAddressId));
        //}


        //public DataTable GetRSIOrders()
        //{
        //    return ExecuteSqlForData(string.Format(StoredProcedures.SqlGetTodaysOeders)) ?? new DataTable();
        //}

        public void InsertReportsData(string accountName, string accountId, DateTime date, double total, double totalLong, double totalShort)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertReportsData, accountName, accountId, date, total, totalLong, totalShort));
        }

        public void InsertActivitiesData(string accountName, string accountId, DateTime date, string activityDescription, double amount)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertActivityData, accountName, accountId, date, activityDescription, amount));
        }

        public void InsertProcessResult(string accountName, DateTime date, string success)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertProcessResult, accountName, date, success));
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
   
        // ----------------------  Core Functions ---------------------------------

        public bool ExecuteSQL(string sql)
        {
            //logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSQL: " + sql);
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
                logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSQL() executing: " + e.Message, "IB_Log");
                return false;
            }
            return true;
        }

        public DataTable ExecuteSqlForData(string sql)
        {
            logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSqlForData executing: " + sql, "IB_Log");
            
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
                logger.WriteToLog(DateTime.Now, "DataHelper.ExecuteSqlForData(): " + e.Message, "IB_Log");
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
