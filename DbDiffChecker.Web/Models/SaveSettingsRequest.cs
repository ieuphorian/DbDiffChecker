namespace DbDiffChecker.Web.Models
{
    public class SaveSettingsRequest
    {
        /// <summary>
        /// Uat Db Settings
        /// </summary>
        public DbConnectionSettings UatDbSettings { get; set; }

        /// <summary>
        /// Prod Db Settings
        /// </summary>
        public DbConnectionSettings ProdDbSettings { get; set; }

        /// <summary>
        /// Excluded Tables when Comparing Design
        /// </summary>
        public List<string> ExcludedDesignTables { get; set; } = new List<string>();


        /// <summary>
        /// Excluded Tables when Comparing Data
        /// </summary>
        public List<string> ExcludedDataTables { get; set; } = new List<string>();
    }
}
