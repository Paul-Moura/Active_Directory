using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_Group : AD_Member
    {
        public string Description { get; set; }

        public AD_Member[] Members => this.GetMembers();

        #region Constructors

        internal AD_Group(DirectoryEntry entry) : base(entry, AD_ObjectClass.Group)
        {
            //List<string> list = (from PropertyValueCollection property in entry.Properties select $"{property.PropertyName}: {property.Value}").ToList();
            //File.WriteAllLines(@"C:\Users\JMoura\Documents\AD_Group_Properties.txt", list);

            this.Description = entry.Properties["description"].ToString();
        }

        #endregion

        #region Methods

        public AD_Member[] GetMembers()
        {
            var results = new List<AD_Member>();
            PropertyValueCollection colPropertyValues;

            using (var entry = new DirectoryEntry(this.Path))
            {
                colPropertyValues = entry.Properties["member"];
            }

            foreach (string memberPath in colPropertyValues)
            {
                AD_Member member;
                using (var memberEntry = new DirectoryEntry("LDAP://" + memberPath))
                {
                    member = (AD_Member)DirectoryServices.ObjectFactory(memberEntry);
                }
                results.Add(member);
            }

            return results.ToArray();
        }

        public AD_Member GetMember(string name)
        {
            return this.Members.FirstOrDefault(x => x.Name == name);
        }

        public void AddMember(string memberName)
        {
            var member = DirectoryServices.GetObject(memberName, AD_SearchScope.Subtree) as AD_Member;

            if (member == null) throw new Exception();

            this.AddMember(member);
        }

        public void AddMember(AD_Member member)
        {
            AD_Group group;
            if (member.IsMemberOf(this.Name, out group)) throw new Exception();

            try
            {
                using (var entry = new DirectoryEntry(this.Path))
                {
                    entry.Properties["member"].Add(member.Path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void RemoveMember(string memberName)
        {
            var member = DirectoryServices.GetObject(memberName, AD_SearchScope.Subtree) as AD_Member;

            if (member == null) throw new Exception();

            this.RemoveMember(member);
        }

        public void RemoveMember(AD_Member member)
        {
            AD_Group group;
            if (!member.IsMemberOf(this.Name, out group)) throw new Exception();

            try
            {
                using (var entry = new DirectoryEntry(group.Path))
                {
                    entry.Properties["member"].Remove(member.Path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}