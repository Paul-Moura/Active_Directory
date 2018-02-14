using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_ServiceConnectionPoint : AD_ConnectionPoint
    {
        internal AD_ServiceConnectionPoint(DirectoryEntry entry) : this(entry, AD_ObjectClass.ServiceConnectionPoint)
        {
        }

        internal AD_ServiceConnectionPoint(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
        }
    }
}
