using System;
using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_ConnectionPoint : AD_Object
    {
        internal AD_ConnectionPoint(DirectoryEntry entry) : this(entry, AD_ObjectClass.ConnectionPoint)
        {
        }

        internal AD_ConnectionPoint(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
        }
    }
}
