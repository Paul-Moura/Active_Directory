using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_Computer : AD_User
    {
        internal AD_Computer(DirectoryEntry entry) : base(entry, AD_ObjectClass.Computer)
        {
        }
    }
}
