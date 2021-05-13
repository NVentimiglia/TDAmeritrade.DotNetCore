using System;
using System.Text;

namespace TDAmeritrade
{
    /// <summary>
    ///     Datamodel serialization helper
    /// </summary>
    public class BinSerializer : IDisposable
    {
        #region  CONSTANTS

        /// <summary>
        /// size of a buffer array
        /// </summary>
        public static int BUFFER_SIZE = 512;

        /// <summary>
        /// for null objects
        /// </summary>
        public const byte NULL = 0;

        /// <summary>
        /// for null objects
        /// </summary>
        public const byte NOTNULL = 1;

        #endregion

        #region HEAD

        /// <summary>
        /// Read Data index.
        /// </summary>
        public int Index;

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// ctor
        /// </summary>
        public BinSerializer() : this(BUFFER_SIZE)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public BinSerializer(int size)
        {
            Data = new byte[size];
        }

        /// <summary>
        /// Reset Indicies
        /// </summary>
        public virtual void Reset()
        {
            Index = 0;
        }

        /// <summary>
        /// Reads to Data, includes header
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(byte[] source)
        {
            var length = source.Length > Data.Length ? Data.Length : source.Length;
            Buffer.BlockCopy(source, 0, Data, 0, length);
        }

        /// <summary>
        /// Reads to Data, includes header
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(byte[] target)
        {
            var length = target.Length > Data.Length ? Data.Length : target.Length;
            Buffer.BlockCopy(Data, 0, target, 0, length);
        }

        /// <summary>
        /// Reads to Data, includes header
        /// </summary>
        public void CopyTo(byte[] target, int length)
        {
            Buffer.BlockCopy(Data, 0, target, 0, length);
        }

        /// <summary>
        /// Writes copy of paylod
        /// </summary>
        public byte[] Copy()
        {
            var b = new byte[Index];
            Buffer.BlockCopy(Data, 0, b, 0, Index);
            return b;
        }

        public void Dispose()
        {
            Data = null;
        }

        void Ensures(bool assertion)
        {
            if (!assertion)
                throw new ArgumentException("BufferSerializer : Method failed sanity.");
        }

        #endregion

        #region Peek     

        public unsafe bool CheckHeader(byte header)
        {
            Ensures(Data.Length >= Index + sizeof(byte));

            var check = Data[Index];
            if (check == header)
            {
                ReadByte();
                return true;
            }
            return false;
        }

        public unsafe byte PeekByte()
        {
            Ensures(Data.Length >= Index + sizeof(byte));

            return Data[Index];
        }

        public unsafe bool PeekBool()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            bool value;

            fixed (byte* ptr = Data)
            {
                value = *(bool*)(ptr + Index);
            }
            return value;
        }

        public unsafe short PeekShort()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            short value;

