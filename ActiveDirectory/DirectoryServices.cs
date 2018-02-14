using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using AdministrativeTools.Exceptions;
using SearchScope = System.DirectoryServices.SearchScope;

namespace AdministrativeTools.ActiveDirectory
{
    /// <summary>
    /// Initializes default connection to CBO Domain.
    /// </summary>
    public static class DirectoryServices
    {
        #region Variables

        private static string _serverAddress;

        private static string _domainComponent;

        #endregion

        #region Properties
        /// <summary>
        /// <example>
        /// <para>DirectoryServices.Ldap =  @"LDAP://cbo.local/DC=cbo,DC=local";</para>
        /// <para>DirectoryServices.Ldap =  "cbo.local";</para>
        /// </example>
        /// </summary>
        public static string Ldap
        {
            get => $"LDAP://{_serverAddress}/{_domainComponent}";
            set
            {
                var ldap = FormatLdap(value);
                var dirs = ldap.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                _serverAddress = dirs[1];
                _domainComponent = dirs[2];
            }
        }
        
        #endregion

        //#region Constructors

        //static DirectoryServices()
        //{
        //    Ldap = @"LDAP://cbo.local/DC=cbo,DC=local";
        //}

        //#endregion

        #region Methods

        internal static string FormatLdap(string path)
        {
            if (TestLdapConnection(path)) return path;
            
            var dirs = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            string root;
            var component = "";

            if (path.ToUpper().StartsWith(@"LDAP://"))
            {
                var dcs = dirs[1].Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                component = dcs.Aggregate(component, (current, dc) => current + ((current == "" ? "" : ",") + $"DC={dc}"));
                
                root = @"LDAP://" + dirs[1] + "/";
                dirs[0] = dirs[1] = "";
            }
            else if (path.StartsWith("."))
            {
                root = @"LDAP://" + _serverAddress + "/";
                component = _domainComponent;
                dirs[0] = "";
            }
            else
            {
                var dcs = dirs[0].Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                component = dcs.Aggregate(component, (current, dc) => current + ((current == "" ? "" : ",") + $"DC={dc}"));

                root = @"LDAP://" + dirs[0] + "/";
                dirs[0] = "";
            }

            if (!TestLdapConnection(root + component))
                throw new LdapConnectionException(root + component + " does not exist.");
            
            foreach (var dir in dirs.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (!dir.StartsWith("CN") && !dir.StartsWith("OU") && !dir.StartsWith("DC"))
                {
                    if (TestLdapConnection(root + "CN=" + dir + 
                        (string.IsNullOrWhiteSpace(component) ? "" : "," + component)))
                    {
                        component = "CN=" + dir + (string.IsNullOrWhiteSpace(component) ? "" : "," + component);
                        continue;
                    }

                    if (TestLdapConnection(root + "OU=" + dir +
                        (string.IsNullOrWhiteSpace(component) ? "" : "," + component)))
                    {
                        component = "OU=" + dir + (string.IsNullOrWhiteSpace(component) ? "" : "," + component);
                        continue;
                    }

                    if (TestLdapConnection(root + "DC=" + dir +
                                           (string.IsNullOrWhiteSpace(component) ? "" : "," + component)))
                    {
                        component = "DC=" + dir + (string.IsNullOrWhiteSpace(component) ? "" : "," + component);
                        continue;
                    }

                    if (component.Contains($"DC={dir}")) continue;

                    throw new LdapConnectionException(root + component + 
                        " does not contain a CN, OU, or DC with the name '" + dir + "'.");
                }

                if (TestLdapConnection(root + dir +
                                       (string.IsNullOrWhiteSpace(component) ? "" : "," + component)))
                {
                    component = dir + (string.IsNullOrWhiteSpace(component) ? "" : "," + component);
                }
                else
                {
                    if (component.Contains(dir)) continue;

                    throw new LdapConnectionException(root + dir +
                                       (string.IsNullOrWhiteSpace(component) ? "" : "," + component) 
                                       + " does not exist.");
                }
            }

            return root + component;
        }

