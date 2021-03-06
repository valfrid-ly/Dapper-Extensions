using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;

namespace DapperExtensions.Test.Sql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class SqlDialectBaseFixture
    {
        public abstract class SqlDialectBaseFixtureBase
        {
            protected TestDialect Dialect;

            [SetUp]
            public void Setup()
            {
                Dialect = new TestDialect();
            }
        }

        [TestFixture]
        public class AbstractMethods : SqlDialectBaseFixtureBase
        {
            [Test]
            public void DatabaseFunctionTests()
            {
                Action action = () => Dialect.GetDatabaseFunctionString(DatabaseFunction.None, "foo");

                action.Should().Throw<NotImplementedException>();
            }

            [Test]
            public void GetIdentitySqlTests()
            {
                Action action = () => Dialect.GetIdentitySql("foo");

                action.Should().Throw<NotImplementedException>();
            }

            [Test]
            public void GetPagingSqlTests()
            {
                Action action = () => Dialect.GetPagingSql("foo", 0, 0, null, null);

                action.Should().Throw<NotImplementedException>();
            }

            [Test]
            public void GetSetSqlTests()
            {
                Action action = () => Dialect.GetSetSql("foo", 0, 0, null);

                action.Should().Throw<NotImplementedException>();
            }

            [Test]
            public void EnableCaseInsensitiveTests()
            {
                Action action = () => Dialect.EnableCaseInsensitive(null);

                action.Should().Throw<NotImplementedException>();
            }
        }

        [TestFixture]
        public class Properties : SqlDialectBaseFixtureBase
        {
            [Test]
            public void CheckSettings()
            {
                Assert.AreEqual('"', Dialect.OpenQuote);
                Assert.AreEqual('"', Dialect.CloseQuote);
                Assert.AreEqual(";" + Environment.NewLine, Dialect.BatchSeperator);
                Assert.AreEqual('@', Dialect.ParameterPrefix);
                Assert.AreEqual("1=1", Dialect.EmptyExpression);
                Assert.IsTrue(Dialect.SupportsMultipleStatements);
                Assert.IsTrue(Dialect.SupportsCountOfSubquery);
            }
        }

        [TestFixture]
        public class IsQuotedMethod : SqlDialectBaseFixtureBase
        {
            [Test]
            public void WithQuotes_ReturnsTrue()
            {
                Assert.IsTrue(Dialect.IsQuoted("\"foo\""));
            }

            [Test]
            public void WithOnlyStartQuotes_ReturnsFalse()
            {
                Assert.IsFalse(Dialect.IsQuoted("\"foo"));
            }

            [Test]
            public void WithOnlyCloseQuotes_ReturnsFalse()
            {
                Assert.IsFalse(Dialect.IsQuoted("foo\""));
            }
        }

        [TestFixture]
        public class QuoteStringMethod : SqlDialectBaseFixtureBase
        {
            [Test]
            public void WithNoQuotes_AddsQuotes()
            {
                Assert.AreEqual("\"foo\"", Dialect.QuoteString("foo"));
            }

            [Test]
            public void WithStartQuote_AddsQuotes()
            {
                Assert.AreEqual("\"\"foo\"", Dialect.QuoteString("\"foo"));
            }

            [Test]
            public void WithCloseQuote_AddsQuotes()
            {
                Assert.AreEqual("\"foo\"\"", Dialect.QuoteString("foo\""));
            }

            [Test]
            public void WithBothQuote_DoesNotAddQuotes()
            {
                Assert.AreEqual("\"foo\"", Dialect.QuoteString("\"foo\""));
            }
        }

        [TestFixture]
        public class UnQuoteStringMethod : SqlDialectBaseFixtureBase
        {
            [Test]
            public void WithNoQuotes_AddsQuotes()
            {
                Assert.AreEqual("foo", Dialect.UnQuoteString("foo"));
            }

            [Test]
            public void WithStartQuote_AddsQuotes()
            {
                Assert.AreEqual("\"foo", Dialect.UnQuoteString("\"foo"));
            }

            [Test]
            public void WithCloseQuote_AddsQuotes()
            {
                Assert.AreEqual("foo\"", Dialect.UnQuoteString("foo\""));
            }

            [Test]
            public void WithBothQuote_DoesNotAddQuotes()
            {
                Assert.AreEqual("foo", Dialect.UnQuoteString("\"foo\""));
            }
        }

        [TestFixture]
        public class GetTableNameMethod : SqlDialectBaseFixtureBase
        {
            [Test]
            public void NullTableName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetTableName(null, null, null));
                StringAssert.AreEqualIgnoringCase("TableName", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void EmptyTableName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetTableName(null, string.Empty, null));
                StringAssert.AreEqualIgnoringCase("TableName", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void TableNameOnly_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName(null, "foo", null);
                Assert.AreEqual("\"foo\"", result);
            }

            [Test]
            public void SchemaAndTable_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName("bar", "foo", null);
                Assert.AreEqual("\"bar\".\"foo\"", result);
            }

            [Test]
            public void AllParams_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetTableName("bar", "foo", "al");
                Assert.AreEqual("\"bar\".\"foo\" \"al\"", result);
            }

            [Test]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                var result = Dialect.GetTableName("\"bar\"", "\"foo\"", "\"al\"");
                Assert.AreEqual("\"bar\".\"foo\" \"al\"", result);
            }
        }

        [TestFixture]
        public class GetColumnNameMethod : SqlDialectBaseFixtureBase
        {
            [Test]
            public void NullColumnName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetColumnName(null, null, null));
                Assert.AreEqual("columnName", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void EmptyColumnName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetColumnName(null, string.Empty, null));
                Assert.AreEqual("columnName", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void WhitespaceColumnName_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetColumnName(null, "  ", null));
                Assert.AreEqual("columnName", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void ColumnNameOnly_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName(null, "foo", null);
                Assert.AreEqual("\"foo\"", result);
            }

            [Test]
            public void PrefixColumnName_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName("bar", "foo", null);
                Assert.AreEqual("\"bar\".\"foo\"", result);
            }

            [Test]
            public void AllParams_ReturnsProperlyQuoted()
            {
                var result = Dialect.GetColumnName("bar", "foo", "al");
                Assert.AreEqual("\"bar\".\"foo\" AS \"al\"", result);
            }

            [Test]
            public void ContainsQuotes_DoesNotAddExtraQuotes()
            {
                var result = Dialect.GetColumnName("\"bar\"", "\"foo\"", "\"al\"");
                Assert.AreEqual("\"bar\".\"foo\" AS \"al\"", result);
            }
        }

        [TestFixture]
        public class GetCountSqlMethod : SqlDialectBaseFixtureBase
        {
            [Test]
            public void NullSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetCountSql(null));
                Assert.AreEqual("sql", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void WhitespaceSql_ThrowsException()
            {
                var ex = Assert.Throws<ArgumentNullException>(() => Dialect.GetCountSql("  "));
                Assert.AreEqual("sql", ex.ParamName);
                StringAssert.Contains("cannot be null", ex.Message);
            }

            [Test]
            public void SelectSql_ReturnsProperSql()
            {
                const string sql = "TABLE";
                var result = Dialect.GetCountSql(sql);
                Assert.AreEqual($"SELECT COUNT(*) AS {Dialect.OpenQuote}Total{Dialect.CloseQuote} FROM {sql}", result);
            }
        }

        public class TestDialect : SqlDialectBase
        {
            public override string GetDatabaseFunctionString(DatabaseFunction databaseFunction, string columnName, string functionParameters = "")
            {
                throw new NotImplementedException();
            }

            public override string GetIdentitySql(string tableName)
            {
                throw new NotImplementedException();
            }

            public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters, string partitionBy)
            {
                throw new NotImplementedException();
            }

            public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
            {
                throw new NotImplementedException();
            }

            public override void EnableCaseInsensitive(IDbConnection connection)
            {
                throw new NotImplementedException();
            }
        }
    }
}