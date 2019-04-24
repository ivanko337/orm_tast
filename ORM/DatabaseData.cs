using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.Reflection;
using ORM.Attributes;
using ORM.Exceptions;

namespace ORM
{
    public class DatabaseData<T> where T : DatabaseObject, new()
    {
        public DataSet Data { get; private set; }
        public OracleCommand UpdateCommand { get; private set; } = new OracleCommand();
        public OracleDataAdapter Adapter { get; private set; }
        public OracleCommandBuilder Builder { get; set; }
        public OracleConnection Connection { get; set; }
        private string TableName { get; set; }
        private string SequenceName { get; set; }

        public DatabaseData(DataSet set, OracleCommandBuilder builder, OracleDataAdapter adapter, OracleConnection connection)
        {
            this.Data = set;
            this.Adapter = adapter;
            this.Builder = builder;
            this.Connection = connection;
            this.TableName = Functions.GetTableName(typeof(T));
        }

        // Составляет условие для выбора элемента для удаления или редактирования.
        // Условие составляется на основе первичного ключа
        // При отсутствии свойства, помеченного аттрибутом PrimaryKeyAttribute
        // возбуждается исключение HaventPrimaryKeyException
        private string GetCondition(T obj)
        {
            string fieldName = "";
            string fieldValue = "";

            DataColumn primaryKeyRow = this.Data.Tables[this.TableName].PrimaryKey[0];
            fieldName = primaryKeyRow.ColumnName;

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                if(attribute.Name == fieldName)
                {
                    fieldValue = property.GetValue(obj, null).ToString();
                }
                if (!string.IsNullOrEmpty(fieldName))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(fieldValue))
            {
                throw new HaventPrimaryKeyException(typeof(T));
            }

            return fieldName + " = " + fieldValue;
        }

        public IEnumerable<T> Select()
        {
            return this.Select("");
        }

        public IEnumerable<T> Select(string condition)
        {
            List<T> res = new List<T>();

            DataRow[] rows = this.Data.Tables[this.TableName].Select(condition);

            foreach (DataRow row in rows)
            {
                T newItem = new T();
                newItem.Attach(row);
                res.Add(newItem);
            }

            return res;
        }

        private void SetDataRowValues(ref DataRow row, T obj)
        {
            PropertyInfo[] propertyes = typeof(T).GetProperties();

            foreach (PropertyInfo property in propertyes)
            {
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                if(attribute == null)
                {
                    continue;
                }
                row[attribute.Name] = property.GetValue(obj, null);
            }
        }

        public T Create(T obj, decimal next)
        {
            DataRow newRow = this.Data.Tables[this.TableName].NewRow();
            string primaryKey = Functions.GetPrimaryKeyColumnName(typeof(T));
            
            this.SetDataRowValues(ref newRow, obj);
            newRow[primaryKey] = next;
            obj.Row = newRow;
            obj.Rollback();

            this.Data.Tables[this.TableName].Rows.Add(newRow);

            this.UpdateCommand = this.Builder.GetUpdateCommand();

            return obj;
        }

        public void Delete(T obj)
        {
            string condition = this.GetCondition(obj);
            this.Builder = new OracleCommandBuilder(this.Adapter);
            DataRow row = this.Data.Tables[this.TableName].Select(condition)[0];

            row.Delete();
        }

        public void Edit(T obj)
        {
            string condition = this.GetCondition(obj);
            DataRow row = this.Data.Tables[this.TableName].Select(condition)[0];

            this.SetDataRowValues(ref row, obj);

            this.UpdateCommand = this.Builder.GetUpdateCommand();
        }
    }
}
