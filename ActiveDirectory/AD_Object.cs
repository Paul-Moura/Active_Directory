using System;
using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public abstract class AD_Object
    {
        #region Properties

        public AD_ObjectClass ObjectClass { get; }

        public string Path { get; }

        public Guid Guid { get; }

        public string Name { get; }

        public string Description { get; }

        public string DistinguishedName { get; }

        public DateTime Created { get; }

        public DateTime Changed { get; private set; }

        public string ObjectCategory { get; }

        public bool? IsCriticalSystemObject { get; }

        public string CN
        {
            get
            {
                int i = this.DistinguishedName.IndexOf(',');
                return this.DistinguishedName.Substring(0, i);
            }
        }

        protected string ParentCN => this.Path.Replace(this.CN + ",", "");

        public AD_Object Parent => DirectoryServices.ObjectFactory(new DirectoryEntry(this.ParentCN));

        #endregion

        #region Constructors

        protected AD_Object(DirectoryEntry entry, AD_ObjectClass oClass)
        {
            this.ObjectClass = oClass;
            this.Path = entry.Path ?? "";
            this.Guid = new Guid((byte[])entry.Properties["objectGUID"].Value);
            this.Name = entry.Properties["name"].Value.ToString();
            this.Description = entry.Properties["description"].Value?.ToString() ?? "";
            this.DistinguishedName = entry.Properties["distinguishedName"].Value.ToString();
            this.Created = (DateTime)entry.Properties["whenCreated"].Value;
            this.Changed = (DateTime)entry.Properties["whenChanged"].Value;
            this.ObjectCategory = entry.Properties["objectCategory"].Value.ToString();
            this.IsCriticalSystemObject = (bool?)entry.Properties["isCriticalSystemObject"].Value;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.ObjectClass}:{this.Path}";
        }

        protected virtual void CommitChanges(DirectoryEntry entry) { }

        public void CommitChanges()
        {
            using (var entry = new DirectoryEntry(this.Path))
            {
                this.CommitChanges(entry);

                entry.CommitChanges();

                using (var adamEntry = new DirectoryEntry(this.ParentCN))
                {
                    adamEntry.CommitChanges();
                }

                this.Changed = (DateTime)entry.Properties["whenChanged"].Value;
            }
        }
        
        public AD_Object[] GetObjects(AD_ObjectClass oClass = AD_ObjectClass.All, AD_SearchScope scope = AD_SearchScope.Base)
        {
            return DirectoryServices.GetObjects(this.Path, oClass, scope);
        }

        public AD_Object GetObject(string name)
        {
            return DirectoryServices.GetObject(this.Path, name);
        }

        public void AddObject(AD_Object member)
        {
            throw new NotImplementedException();
        }

        public void CreateObject(string name, AD_ObjectClass oClass)
        {
            DirectoryServices.CreateObject(this.Path, name, oClass);
        }

        public void DeleteObject(string name)
        {
            throw new NotImplementedException();
        }
        
        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
