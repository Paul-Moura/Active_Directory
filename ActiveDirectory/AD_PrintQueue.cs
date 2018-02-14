using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_PrintQueue : AD_ConnectionPoint
    {
        internal AD_PrintQueue(DirectoryEntry entry) : this(entry, AD_ObjectClass.PrintQueue)
        {
        }

        internal AD_PrintQueue(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
        }
    }
}
