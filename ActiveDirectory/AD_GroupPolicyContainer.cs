using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_GroupPolicyContainer : AD_Container
    {
        internal AD_GroupPolicyContainer(DirectoryEntry entry) : this(entry, AD_ObjectClass.GroupPolicyContainer)
        {
        }

        internal AD_GroupPolicyContainer(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
        }
    }
}
