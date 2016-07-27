// <copyright file="FbUtilityTest.cs">Copyright ©  2016</copyright>
using System;
using FAManagementStudio.Models.Firebird;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FAManagementStudio.Models.Firebird.Tests
{
    /// <summary>このクラスには、FbUtility に対するパラメーター化された単体テストが含まれています</summary>
    [PexClass(typeof(FbUtility))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class FbUtilityTest
    {
        /// <summary>GetCharsetString(FbCharset) のテスト スタブ </summary>
        [PexMethod]
        public string GetCharsetStringTest([PexAssumeUnderTest]FbUtility target, FbCharset charset)
        {
            string result = target.GetCharsetString(charset);
            return result;
            // TODO: アサーションを メソッド FbUtilityTest.GetCharsetStringTest(FbUtility, FbCharset) に追加します
        }
    }
}
