using System;
using System.Runtime.Serialization;

namespace AdministrativeTools.Exceptions
{
    [Serializable]
    public class LdapConnectionException : Exception
    {
        public LdapConnectionException()
        {
        }

        public LdapConnectionException(string message) : base(message)
        {
        }

        public LdapConnectionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LdapConnectionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
