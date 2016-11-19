using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using SimbiosiClientLib.Entities.Messages;

namespace SimbiosiClientLib.Entities.Streams
{
    public class SimbiosiStreamWriter : IDisposable
    {
        public static readonly SimbiosiStreamWriter Null = new SimbiosiStreamWriter();

        protected Stream OutStream;
        private readonly byte[] _buffer;    // temp space for writing primitives to.
        private readonly Encoding _encoding;
        private readonly Encoder _encoder;

        [OptionalField]  // New in .NET FX 4.5.  False is the right default value.
        private readonly bool _leaveOpen;

        // This field should never have been serialized and has not been used since before v2.0.
        // However, this type is serializable, and we need to keep the field name around when deserializing.
        // Also, we'll make .NET FX 4.5 not break if it's missing.
#pragma warning disable 169
        [OptionalField]
        private char[] _tmpOneCharBuffer;
#pragma warning restore 169

        // Perf optimization stuff
        private byte[] _largeByteBuffer;  // temp space for writing chars.
        private int _maxChars;   // max # of chars we can put in _largeByteBuffer
        // Size should be around the max number of chars/string * Encoding's max bytes/char
        private const int LargeByteBufferSize = 1024;

        private byte[] _asciiBuffer;
        private readonly CultureInfo _decimalCultureInfo = new CultureInfo("en-US");
        private long _backPoint = -1;

        protected SimbiosiStreamWriter()
        {
            OutStream = Stream.Null;
            _buffer = new byte[16];
            _encoding = new UTF8Encoding(false, true);
            _encoder = _encoding.GetEncoder();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimbiosiStreamWriter"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="encoding">The encoding.</param>
        public SimbiosiStreamWriter(Stream output, Encoding encoding) : this(output, encoding, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimbiosiStreamWriter"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="leaveOpen">if set to <c>true</c> [leave open].</param>
        /// <exception cref="System.ArgumentNullException">
        /// output
        /// or
        /// encoding
        /// </exception>
        /// <exception cref="System.ArgumentException">The stream is not writable</exception>
        public SimbiosiStreamWriter(Stream output, Encoding encoding, bool leaveOpen)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (!output.CanWrite)
                throw new ArgumentException("The stream is not writable");
            

            OutStream = output;
            _buffer = new byte[16];
            _encoding = encoding;
            _encoder = _encoding.GetEncoder();
            _leaveOpen = leaveOpen;
        }

        // Closes this writer and releases any system resources associated with the
        // writer. Following a call to Close, any operations on the writer
        // may raise exceptions. 
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
                if (_leaveOpen)
                    OutStream.Flush();
                else
                    OutStream.Close();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /*
          * Returns the stream associate with the writer. It flushes all pending
          * writes before returning. All subclasses should override Flush to
          * ensure that all buffered data is sent to the stream.
          */
        /// <summary>
        /// Gets the base stream.
        /// </summary>
        /// <value>
        /// The base stream.
        /// </value>
        public virtual Stream BaseStream
        {
            get
            {
                Flush();
                return OutStream;
            }
        }

        // Clears all buffers for this writer and causes any buffered data to be
        // written to the underlying device. 
        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public virtual void Flush()
        {
            OutStream.Flush();
        }


        /// <summary>
        /// Write7s the bit encoded int.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write7BitEncodedInt(int value)
        {
            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint)value;   // support negative numbers
            while (v >= 0x80)
            {
                Write((byte)(v | 0x80));
                v >>= 7;
            }
            Write((byte)v);
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(byte value)
        {
            OutStream.WriteByte(value);
        }


        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(sbyte value)
        {
            OutStream.WriteByte((byte)value);
        }

        // Writes a byte array to this stream.
        // 
        // This default implementation calls the Write(Object, int, int)
        // method to write the byte array.
        // 
        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        public virtual void Write(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            

            OutStream.Write(buffer, 0, buffer.Length);
        }

        // Writes a section of a byte array to this stream.
        //
        // This default implementation calls the Write(Object, int, int)
        // method to write the byte array.
        // 
        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public virtual void Write(byte[] buffer, int index, int count)
        {
            OutStream.Write(buffer, index, count);
        }


        // Writes a double to this stream. The current position of the stream is
        // advanced by eight.
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual unsafe void Write(double value)
        {
            ulong tmpValue = *(ulong*)&value;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _buffer[4] = (byte)(tmpValue >> 32);
            _buffer[5] = (byte)(tmpValue >> 40);
            _buffer[6] = (byte)(tmpValue >> 48);
            _buffer[7] = (byte)(tmpValue >> 56);
            OutStream.Write(_buffer, 0, 8);
        }


        // Writes a two-byte signed integer to this stream. The current position of
        // the stream is advanced by two.
        // 
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            OutStream.Write(_buffer, 0, 2);
        }

        // Writes a two-byte unsigned integer to this stream. The current position
        // of the stream is advanced by two.
        // 
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            OutStream.Write(_buffer, 0, 2);
        }

