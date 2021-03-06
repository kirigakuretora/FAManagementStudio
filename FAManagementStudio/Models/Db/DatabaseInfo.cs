﻿using FAManagementStudio.Common;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;

namespace FAManagementStudio.Models
{
    public class DatabaseInfo : BindableBase
    {
        public DatabaseInfo(FirebirdInfo inf)
        {
            _fbInfo = inf;
        }
        private FirebirdInfo _fbInfo;
        public string Path { get { return _fbInfo.Path; } }
        public string UserId { get; set; }
        public string Password { get; set; }
        internal string ConnectionString { get { return _fbInfo.Builder.ConnectionString; } }
        public bool CanLoadDatabase { get { return _fbInfo.IsTargetOdsVersion(); } }
        public FbConnectionStringBuilder Builder { get { return _fbInfo.Builder; } }

        public void CreateDatabase()
        {
            FbConnection.CreateDatabase(_fbInfo.Builder.ConnectionString, false);
        }

        public string GetDefaultCharSet(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select RDB$CHARACTER_SET_NAME CharSet from RDB$DATABASE";
                return command.ExecuteScalar() as string ?? "UTF8";
            }
        }

        public IEnumerable<TableInfo> GetTables(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select rdb$relation_name AS Name from rdb$relations where rdb$relation_type = 0 and rdb$system_flag = 0 order by rdb$relation_name asc";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new TableInfo(((string)reader["Name"]).TrimEnd());
                }
            }
        }
        public IEnumerable<ViewInfo> GetViews(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = @"select rdb$relation_name AS Name, rdb$view_source Source from rdb$relations where rdb$relation_type = 1 and rdb$system_flag = 0 order by rdb$relation_name asc";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return new ViewInfo(((string)reader["Name"]).TrimEnd(), ((string)reader["Source"]).Trim(' ', '\r', '\n'));
                }
            }
        }
        public IEnumerable<DomainInfo> GetDomain(FbConnection con)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText =
                     $"select distinct f.rdb$field_type Type, f.rdb$field_sub_type SubType , f.rdb$character_length CharSize, f.rdb$field_name FieldName, f.rdb$field_precision FieldPrecision, f.rdb$field_scale FieldScale " +
                      "from rdb$fields f " +
                     $"where f.rdb$FIELD_NAME not starting with 'RDB$' and f.rdb$FIELD_NAME not starting with 'MON$' and f.rdb$FIELD_NAME not starting with 'SEC$' " +
                      "order by f.rdb$field_name; ";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var name = ((string)reader["FieldName"]).TrimEnd();
                    var size = (reader["CharSize"] == DBNull.Value) ? null : (short?)reader["CharSize"];
                    var subType = (reader["SubType"] == DBNull.Value) ? null : (short?)reader["SubType"];
                    var precision = (reader["FieldPrecision"] == DBNull.Value) ? null : (short?)reader["FieldPrecision"];
                    var scale = (reader["FieldScale"] == DBNull.Value) ? null : (short?)reader["FieldScale"];
                    var type = new FieldType((short)reader["Type"], subType, size, precision, scale);

                    yield return new DomainInfo(name, type);
                }
            }
        }
    }
}
