﻿using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
    public class ViewInfo
    {
        public ViewInfo(string name, string source)
        {
            this.ViewName = name;
            this.Source = source;
        }
        public string ViewName { get; set; }

        public string Source { get; set; }
        public IEnumerable<ColumInfo> GetColums(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                    $"select trim(rf.rdb$field_name) Name, f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, rf.rdb$field_source FieldSource, rf.rdb$null_flag NullFlag, f.rdb$field_precision FieldPrecision, f.rdb$field_scale FieldScale " +
                        "from rdb$relation_fields rf " +
                        "join rdb$relations r on rf.rdb$relation_name = r.rdb$relation_name " +
                                            "and r.rdb$view_blr is not null " +
                                            "and r.rdb$relation_type = 1 and r.rdb$system_flag = 0 " +
                        "join rdb$fields f on f.rdb$field_name = rf.rdb$field_source " +
                     $"where rf.rdb$relation_name = '{this.ViewName}' " +
                      "order by rf.rdb$field_position; ";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
                    var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
                    var nullFlag = reader["NullFlag"] == DBNull.Value;
                    var precision = (reader["FieldPrecision"] == DBNull.Value) ? null : (short?)reader["FieldPrecision"];
                    var scale = (reader["FieldScale"] == DBNull.Value) ? null : (short?)reader["FieldScale"];
                    var type = new FieldType((short)reader["Type"], subType, size, precision, scale);

                    yield return new ColumInfo(((string)reader["Name"]).TrimEnd(), type, null, ((string)reader["FieldSource"]).TrimEnd(), nullFlag, true, "");
                }
            }
        }
    }
}