        // Writes a four-byte signed integer to this stream. The current position
        // of the stream is advanced by four.
        // 
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            OutStream.Write(_buffer, 0, 4);
        }

        // Writes a four-byte unsigned integer to this stream. The current position
        // of the stream is advanced by four.
        // 
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            OutStream.Write(_buffer, 0, 4);
        }

        // Writes an eight-byte signed integer to this stream. The current position
        // of the stream is advanced by eight.
        // 
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(long value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            OutStream.Write(_buffer, 0, 8);
        }

        // Writes an eight-byte unsigned integer to this stream. The current 
        // position of the stream is advanced by eight.
        // 
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(ulong value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            OutStream.Write(_buffer, 0, 8);
        }



        
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(decimal value)
        {
            //Use string serialization instead of binary searlization on 96 bit plus sign and power for
            //Cross-language compliance reasons (for java usage, javascript usage, python usage, plsql and  etc. etc.)
            WriteASCIIShortString(value.ToString(_decimalCultureInfo));
        }


        // Writes a length-prefixed string to this stream in the BinaryWriter's
        // current Encoding. This method first writes the length of the string as 
        // a four-byte unsigned integer, and then writes that many characters 
        // to the stream.
        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">charCount</exception>
        public virtual unsafe void Write(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            

            int len = _encoding.GetByteCount(value);
            Write7BitEncodedInt(len);

            if (_largeByteBuffer == null)
            {
                _largeByteBuffer = new byte[LargeByteBufferSize];
                _maxChars = _largeByteBuffer.Length / _encoding.GetMaxByteCount(1);
            }

            if (len <= _largeByteBuffer.Length)
            {
                //Contract.Assert(len == _encoding.GetBytes(chars, 0, chars.Length, _largeByteBuffer, 0), "encoding's GetByteCount & GetBytes gave different answers!  encoding type: "+_encoding.GetType().Name);
                _encoding.GetBytes(value, 0, value.Length, _largeByteBuffer, 0);
                OutStream.Write(_largeByteBuffer, 0, len);
            }
            else
            {
                // Aggressively try to not allocate memory in this loop for
                // runtime performance reasons.  Use an Encoder to write out 
                // the string correctly (handling surrogates crossing buffer
                // boundaries properly).  
                int charStart = 0;
                int numLeft = value.Length;

                while (numLeft > 0)
                {
                    // Figure out how many chars to process this round.
                    int charCount = (numLeft > _maxChars) ? _maxChars : numLeft;
                    int byteLen;

                    checked
                    {
                        if (charStart < 0 || charCount < 0 || charStart + charCount > value.Length)
                        {
                            throw new ArgumentOutOfRangeException(nameof(value));
                        }

                        fixed (char* pChars = value)
                        {
                            fixed (byte* pBytes = _largeByteBuffer)
                            {
                                byteLen = _encoder.GetBytes(pChars + charStart, charCount, pBytes, _largeByteBuffer.Length, charCount == numLeft);
                            }
                        }
                    }
                    OutStream.Write(_largeByteBuffer, 0, byteLen);
                    charStart += charCount;
                    numLeft -= charCount;
                }

            }
        }


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

            Write7BitEncodedInt(len);
            Encoding.ASCII.GetBytes(value, 0, value.Length, _asciiBuffer, 0);
            OutStream.Write(_asciiBuffer, 0, len);
        }

        /// <summary>
        /// Writes the date time.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Write(DateTime value)
        {
            /*
            DateTime values:
            2009 - 06 - 15T13: 45:30(DateTimeKind.Local)-- > 2009 - 06 - 15T13: 45:30.0000000 - 07:00
            2009 - 06 - 15T13: 45:30(DateTimeKind.Utc)-- > 2009 - 06 - 15T13: 45:30.0000000Z
            2009 - 06 - 15T13: 45:30(DateTimeKind.Unspecified)-- > 2009 - 06 - 15T13: 45:30.0000000
            DateTimeOffset values:
            2009 - 06 - 15T13: 45:30 - 07:00-- > 2009 - 06 - 15T13: 45:30.0000000 - 07:00*/
            WriteASCIIShortString(value.ToString("o"));  //ISO 8601
        }

        /// <summary>
        /// Gets the encoding used
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding => _encoding;


        /// <summary>
        /// Creates the back point.
        /// </summary>
        public void CreateBackPoint()
        {
            _backPoint = BaseStream.Position;
        }

        /// <summary>
        /// Returns to back point.
        /// </summary>
        public void ReturnToBackPoint()
        {
            if (_backPoint != -1)
            {
                BaseStream.Position = _backPoint;
                _backPoint = -1;
            }
        }


        /// <summary>
        /// Writes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Write(ISimbiosiSerializable entity)
        {
            Write(entity, false);
        }

        /// <summary>
        /// Writes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="whenErrorReturn">if set to <c>true</c> [when error return].</param>
        public void Write(ISimbiosiSerializable entity, bool whenErrorReturn)
        {
            entity.Write(this, whenErrorReturn);
        }
    }
}
