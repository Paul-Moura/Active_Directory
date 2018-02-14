using System;
using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_MsmqConfiguration : AD_Object
    {
        internal AD_MsmqConfiguration(DirectoryEntry entry) : base(entry, AD_ObjectClass.MsmqConfiguration)
        {
        }
    }
}