        internal static bool TestLdapConnection(string ldap)
        {
            try
            {
                using (var entry = new DirectoryEntry(ldap))
                {
                    var o = entry.NativeObject;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static string GenerateFilter(AD_ObjectClass oClass)
        {
            if (oClass == AD_ObjectClass.All)
                return "(objectClass=*)";

            var filter = "";

            if (oClass.HasFlag(AD_ObjectClass.User))
            {
                if (oClass.HasFlag(AD_ObjectClass.Computer))
                {
                    filter += "(objectClass=computer)";
                }
                else
                {
                    filter += "(objectClass=user)";
                }
            }

            if (oClass.HasFlag(AD_ObjectClass.Group))
            {
                filter += "(objectClass=group)";
            }

            if (oClass.HasFlag(AD_ObjectClass.OrganizationalUnit))
            {
                filter += "(objectClass=organizationalUnit)";
            }

            if (oClass.HasFlag(AD_ObjectClass.Container))
            {
                filter += "(objectClass=container)";
            }
            
            //TODO: add the rest of the class types

            return string.IsNullOrWhiteSpace(filter) ? "(objectClass=*)" : $"(|{filter})";
        }
        
        internal static AD_Object ObjectFactory(DirectoryEntry entry)
        {
            var oClass = (object[])entry.Properties["objectClass"].Value;
            var className = oClass[oClass.Length - 1];
            
            switch (className.ToString())
            {
                case "user":
                    return new AD_User(entry);

                case "computer":
                    return new AD_Computer(entry);

                case "group":
                    return new AD_Group(entry);

                case "organizationalUnit":
                    return new AD_OrganizationalUnit(entry);

                case "connectionPoint":
                    return new AD_ConnectionPoint(entry);

                case "printQueue":
                    return new AD_PrintQueue(entry);

                case "serviceConnectionPoint":
                    return new AD_ServiceConnectionPoint(entry);

                case "msExchActiveSyncDevice":
                    return new AD_MsExchActiveSyncDevice(entry);

                case "groupPolicyContainer":
                    return new AD_GroupPolicyContainer(entry);

                case "mSMQConfiguration":
                    return new AD_MsmqConfiguration(entry);

                case "container":
                    return new AD_Container(entry);
            }

            return null;
        }
        
        public static AD_Object[] GetObjects(AD_ObjectClass oClass = AD_ObjectClass.All, AD_SearchScope scope = AD_SearchScope.Base)
        {
            return GetObjects(Ldap, oClass, scope);
        }

        internal static AD_Object[] GetObjects(string ldap, AD_ObjectClass oClass = AD_ObjectClass.All, AD_SearchScope scope = AD_SearchScope.Base)
        {
            List<AD_Object> list = new List<AD_Object>();

            ldap = string.IsNullOrWhiteSpace(ldap) ? Ldap : FormatLdap(ldap);

            var filter = GenerateFilter(oClass);

            using (var searcher = new DirectorySearcher(new DirectoryEntry(ldap))
            {
                SearchScope = (SearchScope)(int)scope,
                Filter = filter
            })
            {
                var searchResults = searcher.FindAll();

                foreach (SearchResult result in searchResults)
                {
                    using (var entry = result.GetDirectoryEntry())
                    {
                        var o = ObjectFactory(entry);
                        if (o != null) list.Add(o);
                    }
                }

                if (list.Count > 1)
                {
                    list = list.OrderBy(x => x.Path).ToList();
                }
            }

            return list.ToArray();
        }

        public static AD_Object GetObject(Guid guid)
        {
            DirectoryEntry entry = new DirectoryEntry($"LDAP://<GUID={guid}>");
            return ObjectFactory(entry);
        }

        public static AD_Object GetObject(string name, AD_SearchScope scope = AD_SearchScope.Base)
        {
            return GetObject(Ldap, name, scope);
        }

        internal static AD_Object GetObject(string ldap, string name, AD_SearchScope scope = AD_SearchScope.Base)
        {
            var index = name.IndexOf("\\", StringComparison.Ordinal);
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }
            return GetObject(ldap, scope, $"(|(name={name})(sAMAccountName={name}))");
        }
        
        private static AD_Object GetObject(string ldap, AD_SearchScope scope, string filter)
        {
            try
            {
                var searcher = new DirectorySearcher(new DirectoryEntry(ldap))
                {
                    SearchScope = (SearchScope)(int)scope,
                    Filter = filter
                };

                var searchResults = searcher.FindOne();

                if (searchResults == null) return null;

                var entry = searchResults.GetDirectoryEntry();

                return ObjectFactory(entry);
            }
            catch
            {
                return null;
            }
        }

        public static AD_Object CreateObject(string name, AD_ObjectClass oClass)
        {
            return CreateObject(Ldap, name, oClass);
        }

        internal static AD_Object CreateObject(string ldap, string name, AD_ObjectClass oClass)
        {
            AD_Object o;
            var className = ObjectClassToString(oClass);

            if (className == null) return null;

            try
            {
                using (var adam = new DirectoryEntry(Ldap))
                {
                    adam.RefreshCache();

                    using (var entry = adam.Children.Add("CN=" + name, className))
                    {
                        adam.CommitChanges();
                        entry.Properties["samAccountName"].Value = name;
                        entry.CommitChanges();
                        //adam.CommitChanges();

                        o = ObjectFactory(entry);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return o;
        }

        internal static string ObjectClassToString(AD_ObjectClass oClass)
        {
            switch (oClass)
            {
                case AD_ObjectClass.User:
                    return "user";
                case AD_ObjectClass.Group:
                    return "group";
                case AD_ObjectClass.OrganizationalUnit:
                    return "organizationalUnit";
                case AD_ObjectClass.Container:
                    return "container";
                case AD_ObjectClass.ConnectionPoint:
                    return "connectionPoint";
                case AD_ObjectClass.MsExchActiveSyncDevice:
                    return "msExchActiveSyncDevice";
                case AD_ObjectClass.MsmqConfiguration:
                    return "msmqConfiguration";
                case AD_ObjectClass.Computer:
                    return "computer";
                case AD_ObjectClass.GroupPolicyContainer:
                    return "groupPolicyContainer";
                case AD_ObjectClass.PrintQueue:
                    return "printQueue";
                case AD_ObjectClass.ServiceConnectionPoint:
                    return "serviceConnectionPoint";
                default:
                    return null;
            }
        }
        
        public static void AddMemberToGroup(string memberName, string groupName)
        {
            var group = GetObject(groupName) as AD_Group;

            if (group == null) throw new ArgumentException("The specified group does not exist.", nameof(groupName));

            group.AddMember(memberName);
        }

        public static void RemoveMemberFromGroup(string memberName, string groupName)
        {
            var group = GetObject(groupName) as AD_Group;

            if (group == null) throw new ArgumentException("The specified group does not exist.", nameof(groupName));

            group.RemoveMember(memberName);
        }

        public static bool IsMemberInGroup(string memberName, string groupName, out AD_Group group)
        {
            var member = GetObject(memberName) as AD_Member;

            if (member == null) throw new ArgumentException("The specified member does not exist.", nameof(memberName));

            return member.IsMemberOf(groupName, out group);
        }

        /// <summary>
        /// Gets a SAM Account Name from a given LDAP path
        /// </summary>
        /// <param name="pstrPath">LDAP path to bind to</param>
        internal static string GetSAMAccountName(string pstrPath)
        {
            try
            {
                using (var entry = new DirectoryEntry("LDAP://" + pstrPath))
                {
                    return entry.Properties["sAMAccountName"].Value.ToString();
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return string.Empty;
            }
            catch (NullReferenceException)
            {
                return string.Empty;
            }
        }

        public static bool ValidateCredentials(string userName, string password)
        {
            try
            {
                LdapConnection connection = new LdapConnection(_serverAddress);
                NetworkCredential credential = new NetworkCredential(userName, password);
                connection.Credential = credential;
                connection.Bind();
                return true;
            }
            catch (LdapException lexc)
            {
                return lexc.ServerErrorMessage.Contains("AcceptSecurityContext error, data 773");
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        private static void WriteEntryToFile(DirectoryEntry entry)
        {
            var oClass = (object[])entry.Properties["objectClass"].Value;
            var className = oClass[oClass.Length - 1];

            string file = $@"C:\AD\{className}.txt";

            if (System.IO.File.Exists(file)) return;

            string text = "";
            foreach (PropertyValueCollection collection in entry.Properties)
            {
                text += collection.PropertyName + Environment.NewLine;
                text += ObjectToString(collection) + Environment.NewLine;
            }
            System.IO.File.WriteAllText(file, text);
        }

        private static string ObjectToString(object o)
        {
            string text = "";

            var arr = o as System.Collections.IEnumerable;
            if (arr != null && !(o is string))
            {
                text += o + "::" + Environment.NewLine;
                text = arr.Cast<object>().Aggregate(text, (current, a) => current + ObjectToString(a));
            }
            else
            {
                text += o + Environment.NewLine;
            }

            return text;
        }
    }
}