using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.UnitTests
{
    [TestClass]
    public class SqliteInMemorySharedScopeTest : DatabaseScopeAbstractTest
    {
        protected override IDatabaseScope CreateDatabaseScope()
        {
            return new SqliteInMemorySharedScope();
        }
    }
}
