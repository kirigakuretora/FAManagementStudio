﻿using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;

namespace FAManagementStudio.Models.Tests
{
    [TestClass()]
    public class QueryInfoTests
    {
        [TestMethod()]
        public void GetWordTest()
        {
            var inf = new QueryInfo();
            var input = @"select * from test t where t.Hoge='' and t.Fuga = '' ";
            //idx = 0, length = 0
            (inf.AsDynamic().GetWord(input, 0, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            var input2 = @"select * from test t where t.Hoge='' and t.Fuga = ''";
            (inf.AsDynamic().GetWord(input2, 0, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            var input3 = @"select * from test t where t.Hoge='' and t.Fuga = '';";
            (inf.AsDynamic().GetWord(input3, 0, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''", ";" });

            //idx = any, length = 0
            (inf.AsDynamic().GetWord(input, 1, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "elect", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            (inf.AsDynamic().GetWord(input, 7, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            (inf.AsDynamic().GetWord(input, input.Length, 0) as IEnumerable<string>).ToArray().IsStructuralEqual(new string[] { });

            //idx = any, length = any
            (inf.AsDynamic().GetWord(input, 1, 2) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "e" });
            (inf.AsDynamic().GetWord(input, 7, 28) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "*", "from", "test", "t", "where", "t" });
            (inf.AsDynamic().GetWord(input, 0, input.Length) as IEnumerable<string>).ToArray().IsStructuralEqual(new[] { "select", "*", "from", "test", "t", "where", "t.Hoge=''", "and", "t.Fuga", "=", "''" });
            (inf.AsDynamic().GetWord(input, input.Length, input.Length) as IEnumerable<string>).ToArray().IsStructuralEqual(new string[] { });
        }

        [TestMethod()]
        public void GetStatementTest()
        {
            var inf = new QueryInfo();
            var input1 = "select * from A";
            (inf.AsDynamic().GetStatement(input1) as string[]).IsStructuralEqual(new[] { "select * from A" });

            var input2 = "select * from A;";
            (inf.AsDynamic().GetStatement(input2) as string[]).IsStructuralEqual(new[] { "select * from A" });

            var input3 = "select * from A; select * from B";
            (inf.AsDynamic().GetStatement(input3) as string[]).IsStructuralEqual(new[] { "select * from A", "select * from B" });

            var input4 = @"" + Environment.NewLine +
                      @" create trigger set_foo_primary for foo" + Environment.NewLine +
                      @"before insert" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"as begin" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"new.a = gen_id(gen_foo, 1);" + Environment.NewLine +
                      @"end" + Environment.NewLine +
                      @";" + Environment.NewLine +
                      @"  select* from A;" + Environment.NewLine +
                      @" create table V(a integer, b nvarchar(5))";
            (inf.AsDynamic().GetStatement(input4) as string[]).IsStructuralEqual(new[] {
                  @"create trigger set_foo_primary for foo" + Environment.NewLine +
                      @"before insert" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"as begin" + Environment.NewLine +
                      @"" + Environment.NewLine +
                      @"new.a = gen_id(gen_foo, 1);" + Environment.NewLine +
                      @"end" ,
                "select* from A", "create table V(a integer, b nvarchar(5))" });
        }

        [TestMethod()]
        public void GetStatementTest2()
        {
            var inf = new QueryInfo();
            var input1 =
@"--comment1
create trigger set_foo_primary for foo
before insert
as begin
new.a = gen_id(gen_foo, 1);
end;

--comment2  
select *
from test
where a = 1;

--comment3
create trigger set_foo_primary for foo2
before insert
as begin
new.a = gen_id(gen_foo, 1);
end;

select * from fuga where hoho = 'eeee'
--comment4";

            (inf.AsDynamic().GetStatement(input1) as string[]).IsStructuralEqual(new[] {
@"create trigger set_foo_primary for foo
before insert
as begin
new.a = gen_id(gen_foo, 1);
end",
@"select *
from test
where a = 1",
@"create trigger set_foo_primary for foo2
before insert
as begin
new.a = gen_id(gen_foo, 1);
end",
@"select * from fuga where hoho = 'eeee'"
});
        }

        [TestMethod()]
        public void GetStatementTest3()
        {
            var inf = new QueryInfo();
            var input1 =
@"--comment1
select * from fuga where hoho = 'eeee' --comment2
--comment3";

            (inf.AsDynamic().GetStatement(input1) as string[]).IsStructuralEqual(new[] {
@"select * from fuga where hoho = 'eeee'"});
        }
        [TestMethod()]
        public void GetStatementTest4()
        {
            var inf = new QueryInfo();
            var input =
@"--ExecuteSample1
execute block
as
declare i int = 0;
begin
  while (i < 128) do
  begin
    insert into AsciiTable values (:i, ascii_char(:i));
    i = i + 1;
  end end;

--ExecuteSample2
execute block (x double precision = ?, y double precision = ?)
returns (gmean double precision)
as
begin
  gmean = sqrt(x*y);
  suspend;
end;";
            (inf.AsDynamic().GetStatement(input) as string[]).IsStructuralEqual(new[] {
@"execute block
as
declare i int = 0;
begin
  while (i < 128) do
  begin
    insert into AsciiTable values (:i, ascii_char(:i));
    i = i + 1;
  end end",
@"execute block (x double precision = ?, y double precision = ?)
returns (gmean double precision)
as
begin
  gmean = sqrt(x*y);
  suspend;
end"
});
        }
    }
}