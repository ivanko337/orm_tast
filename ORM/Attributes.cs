using System;

namespace ORM.Attributes
{
    interface IDatabaseAttribute
    {
        string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute, IDatabaseAttribute
    {
        public string Name { get; set; }

        public TableNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldNameAttribute : Attribute, IDatabaseAttribute
    {
        public string Name { get; set; }

        public FieldNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SequenceNameAttribute : Attribute, IDatabaseAttribute
    {
        public string Name { get; set; }

        public SequenceNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    { }
}
