using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.UnitTests
{
    [TestClass]
    public class MsSqlCeInFilePrivateScopeTest : DatabaseScopeAbstractTest
    {
        protected override IDatabaseScope CreateDatabaseScope()
        {
            return new MsSqlCeInFilePrivateScope();
        }
    }
}
