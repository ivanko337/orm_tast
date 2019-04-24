using System;

namespace ORM.Exceptions
{
    public class HaventPrimaryKeyException : Exception
    {
        public HaventPrimaryKeyException(Type type) : base(string.Format("Type {0} haven't primary key property", type.Name))
        { }

        public HaventPrimaryKeyException(string message) : base(message)
        { }
    }

    public class RowIsNullException : Exception
    {
        public RowIsNullException()
        { }

        public RowIsNullException(string message) : base(message)
        { }
    }

    public class IncorrectTypeException : Exception
    {
        public IncorrectTypeException(Type type) : base(string.Format("This ORM can't work with {0} type.", type.Name))
        { }

        public IncorrectTypeException(string msg) : base(msg)
        { }
    }
}
