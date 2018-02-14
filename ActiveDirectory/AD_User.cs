using System.DirectoryServices;

namespace AdministrativeTools.ActiveDirectory
{
    public class AD_User : AD_Member
    {
        #region Variables

        private string _firstName;
        private string _initials;
        private string _lastName;
        private string _email;
        private string _password;

        #endregion

        #region Properties

        public string DisplayName => string.IsNullOrWhiteSpace(this.FirstName + this.LastName)
            ? this.UserName
            : (this.FirstName + " " + (string.IsNullOrWhiteSpace(this.Initials) ? "" : this.Initials + ". ") + this.LastName).Trim();

        public string Email
        {
            get { return this._email; }
            set
            {
                if (value == null) return;
                this._email = value;
            }
        }

        public bool Enabled { get; private set; }

        public string FirstName
        {
            get { return this._firstName; }
            set
            {
                if (value == null) return;
                this._firstName = value;
            }
        }

        public string Initials
        {
            get { return this._initials; }
            set
            {
                if (value == null) return;
                this._initials = value;
            }
        }

        public string LastName
        {
            get { return this._lastName; }
            set
            {
                if (value == null) return;
                this._lastName = value;
            }
        }

        public string Password
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                this._password = value;
            }
        }

        public string UserName => this.SAMAccountName;

        #endregion

        #region Constructors

        internal AD_User(DirectoryEntry entry) : this(entry, AD_ObjectClass.User)
        {
            
        }

        internal AD_User(DirectoryEntry entry, AD_ObjectClass oClass) : base(entry, oClass)
        {
            this.FirstName = (entry.Properties["givenname"].Value ?? "").ToString();
            this.Initials = (entry.Properties["initials"].Value ?? "").ToString();
            this.LastName = (entry.Properties["sn"].Value ?? "").ToString();
            this.Email = (entry.Properties["mail"].Value ?? "").ToString();
            this.Enabled = ((int)entry.Properties["userAccountControl"].Value & 2) == 0;
        }

        #endregion

        #region Methods

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return; //TODO: throw custom exception

            using (var entry = new DirectoryEntry(this.Path))
            {
                entry.Invoke("SetPassword", password);
                entry.CommitChanges();
            }
        }
        
        protected override void CommitChanges(DirectoryEntry entry)
        {
            //base.CommitChanges(entry);

            if (!string.IsNullOrWhiteSpace(this._password))
            {
                entry.Invoke("SetPassword", this._password);
                this._password = "";
            }

            entry.Properties["givenname"].Value = this.FirstName;
            entry.Properties["initials"].Value = this.Initials;
            entry.Properties["sn"].Value = this.LastName;
            entry.Properties["displayName"].Value = this.DisplayName;
            entry.Properties["mail"].Value = this.Email;
        }

        #endregion
    }
}
