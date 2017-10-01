using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.UnitTests
{
    [TestClass]
    public class SqliteInFilePrivateScopeTest : DatabaseScopeAbstractTest
    {
        protected override IDatabaseScope CreateDatabaseScope()
        {
            return new SqliteInFilePrivateScope();
        }
    }
}
