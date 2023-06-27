namespace DbDiffChecker.Data
{
    /// <summary>
    /// Database Table Scheme Model
    /// </summary>
    public class TableDesignData
    {
        /// <summary>
        /// Table Name
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Column Name
        /// </summary>
        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();

        /// <summary>
        /// Table Changes
        /// </summary>
        public List<string> Changes { get; set; } = new List<string>();

        /// <summary>
        /// Table Changed
        /// </summary>
        public bool TableChanged { get; set; } = false;

        /// <summary>
        /// Table Change Type
        /// </summary>
        public ChangeType ChangeType { get; set; } = ChangeType.UnModified;
    }

    /// <summary>
    /// Database Column Scheme Model
    /// </summary>
    public class ColumnModel
    {
        /// <summary>
        /// Column Id
        /// </summary>
        public int ColumnId { get; set; }

        /// <summary>
        /// Column is Primary Key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Max Length
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Max Length String
        /// </summary>
        public string MaxLengthView
        {
            get
            {
                return MaxLength == -1 ? "MAX" : MaxLength.ToString();
            }
        }

        /// <summary>
        /// Decimal precision point
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// Column is changed 
        /// </summary>
        public bool ColumnChanged { get; set; } = false;

        /// <summary>
        /// change type
        /// </summary>
        public ChangeType ChangeType { get; set; } = ChangeType.UnModified;

        /// <summary>
        /// new type
        /// </summary>
        public string NewType { get; set; }

        /// <summary>
        /// new max length
        /// </summary>
        public int NewMaxLength { get; set; }

        /// <summary>
        /// New Precision
        /// </summary>
        public int NewPrecision { get; set; }
    }

    /// <summary>
    /// Sql Table and Column Scheme
    /// </summary>
    public class TableSQLViewDataModel
    {
        /// <summary>
        /// Table Name
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Column Id
        /// </summary>
        public int ColumnId { get; set; }

        /// <summary>
        /// Column Name
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Column Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Column Max Length
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Column Precision
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// Column Is Primary Key
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }

    /// <summary>
    /// Datas with colums for tables
    /// </summary>
    public class DbObject
    {
        /// <summary>
        /// Column Name
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Column Is Primary Key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Column All Datas
        /// </summary>
        public List<DbDataObject> Values { get; set; } = new List<DbDataObject>();

        /// <summary>
        /// Column Type
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Datas with primary key value for columns
    /// </summary>
    public class DbDataObject
    {
        /// <summary>
        /// Id (primary key of table)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Data changes between uat and production databases
    /// </summary>
    public class DataDiffModel
    {
        /// <summary>
        /// Name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Primary Key Id
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Value (Old value if NewValue is not empty)
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// New Value
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Change Type
        /// </summary>
        public ChangeType ChangeType { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string ColumnType { get; set; }
    }

    /// <summary>
    /// Selected changes by the users
    /// </summary>
    public class ChangeModel
    {
        /// <summary>
        /// Object Id (primary key of table)
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Column Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string ColumnName { get; set; }
    }

    public enum ChangeType
    {
        /// <summary>
        /// Table or column deleted
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// Table or column added
        /// </summary>
        Added = 1,

        /// <summary>
        /// Table or column modified
        /// </summary>
        Modified = 2,

        /// <summary>
        /// Table or column unmodified
        /// </summary>
        UnModified = 3
    }
}
