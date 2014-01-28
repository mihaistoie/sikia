﻿using Sikia.Db.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sikia.Db.SqlServer.Model
{
    public class MsSqlTable : DbTable
    {
        #region Schema
        public override void CreateColumnsSQL(List<string> structure)
        {
            var sql = new StringBuilder();
            sql.AppendLine(string.Format("CREATE TABLE {0}(", TableName));
            var count = Columns.Count;
            for (var index = 0; index < count; ++index)
            {
                var c = columns[index];
                if (index > 0)
                {
                    sql.Append(",").Append(Environment.NewLine);

                }
                c.CreateSQL(sql);
            }
            sql.AppendLine();
            sql.AppendLine(")");
            structure.Add(sql.ToString());
        }
        public override void CreateIndexesSQL(List<string> structure)
        {
            var sql = new StringBuilder();
            for (var index = 0; index < indexes.Count; ++index)
            {
                var ii = indexes[index];
                sql.Clear();
                sql.Append(string.Format("CREATE {0}INDEX {1} ON {2}(", (ii.Unique ? "UNIQUE " : string.Empty), ii.IndexName, TableName));
                for (var i = 0; i < ii.Columns.Count; ++i)
                {
                    var ic = ii.Columns[i];
                    if (i > 0) sql.Append(", ");
                    sql.Append(string.Format("{0}{1}", ic.ColumnName, (ic.Descending ? " DESC" : string.Empty)));
                }
                sql.Append(")");
                structure.Add(sql.ToString());
            }
        }
        public override void CreateFKSQL(List<string> structure)
        {
            var rfields = new StringBuilder();
            var ufields = new StringBuilder();
            for (var index = 0; index < foreignKeys.Count; ++index)
            {
                var ii = foreignKeys[index];
                rfields.Clear();
                ufields.Clear();

                for (var i = 0; i < ii.Columns.Count; ++i)
                {
                    var ic = ii.Columns[i];
                    if (i > 0)
                    {
                        rfields.Append(", ");
                        ufields.Append(", ");
                    }
                    rfields.Append(ic.ColumnName);
                    ufields.Append(ic.UniqueColumnName);

                }
                structure.Add(string.Format("ALTER TABLE {0} ADD FOREIGN KEY ({2}) REFERENCES {1}({3})", ii.TableName, ii.UniqueTableName, rfields.ToString(), ufields.ToString()));
            }
        }

        public override void CreatePKSQL(List<string> structure)
        {
            if (pk.Count > 0)
            {
                var sql = new StringBuilder();
                sql.Append(string.Format("ALTER TABLE {0} ADD PRIMARY KEY (", TableName));
                for (var i = 0; i < pk.Count; ++i)
                {
                    if (i > 0) sql.Append(", ");
                    sql.Append(pk[i]);
                }
                sql.Append(")");
                structure.Add(sql.ToString());

            }
        }
        #endregion
    }
}
