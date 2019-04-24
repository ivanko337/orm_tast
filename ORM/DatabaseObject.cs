using System.Linq;
using System.Data;
using System.Reflection;
using ORM.Attributes;
using ORM.Exceptions;

namespace ORM
{
    public class DatabaseObject
    {
        public DataRow Row { get; set; }
        
        public void Attach(DataRow row)
        {
            this.Row = row;

            PropertyInfo[] propertyes = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in propertyes)
            {
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                if (attribute == null)
                {
                    continue;
                }
                property.SetValue(this, row[attribute.Name], null);
            }
        }

        public void Commit()
        {
            if (this.Row == null)
            {
                throw new RowIsNullException(string.Format("In object {0} Row is null", GetType().ToString()));
            }

            PropertyInfo[] propertyes = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in propertyes)
            {
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                if (attribute == null)
                {
                    continue;
                }
                this.Row[attribute.Name] = property.GetValue(this, null);
            }
        }

        public void Rollback()
        {
            this.Attach(this.Row);
        }
    }
}
