// -----------------------------------------------------------------------
// <copyright file="Proto.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;

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
