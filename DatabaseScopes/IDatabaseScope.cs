using System;
using NHibernate;

namespace NHibernateDatabaseScope.DatabaseScopes
{
    public interface IDatabaseScope : IDisposable
    {
        ISession OpenSession();
    }
}
