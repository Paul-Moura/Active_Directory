using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_MsExchActiveSyncDevice : AD_Object
    {
        internal AD_MsExchActiveSyncDevice(DirectoryEntry entry) : base(entry, AD_ObjectClass.MsExchActiveSyncDevice)
        {
        }
    }
}
