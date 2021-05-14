using System;
using System.Text;
#if UNITY
using UnityEngine;
using Unity.Collections;
#endif

namespace TDAmeritrade
{ /// <summary>
  ///     Datamodel serialization helper
  /// </summary>
    public class BitSerializer : IDisposable
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
        /// IS WRITING TO THE STREAM (SERIALIZING, false for DESERIALIZING)
        /// </summary>
        public bool IsWriting;

        /// <summary>
        /// Read Data index. Position.
        /// </summary>
        public int Index;

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// ctor
        /// </summary>
        public BitSerializer() : this(BUFFER_SIZE)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public BitSerializer(int size)
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
        /// returns copy of written buffer
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

        public unsafe byte PeekByte()
        {
            Ensures(Data.Length >= Index + sizeof(byte));

            return Data[Index];
        }

        public unsafe sbyte PeekSByte()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));
            return (sbyte)Data[Index];
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

        public unsafe ushort PeekUShort()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            ushort value;

            fixed (byte* ptr = Data)
            {
                value = *(ushort*)(ptr + Index);
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

        public unsafe uint PeekUInt()
        {
            Ensures(Data.Length >= Index + sizeof(sbyte));

            uint value;

            fixed (byte* ptr = Data)
            {
                value = *(uint*)(ptr + Index);
            }
            return value;
        }

        public unsafe Guid PeekGuid()
        {
            Ensures(Data.Length >= Index + sizeof(Guid));
            Guid value;

            fixed (byte* ptr = Data)
            {
                value = *(Guid*)(ptr + Index);
            }
            return value;
        }
        #endregion

        #region PRIMITIVES

        public unsafe void Parse(ref byte value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(byte));

                Data[Index] = value;
                Index += sizeof(byte);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(byte));

                value = Data[Index];
                Index += sizeof(byte);
            }
        }

        public byte Parse(byte value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref sbyte value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(sbyte));

                Data[Index] = (byte)value;
                Index += sizeof(sbyte);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(sbyte));

                value = (sbyte)Data[Index];
                Index += sizeof(sbyte);
            }
        }

        public sbyte Parse(sbyte value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref bool value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(bool));

                fixed (byte* ptr = Data)
                {
                    *(bool*)(ptr + Index) = value;
                }
                Index += sizeof(bool);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(bool));

                fixed (byte* ptr = Data)
                {
                    value = *(bool*)(ptr + Index);
                }
                Index += sizeof(bool);
            }
        }

        public bool Parse(bool value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref short value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(short));

                fixed (byte* ptr = Data)
                {
                    *(short*)(ptr + Index) = value;
                }
                Index += sizeof(short);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(short));

                fixed (byte* ptr = Data)
                {
                    value = *(short*)(ptr + Index);
                }
                Index += sizeof(short);
            }
        }

        public unsafe void Parse(ref ushort value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(ushort));

                fixed (byte* ptr = Data)
                {
                    *(ushort*)(ptr + Index) = value;
                }
                Index += sizeof(ushort);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(ushort));

                fixed (byte* ptr = Data)
                {
                    value = *(ushort*)(ptr + Index);
                }
                Index += sizeof(ushort);
            }
        }

        public ushort Parse(ushort value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref int value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(int));

                fixed (byte* ptr = Data)
                {
                    *(int*)(ptr + Index) = value;
                }
                Index += sizeof(int);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(int));

                fixed (byte* ptr = Data)
                {
                    value = *(int*)(ptr + Index);
                }
                Index += sizeof(int);
            }
        }

        public int Parse(int value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref uint value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(uint));

                fixed (byte* ptr = Data)
                {
                    *(uint*)(ptr + Index) = value;
                }
                Index += sizeof(uint);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(uint));

                fixed (byte* ptr = Data)
                {
                    value = *(uint*)(ptr + Index);
                }
                Index += sizeof(uint);
            }
        }

        public uint Parse(uint value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref long value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(long));

                fixed (byte* ptr = Data)
                {
                    *(long*)(ptr + Index) = value;
                }
                Index += sizeof(long);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(long));

                fixed (byte* ptr = Data)
                {
                    value = *(long*)(ptr + Index);
                }
                Index += sizeof(long);
            }
        }

        public long Parse(long value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref ulong value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(ulong));

                fixed (byte* ptr = Data)
                {
                    *(ulong*)(ptr + Index) = value;
                }
                Index += sizeof(ulong);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(ulong));

                fixed (byte* ptr = Data)
                {
                    value = *(ulong*)(ptr + Index);
                }
                Index += sizeof(ulong);
            }
        }

        public ulong Parse(ulong value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref double value)
        {

            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(double));

                fixed (byte* ptr = Data)
                {
                    *(double*)(ptr + Index) = value;
                }
                Index += sizeof(double);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(double));

                fixed (byte* ptr = Data)
                {
                    value = *(double*)(ptr + Index);
                }
                Index += sizeof(double);
            }
        }

        public double Parse(double value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref float value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(float));

                fixed (byte* ptr = Data)
                {
                    *(float*)(ptr + Index) = value;
                }
                Index += sizeof(float);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(float));

                fixed (byte* ptr = Data)
                {
                    value = *(float*)(ptr + Index);
                }
                Index += sizeof(float);
            }
        }

        public float Parse(float value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref Guid value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(Guid));

                fixed (byte* ptr = Data)
                {
                    *(Guid*)(ptr + Index) = value;
                }
                Index += sizeof(Guid);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(Guid));

                fixed (byte* ptr = Data)
                {
                    value = *(Guid*)(ptr + Index);
                }
                Index += sizeof(Guid);
            }
        }

        public Guid Parse(Guid value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref DateTime value)
        {
            long ticks = value.ToBinary();
            Parse(ref ticks);
            value = DateTime.FromBinary(ticks);
        }

        public DateTime Parse(DateTime value)
        {
            Parse(ref value);
            return value;
        }

        #endregion

        #region ARRAYS

        public unsafe void Parse(ref byte[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                Buffer.BlockCopy(value, 0, Data, Index, value.Length);

                Index += length;
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new byte[length];
                if (length > 0)
                {
                    Buffer.BlockCopy(Data, Index, value, 0, length);
                }
                Index += length;
            }
        }

        public unsafe byte[] Parse(byte[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref bool[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new bool[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe bool[] Parse(bool[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref ushort[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new ushort[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe ushort[] Parse(ushort[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref short[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new short[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe short[] Parse(short[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref int[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new int[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe int[] Parse(int[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref uint[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new uint[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }
        public unsafe uint[] Parse(uint[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref long[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new long[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe long[] Parse(long[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref ulong[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new ulong[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }
        public unsafe ulong[] Parse(ulong[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref double[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new double[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe double[] Parse(double[] value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref float[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new float[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe float[] Parse(float[] value)
        {
            Parse(ref value);
            return value;
        }

        #endregion

        #region STRINGS

        public unsafe void Parse(ref char value)
        {
            if (IsWriting)
            {
                Ensures(Data.Length >= Index + sizeof(char));

                fixed (byte* ptr = Data)
                {
                    *(char*)(ptr + Index) = value;
                }
                Index += sizeof(char);
            }
            else
            {
                Ensures(Data.Length >= Index + sizeof(char));

                fixed (byte* ptr = Data)
                {
                    value = *(char*)(ptr + Index);
                }
                Index += sizeof(char);
            }
        }

        public unsafe char Parse(char value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref string value)
        {
            if (IsWriting)
            {
                if (value == null)
                {
                    int empty = 0;
                    Parse(ref empty);
                }
                else
                {
                    byte[] data;
                    data = Encoding.UTF8.GetBytes(value);
                    Parse(ref data);
                }
            }
            else
            {
                byte[] buffer = null;
                Parse(ref buffer);

                value = Encoding.UTF8.GetString(buffer);
            }
        }

        public unsafe string Parse(string value)
        {
            Parse(ref value);
            return value;
        }

        public unsafe void Parse(ref string[] value)
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                value = new string[length];
                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe string[] Parse(string[] value)
        {
            Parse(ref value);
            return value;
        }

        #endregion

        #region Native
#if UNITY        
        public unsafe void Parse<T>(ref NativeArray<T> value) where T : struct, IBufferModel
        {
            if (IsWriting)
            {
                var length = value.Length;
                Parse(ref length);

                for (int i = 0; i < value.Length; i++)
                {
                    var v = value[i];
                    Parse(ref v);
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                for (int i = 0; i < length; i++)
                {
                    T v = default(T);
                    Parse(ref v);
                    value[i] = v;
                }
            }
        }

        public unsafe void Parse<T>(ref NativeSlice<T> value) where T : struct, IBufferModel
        {
            if (IsWriting)
            {
                var length = value.Length;
                Parse(ref length);

                for (int i = 0; i < value.Length; i++)
                {
                    var v = value[i];
                    Parse(ref v);
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                for (int i = 0; i < length; i++)
                {
                    T v = default(T);
                    Parse(ref v);
                    value[i] = v;
                }
            }
        }
#endif
        #endregion

        #region OBJECTS

        /// <summary>
        /// READ / WRITE BASED ON this.IsWriting FLAG
        /// </summary>
        public unsafe void Parse<T>(ref T value) where T : IBitModel, new()
        {
            value.Parse(this);
        }
        public unsafe T Parse<T>(T value) where T : IBitModel, new()
        {
            Parse(ref value);
            return value;
        }

        /// <summary>
        /// READ / WRITE BASED ON this.IsWriting FLAG
        /// </summary>
        public unsafe void Parse<T>(ref T[] value) where T : IBitModel, new()
        {
            if (IsWriting)
            {
                var length = value == null ? 0 : value.Length;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        Parse(ref value[i]);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                if (value == null)
                    value = new T[length];
                else if (value.Length != length)
                    Array.Resize(ref value, length);

                for (int i = 0; i < length; i++)
                {
                    Parse(ref value[i]);
                }
            }
        }

        public unsafe T[] Parse<T>(T[] value) where T : IBitModel, new()
        {
            Parse(ref value);
            return value;
        }

        /// <summary>
        /// READ / WRITE BASED ON this.IsWriting FLAG
        /// </summary>
        public unsafe void Parse<T>(ref System.Collections.Generic.List<T> value) where T : IBitModel, new()
        {
            if (value == null)
                value = new System.Collections.Generic.List<T>();

            if (IsWriting)
            {
                var length = value.Count;
                Parse(ref length);

                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        var val = value[i];
                        Parse(ref val);
                    }
                }
            }
            else
            {
                int length = 0;
                Parse(ref length);

                //ordered
                for (int i = 0; i < length; i++)
                {
                    T _item = new T();
                    Parse(ref _item);
                    if (i >= value.Count)
                        value.Add(_item);
                    else
                        value[i] = _item;
                }

                //prune end
                if (value.Count > length)
                {
                    value.RemoveRange(length, value.Count - length);
                }
            }
        }

        public unsafe System.Collections.Generic.List<T> Parse<T>(System.Collections.Generic.List<T> value) where T : struct, IBitModel
        {
            Parse(ref value);
            return value;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Reads a instance of this model from the internal buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Read<T>() where T : IBitModel, new()
        {
            IsWriting = false;
            var model = new T();
            model.Parse(this);
            return model;
        }

        /// <summary>
        /// Writes the model into the internal buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        public void Write<T>(T model) where T : IBitModel, new()
        {
            IsWriting = true;
            model.Parse(this);
        }

        /// <summary>
        /// Model to bytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public byte[] WriteCopy<T>(T model) where T : IBitModel, new()
        {
            IsWriting = true;
            model.Parse(this);
            return Copy();
        }

        /// <summary>
        /// Reads from a passed buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">the array read to</param>
        /// <returns></returns>
        public T ReadFrom<T>(byte[] payload) where T : IBitModel, new()
        {
            var model = new T();

            Reset();
            IsWriting = true;

            var original = Data;
            Data = payload;
            try
            {
                model.Parse(this);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Data = original;
            }
            return model;
        }

        /// <summary>
        /// Writes to a buffer array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="payload">the array written to</param>
        public void WriteTo<T>(T model, byte[] payload) where T : IBitModel, new()
        {
            Reset();
            IsWriting = false;

            var original = Data;
            Data = payload;
            try
            {
                model.Parse(this);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Data = original;
            }
        }
        #endregion
    }
}
