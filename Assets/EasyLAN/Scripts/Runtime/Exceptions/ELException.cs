// -----------------------------------------------------------------------
// <copyright file="ELException.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;

    public class ELException : Exception
    {
        public ELException()
            : base()
        {
        }

        public ELException(string message)
            : base(message)
        {
        }
    }
}
