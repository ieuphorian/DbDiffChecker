using DbDiffChecker.Data;

namespace DbDiffChecker.Service.DbDesign
{
    public interface IDbDesignService
	{
		/// <summary>
		/// Get All Tables with column and types
		/// </summary>
		/// <returns></returns>
		List<TableDesignData> GetAllTablesWithColumns();

		/// <summary>
		/// Check Differences between uat and prod columns
		/// </summary>
		/// <param name="table">Table Name</param>
		/// <returns></returns>
		ReturnData<List<DataDiffModel>> UatProductionDbDataDiffs(string table);

		/// <summary>
		/// Update changes to prod based user's selected changes (changes only based on data scheme will not change)
		/// </summary>
		/// <param name="table">Table Name</param>
		/// <param name="model">User's selected changes</param>
		/// <returns></returns>
		ReturnData<string> UpdateChanges(string table, List<ChangeModel> model);

        /// <summary>
        /// Create sql file for changes uat-prod based user's selected changes (changes only based on data scheme will not change)
        /// </summary>
        /// <param name="table">Table Name</param>
        /// <param name="model">User's selected changes</param>
        /// <returns></returns>
        ReturnData<string> CreateSqlFile(string table, List<ChangeModel> model);
	}
}
