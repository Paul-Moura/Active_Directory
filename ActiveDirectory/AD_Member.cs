using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace AdministrativeTools.ActiveDirectory
{
    public abstract class AD_Member : AD_Object
    {
        #region Properties

        public string SAMAccountName { get; }

        public AD_Group[] MemberOf => this.GetMemberOf();

        #endregion

        #region Constructors

        protected AD_Member(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
            this.SAMAccountName = entry.Properties["sAMAccountName"].Value.ToString();
        }

        #endregion

        #region Methods

        public AD_Group[] GetMemberOf()
        {
            var results = new List<AD_Group>();
            PropertyValueCollection colPropertyValues;

            using (var entry = new DirectoryEntry(this.Path))
            {
                colPropertyValues = entry.Properties["memberOf"];
            }

            foreach (string groupPath in colPropertyValues)
            {
                AD_Group group;
                using (var groupEntry = new DirectoryEntry("LDAP://" + groupPath))
                {
                    group = (AD_Group)DirectoryServices.ObjectFactory(groupEntry);
                }
                results.Add(group);
            }

            return results.ToArray();
        }

        public bool IsMemberOf(string groupName, out AD_Group group)
        {
            var tempGroup = DirectoryServices.GetObject(groupName, AD_SearchScope.Subtree) as AD_Group;

            if (tempGroup == null) throw new Exception(); //TODO: throw custom exception

            List<string> groupsChecked = new List<string>();

            return IsInGroupsMemberOf(this.Name, tempGroup, ref groupsChecked, out group);
        }
        
        private static bool IsInGroupsMemberOf(string memberName, AD_Group tempGroup, ref List<string> groupsChecked, out AD_Group group)
        {
            groupsChecked.Add(tempGroup.Name);

            var members = tempGroup.GetMembers();

            foreach (AD_Member member in members)
            {
                if (string.Equals(member.Name, memberName, StringComparison.CurrentCultureIgnoreCase))
                {
                    group = tempGroup;
                    return true;
                }

                if (member is AD_Group)
                {
                    if (groupsChecked.Contains(member.Name)) continue;

                    if (IsInGroupsMemberOf(memberName, (AD_Group) member, ref groupsChecked, out group))
                    {
                        return true;
                    }
                }
            }
            
            group = null;
            return false;
        }

        #endregion
    }
}
