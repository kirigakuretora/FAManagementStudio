﻿using FAManagementStudio.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels
{
    public class TableViewModel : ViewModelBase, ITableViewModel
    {
        private string _name;
        public TableViewModel(string name)
        {
            _name = name;
        }
        public string TableName { get { return _name; } }
        public List<ColumViewMoodel> Colums { get; } = new List<ColumViewMoodel>();
        public TableKind Kind { get; } = TableKind.Table;
        public List<TriggerViewModel> Triggers { get; } = new List<TriggerViewModel>();
        public List<IndexViewModel> Indexs { get; } = new List<IndexViewModel>();

        public string GetDdl(DbViewModel dbVm)
        {
            var colums = Colums.Select(x =>
            {
                var sql = $"{x.ColumName} {x.ColumType}";
                if (!x.NullFlag)
                {
                    sql += " not null";
                }
                return sql.ToUpper();
            });

            var index = Indexs
                    .Select(x =>
                    {
                        var sql = x.IndexName.StartsWith("rdb", System.StringComparison.OrdinalIgnoreCase) ? "" : $"CONSTRAINT {x.IndexName} ";
                        switch (x.Kind)
                        {
                            case ConstraintsKind.Primary:
                                sql += $"PRIMARY KEY ({string.Join(", ", x.FieldNames.ToArray())})";
                                break;
                            case ConstraintsKind.Foreign:
                                var idx = dbVm.Indexes.Where(dbIdx => dbIdx.IndexName == x.IndexName).First();
                                sql += $"FOREIGN KEY({string.Join(", ", x.FieldNames.ToArray())}) REFERENCES {idx.TableName} ({string.Join(", ", idx.FieldNames.ToArray())})";
                                break;
                            case ConstraintsKind.Unique:
                                sql += $"UNIQUE ({string.Join(", ", x.FieldNames.ToArray())})";
                                break;
                            default:
                                return "";
                        }
                        return sql;
                    });

            var domain = Colums.Where(x => x.IsDomainType)
                                .Select(x => new { x.ColumType, x.ColumDataType })
                                .Distinct()
                                .Select(x => $"CREATE DOMAIN {x.ColumType} AS {x.ColumDataType};\r\n");
            var domainStr = string.Join("", domain.ToArray());
            return domainStr + $"CREATE TABLE {TableName} ({Environment.NewLine}  { string.Join($",{Environment.NewLine}  ", colums.Union(index).ToArray()) + Environment.NewLine})";
        }
    }
}
