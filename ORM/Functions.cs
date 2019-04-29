using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ORM.Attributes;
using ORM.Exceptions;

namespace ORM
{
    public static class Functions
    {
        public static string GetTableName(Type type)
        {
            TableNameAttribute attribute = type.GetCustomAttributes(typeof(TableNameAttribute), true).FirstOrDefault() as TableNameAttribute;
            return attribute.Name;
        }

        public static string GetSequenceName(Type type)
        {
            SequenceNameAttribute attribute = type.GetCustomAttributes(typeof(SequenceNameAttribute), true).FirstOrDefault() as SequenceNameAttribute;
            return attribute.Name;
        }

        public static string GetPrimaryKeyColumnName(Type type)
        {
            PropertyInfo[] propertyes = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in propertyes)
            {
                PrimaryKeyAttribute attribute = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).FirstOrDefault() as PrimaryKeyAttribute;
                if (attribute != null)
                {
                    FieldNameAttribute nameAttribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                    return nameAttribute.Name;
                }
            }

            throw new HaventPrimaryKeyException(type);
        }

        public static List<string> GetAllFields(Type type)
        {
            PropertyInfo[] propertyes = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> res = new List<string>();

            foreach (PropertyInfo property in propertyes)
            {
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                PrimaryKeyAttribute primaryKeyAttribute = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).FirstOrDefault() as PrimaryKeyAttribute;
                if (attribute == null)
                {
                    continue;
                }
                if (primaryKeyAttribute == null)
                {
                    res.Add(attribute.Name);
                }
            }

            return res;
        }

        public static string ConvertToDBValidFormat(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return string.Format("'{0}'", obj);
            }
            else if (obj.GetType() == typeof(decimal))
            {
                return obj.ToString();
            }
            else if (obj.GetType() == typeof(DateTime))
            {
                string format = "yyyy.MM.dd";
                return "TO_DATE('" + DateTime.Parse(obj.ToString()).ToString(format) + "', '" + format + "')";
            }

            throw new IncorrectTypeException(obj.GetType());
        }
        
        // Составляет условие для выбора элемента для удаления или редактирования.
        // Условие составляется на основе первичного ключа
        // При отсутствии свойства, помеченного аттрибутом PrimaryKeyAttribute
        // возбуждается исключение HaventPrimaryKeyException
        public static string GetCondition<T>(T obj)
        {
            if(obj == null)
            {
                return "";
            }

            string fieldName = "";
            string fieldValue = "";

            fieldName = GetPrimaryKeyColumnName(typeof(T));

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                if (attribute.Name == fieldName)
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
    }
}
