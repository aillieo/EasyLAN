using System;

namespace AillieoUtils.EasyLAN
{
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
