using System;

namespace AillieoUtils.EasyLAN
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class ProtoAttribute : Attribute
    {
        public readonly byte id;

        public ProtoAttribute(byte id)
        {
            this.id = id;
        }
    }
}
