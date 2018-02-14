using System;

namespace AdministrativeTools.ActiveDirectory
{
    [Flags]
    public enum AD_ObjectClass
    {
        All = int.MaxValue,
        User = 1,
        Group = 2,
        OrganizationalUnit = 4,
        Container = 8,
        ConnectionPoint = 16,
        MsExchActiveSyncDevice = 32,
        MsmqConfiguration = 64,
        Computer = 128 | User,
        GroupPolicyContainer = 256 | Container,
        PrintQueue = 512 | ConnectionPoint,
        ServiceConnectionPoint = 1024 | ConnectionPoint
    }
}
