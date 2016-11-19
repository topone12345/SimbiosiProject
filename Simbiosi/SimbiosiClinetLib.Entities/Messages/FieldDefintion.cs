using System;
using SimbiosiClientLib.Entities.Streams;

namespace SimbiosiClientLib.Entities.Messages
{
    /// <summary>
    /// The supported field types
    /// </summary>
    public enum FieldTypes
    {
        ASCIIString,
        String,
        Int32,
        UInt32,
        Int16,
        UInt16,
        Int64,
        UInt64,
        Double,
        Decimal,
        Datetime,
        Timespan,
        Blob,
        NetworkResource
    }

    public class FieldDefintion : ISimbiosiSerializable, IEquatable<FieldDefintion>
    {
        private  string _name;

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(FieldDefintion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as FieldDefintion;
            return other != null && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
           
            return Name.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(FieldDefintion left, FieldDefintion right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(FieldDefintion left, FieldDefintion right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region fields

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => _name;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FieldTypes Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDefintion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public FieldDefintion(string name, FieldTypes type)
        {
            _name = name;
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDefintion"/> class.
        /// </summary>
        public FieldDefintion()
        {
            _name = string.Empty;
        }

        #endregion

        #region Implementation of ISimbiosiSerializable


        /// <summary>
        /// Writes the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="whenErrorReturnBack">if set to <c>true</c> [when error return back].</param>
        public void Write(SimbiosiStreamWriter writer, bool whenErrorReturnBack)
        {
            if (whenErrorReturnBack)
                writer.CreateBackPoint();

            try
            {
                writer.Write((byte) Type);
                writer.Write(Name);
            }
            finally
            {
                if(whenErrorReturnBack)
                    writer.ReturnToBackPoint();
            }
        }

        /// <summary>
        /// Reads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="whenErrorReturnBack">if set to <c>true</c> [when error return back].</param>
        public void Read(SimbiosiStreamReader reader, bool whenErrorReturnBack)
        {
            if (whenErrorReturnBack)
                reader.CreateBackPoint();

            try
            {
                Type = (FieldTypes) reader.ReadByte();
                _name = reader.ReadString();
            }
            finally
            {
                if (whenErrorReturnBack)
                    reader.ReturnToBackPoint();
            }
        }

        #endregion
    }
}
