using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using SimbiosiClientLib.Entities.Messages;
using static System.String;

namespace SimbiosiClientLib.Entities.Streams
{
    public class SimbiosiStreamReader : IDisposable
    {
        private const int MaxCharBytesSize = 1024;

        private Stream _mStream;
        private byte[] _mBuffer;
        private Decoder _mDecoder;
        private byte[] _mCharBytes;
 
        private char[] _mCharBuffer;
        private readonly int _mMaxCharsSize;  // From MaxCharBytesSize & Encoding

        // Performance optimization for Read() w/ Unicode.  Speeds us up by ~40% 
        private readonly bool _mLeaveOpen;
        private byte[] _asciiBuffer;
        private readonly CultureInfo _decimalCultureInfo = new CultureInfo("en-US");
        private long _backPoint = -1;

        public SimbiosiStreamReader(Stream input, Encoding encoding) : this(input, encoding, false) {
        }

        public SimbiosiStreamReader(Stream input, Encoding encoding, bool leaveOpen)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
            if (!input.CanRead)
                throw new ArgumentException("Stream is not readable");
            
            _mStream = input;
            _mDecoder = encoding.GetDecoder();
            _mMaxCharsSize = encoding.GetMaxCharCount(MaxCharBytesSize);
            int minBufferSize = encoding.GetMaxByteCount(1);  // max bytes per one char
            if (minBufferSize < 16)
                minBufferSize = 16;
            _mBuffer = new byte[minBufferSize];
            // m_charBuffer and m_charBytes will be left null.

            // For Encodings that always use 2 bytes per char (or more), 
            // special case them here to make Read() & Peek() faster.
            // check if BinaryReader is based on MemoryStream, and keep this for it's life
            // we cannot use "as" operator, since derived classes are not allowed
            _mLeaveOpen = leaveOpen;

        }

