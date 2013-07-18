using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace IB_Reports.Model
{
    public class DataHelper
    {
        string ConnectionFormat = ConfigurationManager.AppSettings["ConnectionFormat"];

        string DataSource = ConfigurationManager.AppSettings["DataSource"];
        string Password = ConfigurationManager.AppSettings["Password"];
        string UserId = ConfigurationManager.AppSettings["UserId"];
        string DefaultDB = ConfigurationManager.AppSettings["DefaultDB"];
        int connectionTimeout = Convert.ToInt16(ConfigurationManager.AppSettings["connectionTimeout"]);
        bool TrastedConnection = Convert.ToBoolean(ConfigurationManager.AppSettings["TrastedConnection"]);

        private SqlConnection _connection;
        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public void Connect(string initialCatalog)
        {
            if (_isConnected) return;
            if (_connection != null && _connection.State == ConnectionState.Connecting)
            {
                return;
            }

            lock (new object())
            {
                _connection = new SqlConnection { ConnectionString = string.Format(ConnectionFormat, UserId, DataSource, TrastedConnection, initialCatalog, connectionTimeout) };

                if (_connection.State != ConnectionState.Open)
                {
                    try
                    {
                        _connection.Open();
                        _isConnected = true;
                    }
                    catch (Exception e)
                    {
                        Logger.WriteToLog("DataHelper.Connect(): " + e.Message);
                        if (_connection.State != ConnectionState.Open)
                            _isConnected = false;
                    }
                }
            }
        }

        public void Disconnect()
        {
            if (_isConnected)
            {
                try
                {
                    _connection.Close();
                    _isConnected = false;
                }
                catch (Exception e)
                {
                    if (_connection.State != ConnectionState.Open)
                        _isConnected = false;
                    Logger.WriteToLog("DataHelper.Disconnect(): " + e.Message);
                }
                finally
                {
                    _connection = null;
                }
            }
        }

        private SqlConnection GetConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                Connect(DefaultDB);

            return _connection;
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
                Logger.WriteToLog("DataHelper.InsertDataTable(): " + e.Message);
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

        public void InsertReportsData(string AccountName, string AccountId, DateTime date, double total, double totalLong, double totalShort)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertReportsData, AccountName, AccountId, date, total, totalLong, totalShort));
        }

        public void InsertActivitiesData(string AccountName, string AccountId, DateTime date, string ActivityDescription, double Amount)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertActivityData, AccountName, AccountId, date, ActivityDescription, Amount));
        }

        public void InsertProcessResult(string AccountName, DateTime date, string success)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlInsertProcessResult, AccountName, date, success));
        }

        public DataTable GetProcessSuccessAccountsNames()
        {
            return ExecuteSqlForData(string.Format(StoredProcedures.SqlGetProcessSuccessAccountsNames, DateTime.Today)) ?? new DataTable();
        }

        public void TrancateActivitysDataRows(string AccountName, DateTime date)
        {
            ExecuteSQL(string.Format(StoredProcedures.SqlTrancateActivitysDataRows, date, AccountName));
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
            //Logger.WriteToLog("DataHelper.ExecuteSQL: " + sql);
            return ExecuteSQL(sql, CommandType.Text, null);
        }

        public bool ExecuteSQL(string sql, CommandType commandType, List<SqlParameter> parameters)
        {
            SqlCommand command;

            if (!IsConnected)
                Connect(DefaultDB);

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
                Logger.WriteToLog("DataHelper.ExecuteSQL() executing: " + e.Message);
                return false;
            }
            return true;
        }

        public DataTable ExecuteSqlForData(string sql)
        {
            Logger.WriteToLog("DataHelper.ExecuteSqlForData executing: " + sql);
            
            if (!IsConnected || _connection == null)
                Connect(DefaultDB);

            System.Diagnostics.Debug.WriteLine(sql);

            DataTable result = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            DataRow row;
            try
            {
                command = new SqlCommand(sql, GetConnection()) { CommandType = CommandType.Text };
                command.CommandTimeout = 0;
                reader = command.ExecuteReader();
                if (reader != null)
                    while (reader.Read())
                    {
                        if (result == null)
                        {
                            result = CreateResultTable(reader);
                        }
                        row = result.NewRow();
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
                Logger.WriteToLog("DataHelper.ExecuteSqlForData(): " + e.Message);
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

        private DataTable CreateResultTable(IDataRecord reader)
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
