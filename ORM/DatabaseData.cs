using System.Collections.Generic;
using System.Data;

namespace ORM
{
    public class DatabaseData<T> where T : DatabaseObject, new()
    {
        public DataSet Data { get; private set; }
        private string TableName { get; set; }
        private string SequenceName { get; set; }

        public DatabaseData(DataSet set)
        {
            this.Data = set;
            this.TableName = Functions.GetTableName(typeof(T));
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

        public T Create(T obj)
        {
            DataRow newRow = this.Data.Tables[this.TableName].NewRow();
            string primaryKey = Functions.GetPrimaryKeyColumnName(typeof(T));

            obj.Row = newRow;
            obj.Commit();

            this.Data.Tables[this.TableName].Rows.Add(newRow);

            return obj;
        }

        public void Delete(T obj)
        {
            string condition = Functions.GetCondition(obj);
            DataRow row = this.Data.Tables[this.TableName].Select(condition)[0];

            row.Delete();
        }

        public void Edit(T obj)
        {
            string condition = Functions.GetCondition(obj);
            DataRow row = this.Data.Tables[this.TableName].Select(condition)[0];

            obj.Commit();
            row = obj.Row;
        }
    }
}
