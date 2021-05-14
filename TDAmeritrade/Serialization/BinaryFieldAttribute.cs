using System;

namespace TDAmeritrade.Serialization
{
    public class BinaryFieldAttribute : Attribute
    {
		public int Index;

        public BinaryFieldAttribute(int index)
        {
			Index = index;
        }
    }
}