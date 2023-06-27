namespace DbDiffChecker.Web.Models
{
    public class DbConnectionSettings
    {
        /// <summary>
        /// Db Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Server Adress
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Server User Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Server User Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Enable Multiple Active Result Sets
        /// </summary>
        public bool MarsEnabled { get; set; }

        /// <summary>
        /// Enable Trusted Connection
        /// </summary>
        public bool TrustedConnection { get; set; }

        /// <summary>
        /// Enable Trust Server Certificate
        /// </summary>
        public bool TrustServerCertificate { get; set; }

        /// <summary>
        /// Parameters is not null and valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid => !string.IsNullOrEmpty(Name) &&
                !string.IsNullOrEmpty(Server) &&
                (!TrustedConnection ? !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) : true);
    }
}
