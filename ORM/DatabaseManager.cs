using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.OracleClient;
using ORM.Exceptions;
using ORM.Attributes;

namespace ORM
{
    public class DatabaseManager
    {
        private string ConnectionString { get; set; }

        public DatabaseManager(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public DatabaseData<T> GetData<T>() where T : DatabaseObject, new()
        {
            string tableName = Functions.GetTableName(typeof(T));
            string query = "SELECT * FROM " + tableName;
            DataSet dataSet = new DataSet();
            OracleDataAdapter resultAdapter;
            OracleConnection resConnection;

            using (OracleConnection connection = new OracleConnection(this.ConnectionString))
            {
                connection.Open();

                using (OracleDataAdapter adapter = new OracleDataAdapter(query, connection))
                {
                    adapter.FillSchema(dataSet, SchemaType.Source, tableName);
                    adapter.Fill(dataSet, tableName);
                    resultAdapter = adapter;
                }

                resConnection = connection;
                connection.Close();
            }

            return new DatabaseData<T>(dataSet, new OracleCommandBuilder(resultAdapter), resultAdapter, resConnection);
        }

        private string GetUpdateCommand<T>(DataRow row) where T : DatabaseObject, new()
        {
            string tableName = Functions.GetTableName(typeof(T));
            string primaryKeyColumn = Functions.GetPrimaryKeyColumnName(typeof(T));
            List<string> fieldNames = Functions.GetAllFields(typeof(T));
            string query = "UPDATE " + tableName + " SET ";

            foreach (string field in fieldNames)
            {
                query += field + " = " + Functions.ConvertToDBValidFormat(row[field]) + ", ";
            }
            query = query.Remove(query.Length - 2);

            query += " WHERE " + primaryKeyColumn + " = " + row[primaryKeyColumn].ToString();

            return query;
        }

        public void Commit<T>(DatabaseData<T> data) where T : DatabaseObject, new()
        {
            string tableName = Functions.GetTableName(typeof(T));
            string query = "SELECT * FROM " + tableName;

            data.Connection.ConnectionString = this.ConnectionString;
            data.Connection.Open();
            data.Adapter.SelectCommand = new OracleCommand(query, data.Connection);
            data.Adapter.Update(data.Data, tableName);
            data.Connection.Close();
        }

        public decimal GetNext(Type type)
        {
            SequenceNameAttribute sequenceAttribute = type.GetCustomAttributes(typeof(SequenceNameAttribute), true).FirstOrDefault() as SequenceNameAttribute;
            string sequenceName = sequenceAttribute.Name;
            string query = "SELECT " + sequenceName + ".NEXTVAL FROM dual";
            decimal res;

            using (OracleConnection connection = new OracleConnection(this.ConnectionString))
            {
                connection.Open();

                OracleCommand command = new OracleCommand(query, connection);
                res = Convert.ToDecimal(command.ExecuteScalar());

                connection.Close();
            }

            return res;
        }
    }
}
