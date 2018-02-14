using System.Linq;
using System.Web.Configuration;
using AdministrativeTools.ActiveDirectory;

namespace AdministrativeTools.Web.Security
{
    public static class WebAuthorization
    {
        public static bool ValidateUser(string userName, string folderPath)
        {
            var section = GetAuthorizationSection(folderPath);

            foreach (AuthorizationRule rule in section.Rules)
            {
                foreach (string user in rule.Users)
                {
                    if (user == "*")
                    {
                        return rule.Action == AuthorizationRuleAction.Allow;
                    }

                    if (user == userName)
                    {
                        return rule.Action == AuthorizationRuleAction.Allow;
                    }
                }

                // Don't forget that users might belong to some roles!
                AD_Group group;
                if (rule.Roles.Cast<string>().Any(role => DirectoryServices.IsMemberInGroup(userName, role, out group)))
                {
                    return rule.Action == AuthorizationRuleAction.Allow;
                }
            }

            return true;
        }

        public static bool ValidateRole(string roleName, string folderPath)
        {
            var section = GetAuthorizationSection(folderPath);

            foreach (AuthorizationRule rule in section.Rules)
            {
                if (rule.Users.Cast<string>().Any(user => user == "*"))
                {
                    return rule.Action == AuthorizationRuleAction.Allow;
                }

                if (rule.Roles.Cast<string>().Any(role => role == roleName))
                {
                    return rule.Action == AuthorizationRuleAction.Allow;
                }
            }

            return true;
        }

        private static AuthorizationSection GetAuthorizationSection(string folderPath)
        {
            var config = WebConfigurationManager.OpenWebConfiguration(folderPath);
            var systemWeb = (SystemWebSectionGroup)config.GetSectionGroup("system.web");

            if (systemWeb != null)
            {
                return (AuthorizationSection)systemWeb.Sections["authorization"];
            }

            return null;
        }
    }
}