            fixed (byte* ptr = Data)
            {
                value = *(short*)(ptr + Index);
            }
            return value;
        }

        public unsafe int PeekInt()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            int value;

            fixed (byte* ptr = Data)
            {
                value = *(int*)(ptr + Index);
            }
            return value;
        }

        #endregion

        #region Write

        public unsafe void Write(byte value)
        {
            Ensures(Data.Length >= Index + sizeof(byte));

            Data[Index] = value;
            Index += sizeof(byte);
        }

        public unsafe void Write(sbyte value)
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            Data[Index] = (byte)value;
            Index += sizeof(sbyte);
        }

        public unsafe void Write(bool value)
        {
            Ensures(Data.Length >= Index + sizeof(bool));

            fixed (byte* ptr = Data)
            {
                *(bool*)(ptr + Index) = value;
            }
            Index += sizeof(bool);
        }

        public unsafe void Write(short value)
        {
            Ensures(Data.Length >= Index + sizeof(short));

            fixed (byte* ptr = Data)
            {
                *(short*)(ptr + Index) = value;
            }
            Index += sizeof(short);
        }

        public unsafe void Write(ushort value)
        {
            Ensures(Data.Length >= Index + sizeof(ushort));

            fixed (byte* ptr = Data)
            {
                *(ushort*)(ptr + Index) = value;
            }
            Index += sizeof(ushort);
        }

        public unsafe void Write(int value)
        {
            Ensures(Data.Length >= Index + sizeof(int));

            fixed (byte* ptr = Data)
            {
                *(int*)(ptr + Index) = value;
            }
            Index += sizeof(int);
        }

        public unsafe void Write(uint value)
        {
            Ensures(Data.Length >= Index + sizeof(uint));

            fixed (byte* ptr = Data)
            {
                *(uint*)(ptr + Index) = value;
            }
            Index += sizeof(uint);
        }

        public unsafe void Write(long value)
        {
            Ensures(Data.Length >= Index + sizeof(long));

            fixed (byte* ptr = Data)
            {
                *(long*)(ptr + Index) = value;
            }
            Index += sizeof(long);
        }

        public unsafe void Write(ulong value)
        {
            Ensures(Data.Length >= Index + sizeof(ulong));

            fixed (byte* ptr = Data)
            {
                *(ulong*)(ptr + Index) = value;
            }
            Index += sizeof(ulong);
        }

        public unsafe void Write(double value)
        {
            Ensures(Data.Length >= Index + sizeof(double));

            fixed (byte* ptr = Data)
            {
                *(double*)(ptr + Index) = value;
            }
            Index += sizeof(double);
        }

        public unsafe void Write(float value)
        {
            Ensures(Data.Length >= Index + sizeof(float));

            fixed (byte* ptr = Data)
            {
                *(float*)(ptr + Index) = value;
            }
            Index += sizeof(float);
        }

        public unsafe void Write(Guid value)
        {
            Ensures(Data.Length >= Index + sizeof(Guid));

            fixed (byte* ptr = Data)
            {
                *(Guid*)(ptr + Index) = value;
            }
            Index += sizeof(Guid);
        }

        public unsafe void Write(DateTime value)
        {
            long ticks = value.ToBinary();
            Write(ticks);
            value = DateTime.FromBinary(ticks);
        }

        public unsafe void Write(byte[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            Buffer.BlockCopy(value, 0, Data, Index, value.Length);

            Index += length;
        }

        public unsafe void Write(bool[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(ushort[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(short[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(int[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(uint[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(long[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(ulong[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(double[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(float[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);

            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        public unsafe void Write(char value)
        {
            Ensures(Data.Length >= Index + sizeof(char));

            fixed (byte* ptr = Data)
            {
                *(char*)(ptr + Index) = value;
            }
            Index += sizeof(char);
        }

        public unsafe void Write(string value)
        {
            if (value == null)
            {
                int empty = 0;
                Write(empty);
            }
            else
            {
                byte[] data;
                data = Encoding.UTF8.GetBytes(value);
                Write(data);
            }
        }

        public unsafe void Write(string[] value)
        {
            var length = value == null ? 0 : value.Length;
            Write(length);
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    Write(value[i]);
                }
            }
        }

        #endregion

        #region Read

        public unsafe byte ReadByte()
        {
            Ensures(Data.Length >= Index + sizeof(byte));

            var value = Data[Index];
            Index += sizeof(byte);
            return value;
        }

        public unsafe sbyte ReadSByte()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            var value = (sbyte)Data[Index];
            Index += sizeof(sbyte);
            return value;
        }

        public unsafe bool ReadBool()
        {
            Ensures(Data.Length >= Index + sizeof(bool));

            bool value;
            fixed (byte* ptr = Data)
            {
                value = *(bool*)(ptr + Index);
            }
            Index += sizeof(bool);
            return value;
        }

        public unsafe short ReadShort()
        {
            Ensures(Data.Length >= Index + sizeof(short));

            short value;
            fixed (byte* ptr = Data)
            {
                value = *(short*)(ptr + Index);
            }
            Index += sizeof(short);
            return value;
        }

        public unsafe ushort ReadUShort()
        {
            Ensures(Data.Length >= Index + sizeof(ushort));

            ushort value;
            fixed (byte* ptr = Data)
            {
                value = *(ushort*)(ptr + Index);
            }
            Index += sizeof(ushort);
            return value;
        }

        public unsafe int ReadInt()
        {
            Ensures(Data.Length >= Index + sizeof(int));

            int value;
            fixed (byte* ptr = Data)
            {
                value = *(int*)(ptr + Index);
            }
            Index += sizeof(int);
            return value;
        }

        public unsafe uint ReadUInt()
        {
            Ensures(Data.Length >= Index + sizeof(uint));

            uint value;
            fixed (byte* ptr = Data)
            {
                value = *(uint*)(ptr + Index);
            }
            Index += sizeof(uint);
            return value;
        }

        public unsafe long ReadLong()
        {
            Ensures(Data.Length >= Index + sizeof(long));

            long value;
            fixed (byte* ptr = Data)
            {
                value = *(long*)(ptr + Index);
            }
            Index += sizeof(long);
            return value;
        }

        public unsafe ulong ReadULong()
        {
            Ensures(Data.Length >= Index + sizeof(ulong));

            ulong value;
            fixed (byte* ptr = Data)
            {
                value = *(ulong*)(ptr + Index);
            }
            Index += sizeof(ulong);
            return value;
        }

        public unsafe double ReadDouble()
        {
            Ensures(Data.Length >= Index + sizeof(double));

            double value;
            fixed (byte* ptr = Data)
            {
                value = *(double*)(ptr + Index);
            }
            Index += sizeof(double);
            return value;
        }

        public unsafe float ReadFloat()
        {
            Ensures(Data.Length >= Index + sizeof(float));

            float value;
            fixed (byte* ptr = Data)
            {
                value = *(float*)(ptr + Index);
            }
            Index += sizeof(float);
            return value;
        }

        public unsafe Guid ReadGuid()
        {
            Ensures(Data.Length >= Index + sizeof(Guid));

            Guid value;
            fixed (byte* ptr = Data)
            {
                value = *(Guid*)(ptr + Index);
            }
            Index += sizeof(Guid);
            return value;
        }

        public unsafe DateTime ReadDateTime()
        {
            long ticks = ReadLong();
            var value = DateTime.FromBinary(ticks);
            return value;
        }

        public unsafe byte[] ReadBytes()
        {
            int length = ReadInt();
            var value = new byte[length];
            if (length > 0)
            {
                Buffer.BlockCopy(Data, Index, value, 0, length);
            }
            Index += length;
            return value;
        }

        public unsafe bool[] ReadBools()
        {
            int length = ReadInt();
            var value = new bool[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadBool();
            }
            return value;
        }

        public unsafe ushort[] ReadUShorts()
        {
            int length = ReadInt();
            var value = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUShort();
            }
            return value;
        }

        public unsafe short[] ReadShorts()
        {
            int length = ReadInt();
            var value = new short[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadShort();
            }
            return value;
        }

        public unsafe int[] ReadInts()
        {
            int length = ReadInt();
            var value = new int[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadInt();
            }
            return value;
        }

        public unsafe uint[] ReadUInts()
        {
            int length = ReadInt();
            var value = new uint[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUInt();
            }
            return value;
        }

        public unsafe long[] ReadLongs()
        {
            int length = ReadInt();
            var value = new long[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadLong();
            }
            return value;
        }

        public unsafe ulong[] ReadULongs()
        {
            int length = ReadInt();
            var value = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadULong();
            }
            return value;
        }

        public unsafe double[] ReadDoubles()
        {
            int length = ReadInt();
            var value = new double[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadDouble();
            }
            return value;
        }

        public unsafe float[] ReadFloats()
        {
            int length = ReadInt();
            var value = new float[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadFloat();
            }
            return value;
        }

        public unsafe char ReadChar()
        {
            Ensures(Data.Length >= Index + sizeof(char));
            char value;
            fixed (byte* ptr = Data)
            {
                value = *(char*)(ptr + Index);
            }
            Index += sizeof(char);
            return value;
        }

        public unsafe string ReadString()
        {
            byte[] buffer = ReadBytes();
            var value = Encoding.UTF8.GetString(buffer);
            return value;
        }

        public unsafe string[] ReadStrings()
        {
            int length = ReadInt();
            var value = new string[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadString();
            }
            return value;
        }

        #endregion
    }
}
