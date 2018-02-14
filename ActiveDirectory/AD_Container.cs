using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_Container : AD_Object
    {
        internal AD_Container(DirectoryEntry entry) : this(entry, AD_ObjectClass.Container)
        {
        }

        internal AD_Container(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
        }
    }
}
