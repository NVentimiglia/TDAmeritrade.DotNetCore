using System;

namespace TDAmeritrade
{
    public class FlatBufferHeaderAttribute : Attribute
    {
        public byte Header { get; set; }

        public FlatBufferHeaderAttribute(byte value)
        {
            Header = value;
        }


        public static byte GetHeader<TModel>()
        {
            var at = typeof(TModel).GetCustomAttributes(typeof(FlatBufferHeaderAttribute), false);
            if (at == null || at.Length == 0)
                return 0;
            var at1 = at[0] as FlatBufferHeaderAttribute;
            return at1.Header;
        }
    }
}