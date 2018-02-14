using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_OrganizationalUnit : AD_Object
    {
        internal AD_OrganizationalUnit(DirectoryEntry entry) : base(entry, AD_ObjectClass.OrganizationalUnit)
        {
        }
    }
}
