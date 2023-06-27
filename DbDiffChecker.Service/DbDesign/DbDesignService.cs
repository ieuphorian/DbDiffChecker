using DbDiffChecker.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace DbDiffChecker.Service.DbDesign
{
    public class DbDesignService : IDbDesignService
    {
        #region Initialize
        private readonly IConfiguration _configuration;
        public DbDesignService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Methods

        /// <inheritdoc/>
        public List<TableDesignData> GetAllTablesWithColumns()
        {
            var uatData = new List<TableSQLViewDataModel>();
            var prodData = new List<TableSQLViewDataModel>();
            #region UAT Tables And Columns
            using (var cnn = new SqlConnection(_configuration.GetConnectionString("ConnectionString_UAT")))
            {
                cnn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = @"
                        SELECT tab.name as table_name, 
						    col.column_id,
							(SELECT COUNT(column_name)
							FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE
								TABLE_NAME = tab.name AND
								CONSTRAINT_NAME =
								(SELECT
									CONSTRAINT_NAME
								FROM 
									INFORMATION_SCHEMA.TABLE_CONSTRAINTS
								WHERE
									TABLE_NAME = tab.name AND                    
									CONSTRAINT_TYPE = 'PRIMARY KEY' AND
									COLUMN_NAME = col.name)) AS is_primary_key,
						    col.name as column_name, 
						    t.name as data_type,    
						    col.max_length,
						    col.precision
						FROM sys.tables as tab
						    INNER JOIN sys.columns as col
						        on tab.object_id = col.object_id
						    LEFT JOIN sys.types as t
						    on col.user_type_id = t.user_type_id 
						ORDER BY table_name, 
						    column_id;";
                    cmd.Connection = cnn;
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var entity = new TableSQLViewDataModel();
                        entity.Table = dr.GetString("table_name");
                        entity.Column = dr.GetString("column_name");
                        entity.ColumnId = dr.GetInt32("column_id");
                        entity.IsPrimaryKey = Convert.ToBoolean(dr.GetInt32("is_primary_key"));
                        entity.Type = dr.GetString("data_type");
                        entity.MaxLength = dr.GetInt16("max_length");
                        entity.Precision = dr.GetByte("precision");
                        uatData.Add(entity);
                    }
                }
                cnn.Close();
            }
            #endregion
            #region Production Tables And Columns
            using (var cnn = new SqlConnection(_configuration.GetConnectionString("ConnectionString_Prod")))
            {
                cnn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = @"
						SELECT tab.name as table_name, 
						    col.column_id,
							(SELECT COUNT(column_name)
							FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE
								TABLE_NAME = tab.name AND
								CONSTRAINT_NAME =
								(SELECT
									CONSTRAINT_NAME
								FROM 
									INFORMATION_SCHEMA.TABLE_CONSTRAINTS
								WHERE
									TABLE_NAME = tab.name AND                    
									CONSTRAINT_TYPE = 'PRIMARY KEY' AND
									COLUMN_NAME = col.name)) AS is_primary_key,
						    col.name as column_name, 
						    t.name as data_type,    
						    col.max_length,
						    col.precision
						FROM sys.tables as tab
						    INNER JOIN sys.columns as col
						        on tab.object_id = col.object_id
						    LEFT JOIN sys.types as t
						    on col.user_type_id = t.user_type_id 
						ORDER BY table_name, 
						    column_id;";
                    cmd.Connection = cnn;
                    var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var entity = new TableSQLViewDataModel();
                        entity.Table = dr.GetString("table_name");
                        entity.Column = dr.GetString("column_name");
                        entity.ColumnId = dr.GetInt32("column_id");
                        entity.IsPrimaryKey = Convert.ToBoolean(dr.GetInt32("is_primary_key"));
                        entity.Type = dr.GetString("data_type");
                        entity.MaxLength = dr.GetInt16("max_length");
                        entity.Precision = dr.GetByte("precision");
                        prodData.Add(entity);
                    }
                }
                cnn.Close();
            }
            #endregion
            var data = new List<TableDesignData>();

            // tables exists in uat but not in prod
            var newTables = uatData.Where(f => !prodData.Any(x => x.Table == f.Table));
            foreach (var newTable in newTables.GroupBy(f => f.Table))
            {
                var entity = new TableDesignData();
                entity.Table = newTable.Key;
                entity.TableChanged = true;
                entity.ChangeType = ChangeType.Added;
                entity.Changes.Add($"{newTable.Key} added to uat database but does not exists in production.");
                foreach (var column in newTable)
                {
                    entity.Columns.Add(new ColumnModel()
                    {
                        ColumnId = column.ColumnId,
                        IsPrimaryKey = column.IsPrimaryKey,
                        ChangeType = ChangeType.Added,
                        ColumnName = column.Column,
                        Type = column.Type,
                        NewType = column.Type,
                        MaxLength = column.MaxLength,
                        NewMaxLength = column.MaxLength,
                        Precision = column.Precision,
                        NewPrecision = column.Precision
                    });
                }
                data.Add(entity);
            }

            // tables exists in prod but not in uat
            var deletedOrOnlyProdTables = prodData.Where(f => !uatData.Any(x => x.Table == f.Table));
            foreach (var table in deletedOrOnlyProdTables.GroupBy(f => f.Table))
            {
                var entity = new TableDesignData();
                entity.Table = table.Key;
                entity.TableChanged = true;
                entity.ChangeType = ChangeType.Deleted;
                entity.Changes.Add($"{table.Key} exists in production database but does not exists in uat.");
                data.Add(entity);
            }

            // tables existed in both database
            var intersection = uatData.Where(f => prodData.Any(x => x.Table == f.Table));
            foreach (var table in intersection.GroupBy(f => f.Table))
            {
                var entity = new TableDesignData();
                entity.Table = table.Key;
                var prodTable = prodData.Where(f => f.Table == table.Key);
                foreach (var column in table)
                {
                    var entityColumn = new ColumnModel();
                    var prodColumn = prodTable.FirstOrDefault(f => f.Column == column.Column);
                    if (prodColumn != null)
                    {
                        var haveUpdate = false;
                        entityColumn.ColumnId = column.ColumnId;
                        entityColumn.IsPrimaryKey = column.IsPrimaryKey;
                        entityColumn.ColumnName = prodColumn.Column;

                        entityColumn.Type = prodColumn.Type;
                        entityColumn.MaxLength = prodColumn.MaxLength;
                        entityColumn.Precision = prodColumn.Precision;

                        entityColumn.NewType = prodColumn.Type;
                        entityColumn.NewMaxLength = prodColumn.MaxLength;
                        entityColumn.NewPrecision = prodColumn.Precision;

                        if (prodColumn.Type != column.Type)
                        {
                            haveUpdate = true;
                            entityColumn.NewType = column.Type;
                            entity.Changes.Add($"{entityColumn.ColumnName} column in {table.Key} table's Type attribute changed. {entityColumn.Type} => {entityColumn.NewType}");
                        }
                        if (prodColumn.MaxLength != column.MaxLength)
                        {
                            haveUpdate = true;
                            entityColumn.MaxLength = column.MaxLength;
                            entity.Changes.Add($"{entityColumn.ColumnName} column in {table.Key} table's MaxLength attribute changed. {entityColumn.MaxLength} => {entityColumn.NewMaxLength}");
                        }
                        if (prodColumn.Precision != column.Precision)
                        {
                            haveUpdate = true;
                            entityColumn.Precision = column.Precision;
                            entity.Changes.Add($"{entityColumn.ColumnName} column in {table.Key} table's Precision attribute changed. {entityColumn.Precision} => {entityColumn.NewPrecision}");
                        }
                        if (haveUpdate)
                        {
                            entity.TableChanged = true;
                            entity.ChangeType = ChangeType.Modified;
                            entityColumn.ColumnChanged = true;
                            entityColumn.ChangeType = ChangeType.Modified;
                        }
                    }
                    else
                    {
                        entityColumn.ColumnId = column.ColumnId;
                        entityColumn.IsPrimaryKey = column.IsPrimaryKey;
                        entityColumn.ColumnName = column.Column;

                        entityColumn.Type = column.Type;
                        entityColumn.MaxLength = column.MaxLength;
                        entityColumn.Precision = column.Precision;

                        entityColumn.NewType = column.Type;
                        entityColumn.NewMaxLength = column.MaxLength;
                        entityColumn.NewPrecision = column.Precision;

                        entity.TableChanged = true;
                        entity.ChangeType = ChangeType.Modified;
                        entityColumn.ColumnChanged = true;
                        entityColumn.ChangeType = ChangeType.Added;
                        entity.Changes.Add($"New {entityColumn.ColumnName} column added to {table.Key} table. Precision attribute changed. Type: {entityColumn.Type} <br/> MaxLength: {entityColumn.MaxLength} <br/> Precision: {entityColumn.Precision} <br/>");
                    }
                    entity.Columns.Add(entityColumn);
                }
                data.Add(entity);
            }
            return data;
        }

        /// <inheritdoc/>
        public ReturnData<List<DataDiffModel>> UatProductionDbDataDiffs(string table)
        {
            // I didn't do anything about sql injection attacks because this app will be using for personal or internal.
            // Don't question too much :D
            var data = new List<DataDiffModel>();
            var dataUat = new List<DbObject>();
            var dataProd = new List<DbObject>();
            var excludedTables = _configuration.GetValue<string>("ApplicationSettings:ExcludeDataFromThisTables").Split(',');
            string errMessage = "";
            if (excludedTables.Contains(table))
            {
                //Bu tablolarda veri getirme işlemleri yapılmayacak
                errMessage = "It's not permitted to access this table because this table added to 'ExcludeDataFromThisTables' list.";
                return new ReturnData<List<DataDiffModel>>()
                {
                    ErrorMessage = errMessage,
                    IsSucccesful = false,
                    Data = null
                };
            }
            else
            {
                var likeTables = excludedTables.Where(f => f.Contains("%"));
                foreach (var tab in likeTables)
                {
                    var name = tab.Replace("%", "");
                    if (tab.StartsWith("%") && !tab.EndsWith("%"))
                    {
                        if (table.StartsWith(name))
                        {
                            errMessage = "It's not permitted to access this table because this table added to 'ExcludeDataFromThisTables' list.";
                            return new ReturnData<List<DataDiffModel>>()
                            {
                                ErrorMessage = errMessage,
                                IsSucccesful = false,
                                Data = null
                            };
                        }
                    }
                    else if (!tab.StartsWith("%") && tab.EndsWith("%"))
                    {
                        if (table.EndsWith(name))
                        {
                            errMessage = "It's not permitted to access this table because this table added to 'ExcludeDataFromThisTables' list.";
                            return new ReturnData<List<DataDiffModel>>()
                            {
                                ErrorMessage = errMessage,
                                IsSucccesful = false,
                                Data = null
                            };
                        }
                    }
                    else
                    {
                        if (table.Contains(name))
                        {
                            errMessage = "It's not permitted to access this table because this table added to 'ExcludeDataFromThisTables' list.";
                            return new ReturnData<List<DataDiffModel>>()
                            {
                                ErrorMessage = errMessage,
                                IsSucccesful = false,
                                Data = null
                            };
                        }
                    }
                }
            }
            var tableDetails = GetAllTablesWithColumns().FirstOrDefault(f => f.Table == table);
            if (tableDetails == null)
            {
                errMessage = "There is no table with this name.";
                return new ReturnData<List<DataDiffModel>>()
                {
                    ErrorMessage = errMessage,
                    IsSucccesful = false,
                    Data = null
                };
            }
            var primaryKeyColumn = tableDetails.Columns.FirstOrDefault(f => f.IsPrimaryKey);
            string primaryKey = "";
            if (primaryKeyColumn != null)
            {
                primaryKey = primaryKeyColumn.ColumnName;
            }
            else
            {
                errMessage = "There is no primary key in this table so you cannot control this table";
                return new ReturnData<List<DataDiffModel>>()
                {
                    ErrorMessage = errMessage,
                    IsSucccesful = false,
                    Data = null
                };
            }
            if (tableDetails.ChangeType != ChangeType.Deleted)
            {
                using (var cnn = new SqlConnection(_configuration.GetConnectionString("ConnectionString_UAT")))
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"select * from " + table;
                        cmd.Connection = cnn;
                        var dr = cmd.ExecuteReader();
                        int i = 0;
                        while (dr.Read())
                        {
                            foreach (var column in tableDetails.Columns)
                            {
                                if (dataUat.Any(f => f.Column == column.ColumnName))
                                {
                                    var entityColumn = dataUat.FirstOrDefault(f => f.Column == column.ColumnName);
                                    entityColumn.Values.Add(new DbDataObject()
                                    {
                                        Id = dr[primaryKey].ToString(),
                                        Value = dr[column.ColumnName]
                                    });
                                }
                                else
                                {
                                    var entityColumn = new DbObject();
                                    entityColumn.Column = column.ColumnName;
                                    entityColumn.IsPrimaryKey = column.IsPrimaryKey;
                                    entityColumn.Type = column.Type; 
                                    entityColumn.Values.Add(new DbDataObject()
                                    {
                                        Id = dr[primaryKey].ToString(),
                                        Value = dr[column.ColumnName]
                                    });
                                    dataUat.Add(entityColumn);
                                }
                            }
                            i++;
                        }
                    }
                    cnn.Close();
                }
            }
            else
            {
                errMessage = "You cannot access to this table's datas because table deleted in uat database.";
                return new ReturnData<List<DataDiffModel>>()
                {
                    ErrorMessage = errMessage,
                    IsSucccesful = false,
                    Data = null
                };
            }
            if (tableDetails.ChangeType != ChangeType.Added && tableDetails.ChangeType != ChangeType.Deleted)
            {
                using (var cnn = new SqlConnection(_configuration.GetConnectionString("ConnectionString_Prod")))
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"select * from " + table;
                        cmd.Connection = cnn;
                        var dr = cmd.ExecuteReader();
                        int i = 0;
                        while (dr.Read())
                        {
                            foreach (var column in tableDetails.Columns.Where(f => f.ChangeType != ChangeType.Added).OrderBy(f => f.ColumnId))
                            {
                                if (dataProd.Any(f => f.Column == column.ColumnName))
                                {
                                    var entityColumn = dataProd.FirstOrDefault(f => f.Column == column.ColumnName);
                                    entityColumn.Values.Add(new DbDataObject()
                                    {
                                        Id = dr[primaryKey].ToString(),
                                        Value = dr[column.ColumnName]
                                    });
                                }
                                else
                                {
                                    var entityColumn = new DbObject();
                                    entityColumn.Column = column.ColumnName;
                                    entityColumn.IsPrimaryKey = column.IsPrimaryKey;
                                    entityColumn.Type = column.Type;
                                    entityColumn.Values.Add(new DbDataObject()
                                    {
                                        Id = dr[primaryKey].ToString(),
                                        Value = dr[column.ColumnName]
                                    });
                                    dataProd.Add(entityColumn);
                                }
                            }
                            i++;
                        }
                    }
                    cnn.Close();
                }
            }
            if (tableDetails.ChangeType == ChangeType.Added)
            {
                foreach (var uat in dataUat)
                {
                    foreach (var value in uat.Values)
                    {
                        data.Add(new DataDiffModel()
                        {
                            ColumnName = uat.Column,
                            Value = value.Value,
                            NewValue = value.Value,
                            ChangeType = ChangeType.Added,
                            ObjectId = value.Id,
                            ColumnType = uat.Type
                        });
                    }
                }
            }
            else if (tableDetails.ChangeType == ChangeType.Modified || tableDetails.ChangeType == ChangeType.UnModified)
            {
                foreach (var column in tableDetails.Columns)
                {
                    if (column.ChangeType == ChangeType.Added)
                    {
                        var uat = dataUat.FirstOrDefault(f => f.Column == column.ColumnName);
                        foreach (var value in uat.Values)
                        {
                            data.Add(new DataDiffModel()
                            {
                                ColumnName = uat.Column,
                                Value = value.Value,
                                NewValue = value.Value,
                                ChangeType = ChangeType.Added,
                                ObjectId = value.Id,
                                ColumnType = uat.Type
                            });
                        }
                    }
                    else if (column.ChangeType == ChangeType.Deleted)
                    {
                        var prod = dataProd.FirstOrDefault(f => f.Column == column.ColumnName);
                        foreach (var value in prod.Values)
                        {
                            data.Add(new DataDiffModel()
                            {
                                ColumnName = prod.Column,
                                Value = value.Value,
                                NewValue = value.Value,
                                ChangeType = ChangeType.Deleted,
                                ObjectId = value.Id,
                                ColumnType = prod.Type
                            });
                        }
                    }
                    else if (column.ChangeType == ChangeType.Modified || column.ChangeType == ChangeType.UnModified)
                    {
                        var uat = dataUat.FirstOrDefault(f => f.Column == column.ColumnName);
                        var prod = dataProd.FirstOrDefault(f => f.Column == column.ColumnName);
                        var uatOnlyValues = uat.Values.Where(f => !prod.Values.Any(x => x.Id == f.Id));
                        foreach (var value in uatOnlyValues)
                        {
                            data.Add(new DataDiffModel()
                            {
                                ColumnName = uat.Column,
                                Value = value.Value,
                                NewValue = value.Value,
                                ChangeType = ChangeType.Added,
                                ObjectId = value.Id,
                                ColumnType = uat.Type
                            });
                        }
                        var prodOnlyValues = prod.Values.Where(f => !uat.Values.Any(x => x.Id == f.Id));
                        foreach (var value in prodOnlyValues)
                        {
                            data.Add(new DataDiffModel()
                            {
                                ColumnName = uat.Column,
                                Value = value.Value,
                                NewValue = value.Value,
                                ChangeType = ChangeType.Deleted,
                                ObjectId = value.Id,
                                ColumnType = uat.Type
                            });
                        }
                        var intersection = uat.Values.Where(f => prod.Values.Any(x => x.Id == f.Id));
                        foreach (var value in intersection)
                        {
                            var prodValue = prod.Values.FirstOrDefault(f => f.Id == value.Id).Value;
                            if (prodValue.ToString() != value.Value.ToString())
                            {
                                data.Add(new DataDiffModel()
                                {
                                    ColumnName = uat.Column,
                                    Value = prodValue,
                                    NewValue = value.Value,
                                    ChangeType = ChangeType.Modified,
                                    ObjectId = value.Id,
                                    ColumnType = uat.Type
                                });
                            }
                            else
                            {
                                data.Add(new DataDiffModel()
                                {
                                    ColumnName = uat.Column,
                                    Value = value.Value,
                                    NewValue = value.Value,
                                    ChangeType = ChangeType.UnModified,
                                    ObjectId = value.Id,
                                    ColumnType = uat.Type
                                });
                            }
                        }
                    }
                }
            }
            else if (tableDetails.ChangeType == ChangeType.Deleted)
            {
                foreach (var prod in dataProd)
                {
                    foreach (var value in prod.Values)
                    {
                        data.Add(new DataDiffModel()
                        {
                            ColumnName = prod.Column,
                            Value = value.Value,
                            NewValue = value.Value,
                            ChangeType = ChangeType.Deleted,
                            ObjectId = value.Id,
                            ColumnType = prod.Type
                        });
                    }
                }
            }
            return new ReturnData<List<DataDiffModel>>()
            {
                AdditionalData = primaryKey,
                ErrorMessage = string.Empty,
                IsSucccesful = true,
                Data = data
            };
        }

        /// <inheritdoc/>
        public ReturnData<string> UpdateChanges(string table, List<ChangeModel> model)
        {
            var changeResult = PrepareAllChanges(table, model);
            if (!changeResult.IsSucccesful)
            {
                return new ReturnData<string>() { IsSucccesful = changeResult.IsSucccesful, ErrorMessage = changeResult.ErrorMessage };
            }
            return new ReturnData<string>() { IsSucccesful = true, ErrorMessage = "" };
        }

        /// <inheritdoc/>
        public ReturnData<string> CreateSqlFile(string table, List<ChangeModel> model)
        {
            var changeResult = PrepareAllChanges(table, model);
            if (!changeResult.IsSucccesful)
            {
                return new ReturnData<string>() { IsSucccesful = changeResult.IsSucccesful, ErrorMessage = changeResult.ErrorMessage };
            }
            var fileName = table + "_Update.sql";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sqlFiles", fileName);
            using (FileStream fs = File.Create(filePath))
            {
                byte[] byteData = Encoding.UTF8.GetBytes(string.Join("\n", changeResult.Data));
                fs.Write(byteData, 0, byteData.Length);
            }
            return new ReturnData<string>() { Data = filePath, ErrorMessage = "", IsSucccesful = true };
        }

        #region Helpers
        /// <summary>
        /// Prepare commands based user's selected changes
        /// </summary>
        /// <param name="table"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private ReturnData<List<string>> PrepareAllChanges(string table, List<ChangeModel> model)
        {
            var data = new List<string>();
            var dbDiffsResult = UatProductionDbDataDiffs(table);
            var tableAllColumns = GetAllTablesWithColumns().FirstOrDefault(f => f.Table == table);
            var insertTableTemplate = "insert into {0} ({1}) values ({2});"; // 0: table name, 1: columns, 2: values
            var updateRowTemplate = "update {0} set {1} = {2} where {3} = {4};"; // 0: table name, 1: column name, 2: new value, 3: primary key column name, 4: row id
            if (!dbDiffsResult.IsSucccesful || tableAllColumns == null)
            {
                return new ReturnData<List<string>>() { IsSucccesful = dbDiffsResult.IsSucccesful, ErrorMessage = dbDiffsResult.ErrorMessage, Data = data };
            }
            foreach (var selectedChange in model)
            {
                var diff = dbDiffsResult.Data.FirstOrDefault(f => f.ObjectId == selectedChange.ObjectId);
                var values = dbDiffsResult.Data.Where(f => f.ObjectId == selectedChange.ObjectId);
                var sqlCommand = "";
                if (selectedChange.Type == "row")
                {
                    sqlCommand = string.Format(
                    insertTableTemplate,
                    table,
                    string.Join(",", tableAllColumns.Columns.OrderBy(f => f.ColumnName).Select(f => f.ColumnName)),
                    string.Join(",", values.OrderBy(f => f.ColumnName).Select(f => FormatValue(f.NewValue, f.ColumnType))));
                }
                else
                {
                    var primaryKeyColumnName = tableAllColumns.Columns.FirstOrDefault(f => f.IsPrimaryKey)!.ColumnName;
                    var columnValue = values.FirstOrDefault(f => f.ColumnName == selectedChange.ColumnName)!;
                    sqlCommand = string.Format(
                    updateRowTemplate,
                    table,
                    selectedChange.ColumnName,
                    FormatValue(columnValue.NewValue, columnValue.ColumnType),
                    primaryKeyColumnName,
                    $"'{selectedChange.ObjectId}'");
                }
                data.Add(sqlCommand);
            }
            return new ReturnData<List<string>>() { IsSucccesful = true, ErrorMessage = string.Empty, Data = data };
        }

        /// <summary>
        /// Format value based column type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        private string FormatValue(object value, string columnType = "")
        {
            var data = "";
            var valueStr = value.ToString();
            if (valueStr != null && value != DBNull.Value)
            {
                if (columnType == "uniqueidentifier")
                {
                    data = "'" + valueStr + "'";
                }
                else if (columnType == "nvarchar" || columnType == "varchar")
                {
                    data = "N'" + valueStr.Replace("'", "''") + "'";
                }
                else if (columnType == "datetime2" || columnType == "datetime")
                {
                    DateTime date;
                    if (DateTime.TryParse(valueStr, out date))
                    {
                        data = "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    }
                    else
                    {
                        data = "NULL";
                    }
                }
                else if (columnType == "bit")
                {
                    if (valueStr == "True")
                    {
                        data = "1";
                    }
                    else
                    {
                        data = "0";
                    }
                }
                else if (columnType == "int")
                {
                    data = "'" + valueStr + "'";
                }
                else
                {
                    data = "'" + valueStr.Replace("'", "''") + "'";
                }
            }
            else
            {
                data = "NULL";
            }
            return data;
        }
        #endregion
        #endregion
    }
}