        /// <summary>
        /// Gets the base stream.
        /// </summary>
        /// <value>
        /// The base stream.
        /// </value>
        public virtual Stream BaseStream => _mStream;

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public virtual void Close()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream copyOfStream = _mStream;
                _mStream = null;
                if (copyOfStream != null && !_mLeaveOpen)
                    copyOfStream.Close();
            }
            _mStream = null;
            _mBuffer = null;
            _mDecoder = null;
            _mCharBytes = null;
            _mCharBuffer = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public virtual int Read(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "index is out of range. Needs to be greater than 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count is out of range.  Needs to be greater than 0");
            if (buffer.Length - index < count)
                throw new ArgumentException("The index + count passed is greater than buffer len");

            if(_mStream==null) throw new ObjectDisposedException("The reader has been disposed");
            
            return _mStream.Read(buffer, index, count);
        }

        public virtual byte[] ReadBytes(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count is out of range.  Needs to be greater than 0");
          

            if (_mStream == null) throw new ObjectDisposedException("The reader has been disposed");

            if (count == 0)
                return new byte[0];

            byte[] result = new byte[count];

            int numRead = 0;
            do
            {
                int n = _mStream.Read(result, numRead, count);
                if (n == 0)
                    break;
                numRead += n;
                count -= n;
            } while (count > 0);

            if (numRead != result.Length)
            {
                // Trim array.  This should happen on EOF & possibly net streams.
                byte[] copy = new byte[numRead];
                Buffer.BlockCopy(result, 0, copy, 0, numRead);
                result = copy;
            }

            return result;
        }

        public virtual byte ReadByte()
        {
            // Inlined to avoid some method call overhead with FillBuffer.
            if (_mStream == null) throw new ObjectDisposedException("Reader is disposed");

            int b = _mStream.ReadByte();
            if (b == -1)
                throw new EndOfStreamException("EOF reading from SimbiosiStreamReader"); 

            return (byte)b;
        }

        public int Read7BitEncodedInt()
        {
            // Read out an Int32 7 bits at a time.  The high bit
            // of the byte when on means to continue reading more bytes.
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream.  Read a max of 5 bytes.
                // In a future version, add a DataFormatException.
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    throw new FormatException("Bad stream format");

                // ReadByte handles end of stream cases for us.
                b = ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }

        /*
            /// <summary>
        /// Writes the ASCII short string.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void WriteASCIIShortString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var len = Encoding.ASCII.GetByteCount(value);

            if (len>4096)
                throw new ArgumentException("ASCII short string should be lower than 4096 bytes len");

            if (_asciiBuffer == null)
            {
                _asciiBuffer = new byte[4096];
            }

            Encoding.ASCII.GetBytes(value, 0, value.Length, _asciiBuffer, 0);
            OutStream.Write(_asciiBuffer, 0, len);
        }
         */

        /// <summary>
        /// Reads the ASCII short string.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">Reader is disposed</exception>
        /// <exception cref="System.IO.IOException">Invalid string len</exception>
        public virtual string ReadASCIIShortString()
        {
            if (_mStream == null) throw new ObjectDisposedException("Reader is disposed");

            var len = Read7BitEncodedInt();

            if(len<0 || len>4096)
                throw new IOException("Invalid string len");

            if (len == 0) return Empty;

            if(_asciiBuffer==null) _asciiBuffer = new byte[4096];
            _mStream.Read(_asciiBuffer, 0, len);
            return Encoding.ASCII.GetString(_asciiBuffer, 0, len);
        }


        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">Reader is disposed</exception>
        /// <exception cref="System.IO.IOException">Invalid string length</exception>
        /// <exception cref="System.IO.EndOfStreamException"></exception>
        public virtual string ReadString()
        {


            if (_mStream == null) throw new ObjectDisposedException("Reader is disposed");

            int currPos = 0;

            // Length of the string in bytes, not chars
            var stringLength = Read7BitEncodedInt();
            if (stringLength < 0)
            {
                throw new IOException("Invalid string length", stringLength);
            }

            if (stringLength == 0)
                return Empty;

            if (_mCharBytes == null)
                _mCharBytes = new byte[MaxCharBytesSize];

            if (_mCharBuffer == null)
                _mCharBuffer = new char[_mMaxCharsSize];

            StringBuilder sb = null;
            do
            {
                var readLength = ((stringLength - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (stringLength - currPos);

                var n = _mStream.Read(_mCharBytes, 0, readLength);
                if (n == 0)
                    throw new EndOfStreamException();

                var charsRead = _mDecoder.GetChars(_mCharBytes, 0, n, _mCharBuffer, 0);

                if (currPos == 0 && n == stringLength)
                    return new string(_mCharBuffer, 0, charsRead);

                if (sb == null)
                    sb = new StringBuilder(stringLength); // Actual string length in chars may be smaller.
                sb.Append(_mCharBuffer, 0, charsRead);
                currPos += n;

            } while (currPos < stringLength);

            return sb.ToString();
        }



        /// <summary>
        /// Reads the s byte.
        /// </summary>
        /// <returns></returns>
        public virtual sbyte ReadSByte()
        {
            FillBuffer(1);
            return (sbyte)(_mBuffer[0]);
        }

        /// <summary>
        /// Reads the int16.
        /// </summary>
        /// <returns></returns>
        public virtual short ReadInt16()
        {
            FillBuffer(2);
            return (short)(_mBuffer[0] | _mBuffer[1] << 8);
        }


        /// <summary>
        /// Reads the int32.
        /// </summary>
        /// <returns></returns>
        public virtual int ReadInt32()
        {
          
            FillBuffer(4);
            return _mBuffer[0] | _mBuffer[1] << 8 | _mBuffer[2] << 16 | _mBuffer[3] << 24;
            
        }


        /// <summary>
        /// Reads the u int32.
        /// </summary>
        /// <returns></returns>
        public virtual uint ReadUInt32()
        {
            FillBuffer(4);
            return (uint)(_mBuffer[0] | _mBuffer[1] << 8 | _mBuffer[2] << 16 | _mBuffer[3] << 24);
        }

        /// <summary>
        /// Reads the u int16.
        /// </summary>
        /// <returns></returns>
        public virtual ushort ReadUInt16()
        {
            FillBuffer(2);
            return (ushort)(_mBuffer[0] | _mBuffer[1] << 8);
        }

        /// <summary>
        /// Reads the int64.
        /// </summary>
        /// <returns></returns>
        public virtual long ReadInt64()
        {
            FillBuffer(8);
            uint lo = (uint)(_mBuffer[0] | _mBuffer[1] << 8 |
                             _mBuffer[2] << 16 | _mBuffer[3] << 24);
            uint hi = (uint)(_mBuffer[4] | _mBuffer[5] << 8 |
                             _mBuffer[6] << 16 | _mBuffer[7] << 24);
            // ReSharper disable once RedundantCast
            return (long)((ulong)hi) << 32 | lo;
        }


        /// <summary>
        /// Reads the u int64.
        /// </summary>
        /// <returns></returns>
        public virtual ulong ReadUInt64()
        {
            FillBuffer(8);
            uint lo = (uint)(_mBuffer[0] | _mBuffer[1] << 8 |
                             _mBuffer[2] << 16 | _mBuffer[3] << 24);
            uint hi = (uint)(_mBuffer[4] | _mBuffer[5] << 8 |
                             _mBuffer[6] << 16 | _mBuffer[7] << 24);
            return ((ulong)hi) << 32 | lo;
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <returns></returns>
        public virtual unsafe double ReadDouble()
        {
            FillBuffer(8);
            uint lo = (uint)(_mBuffer[0] | _mBuffer[1] << 8 |
                _mBuffer[2] << 16 | _mBuffer[3] << 24);
            uint hi = (uint)(_mBuffer[4] | _mBuffer[5] << 8 |
                _mBuffer[6] << 16 | _mBuffer[7] << 24);

            ulong tmpBuffer = ((ulong)hi) << 32 | lo;
            return *((double*)&tmpBuffer);
        }

        /// <summary>
        /// Reads the decimal.
        /// </summary>
        /// <returns></returns>
        public virtual decimal ReadDecimal()
        {
            return decimal.Parse(ReadASCIIShortString(), _decimalCultureInfo);
        }

        /// <summary>
        /// Reas the date time.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime ReaDateTime()
        {
            var dt = ReadASCIIShortString();
            return DateTime.Parse(dt, null, DateTimeStyles.RoundtripKind);  //ISO 8601
        }


        protected virtual void FillBuffer(int numBytes)
        {
            if (_mBuffer != null && (numBytes < 0 || numBytes > _mBuffer.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(numBytes));
            }
            int bytesRead = 0;
            int n;

            if (_mStream == null) throw new ObjectDisposedException("Reader is disposed");

            // Need to find a good threshold for calling ReadByte() repeatedly
            // vs. calling Read(byte[], int, int) for both buffered & unbuffered
            // streams.
            if (numBytes == 1)
            {
                n = _mStream.ReadByte();
                if (n == -1)
                    throw new EndOfStreamException();

                Debug.Assert(_mBuffer != null, "m_buffer != null");
                _mBuffer[0] = (byte)n;
                return;
            }

            do
            {
                Debug.Assert(_mBuffer != null, "m_buffer != null");
                n = _mStream.Read(_mBuffer, bytesRead, numBytes - bytesRead);
                if (n == 0)
                {
                     throw new EndOfStreamException();
                }
                bytesRead += n;
            } while (bytesRead < numBytes);
        }


        /// <summary>
        /// Creates the back point.
        /// </summary>
        public void CreateBackPoint()
        {
            if (_mStream == null) throw new ObjectDisposedException("Reader is disposed");
            _backPoint = BaseStream.Position;
        }

        /// <summary>
        /// Returns to back point.
        /// </summary>
        public void ReturnToBackPoint()
        {
            if (_mStream == null) throw new ObjectDisposedException("Reader is disposed");
            if (_backPoint != -1)
            {
                BaseStream.Position = _backPoint;
                _backPoint = -1;
            }
        }


        /// <summary>
        /// Reads the specified entity to read.
        /// </summary>
        /// <param name="entityToRead">The entity to read.</param>
        /// <param name="whenErrorReturn">if set to <c>true</c> [when error return].</param>
        public void Read(ISimbiosiSerializable entityToRead, bool whenErrorReturn)
        {
            entityToRead.Read(this, whenErrorReturn);
        }


        /// <summary>
        /// Reads the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="whenErrorReturn">if set to <c>true</c> [when error return].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Type provideed \"" + type.Name +
        ///                                             "\" does not implement ISImbiosiSerializable
        /// or
        /// Type provideed \"" + type.Name +
        ///                                             "\" has not a public parameterless constructor
        /// </exception>
        public ISimbiosiSerializable Read(Type type, bool whenErrorReturn)
        {

            if (!type.IsAssignableFrom(typeof(ISimbiosiSerializable)))
                throw new ArgumentException("Type provideed \"" + type.Name +
                                            "\" does not implement ISImbiosiSerializable");

            if (!(type.GetConstructor(Type.EmptyTypes) == null))
                throw new ArgumentException("Type provideed \"" + type.Name +
                                            "\" has not a public parameterless constructor");
            var instance =  (ISimbiosiSerializable) Activator.CreateInstance(type);

            Read(instance, whenErrorReturn);

            return instance;
        }

        /// <summary>
        /// Reads the specified when error return. With generic methods performance is improved
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whenErrorReturn">if set to <c>true</c> [when error return].</param>
        /// <returns></returns>
        public ISimbiosiSerializable Read<T>(bool whenErrorReturn) where T : ISimbiosiSerializable
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            ObjectActivator<T> createdActivator = GetActivator<T>(ctor);
            var instance = (ISimbiosiSerializable)createdActivator();

            Read(instance, whenErrorReturn);
            return instance;
        }


        internal delegate T ObjectActivator<T>(params object[] args);

        internal static ObjectActivator<T> GetActivator<T>
        (ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            //compile it
            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
            return compiled;
        }




    }

  
}
