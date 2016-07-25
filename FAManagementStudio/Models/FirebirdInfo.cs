﻿using FAManagementStudio.Models.Firebird;
using FirebirdSql.Data.FirebirdClient;

namespace FAManagementStudio.Models
{
    public class FirebirdInfo
    {
        public FirebirdInfo(string path)
        {
            SetConnection(path);
        }

        public FirebirdInfo(string path, int odsVer)
        {
            _builder.Database = path;
            _builder.ClientLibrary = odsVer == _fb3Ods ? @"\fb3\fbclient" : @"fb25\fbembed";
        }

        public string Path { get { return _builder.Database; } }

        private const int _fb2Ods = 11;
        private const int _fb3Ods = 12;
        private int _odsVersion = -1;

        private void SetConnection(string path)
        {
            _odsVersion = new FbUtility().GetOdsVersion(path);
            _builder.Database = path;
            _builder.ClientLibrary = _odsVersion == _fb3Ods ? @"\fb3\fbclient" : @"fb25\fbembed";
        }

        public bool IsTargetOdsVersion()
        {
            try
            {
                return _odsVersion == _fb2Ods || _odsVersion == _fb3Ods;
            }
            catch
            {
                return false;
            }
        }

        private FbConnectionStringBuilder _builder = new FbConnectionStringBuilder()
        {
            DataSource = "localhost",
            Charset = FbCharset.Utf8.ToString(),
            UserID = "SYSDBA",
            Password = "masterkey",
            ServerType = FbServerType.Embedded,
            Pooling = false
        };

        public FbConnectionStringBuilder Builder { get { return _builder; } }
    }
}
