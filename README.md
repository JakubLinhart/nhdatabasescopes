## Overview

Examples in this project were created with two goals in mind: isolation and speed.
There is one issue with keeping execution time low enough when doing integration testing with NH. Building configuration, session factory and building the database is very time consuming operation, so it is not effective to build them by each executed test – they should be shared between tests as much as possible.

Individual tests should not interfere each other by modifying database content and resulting in annoying bugs. But the sharing is in conflict with test isolation. There are many ways how to resolve the conflict and four of them are included in this example collection:

## SqliteInMemorySharedScope

Single database is created in-memory using Sqlite and it is shared between al tests then. Before execution of each test the database is dropped and recreated again. This method is extremely fast but is unusable when the tests are executed in parallel. I don’t know how increasing domain complexity affects performance of this example. The idea comes from NHibernate Testing with SQLite in-memory DB [blog](http://jasondentler.com/blog/2009/09/nhibernate-testing-with-sqlite-in-memory-db/).

## SqliteInFilePrivateScope

A master database is created in a file. A private copy of this master file is created before each test and each test uses only database stored in this copied file. So tests are completely isolated and there are no issues when the tests are executed in parallel. The price is that performance is an order of magnitude lower than in case of SqliteInMemorySharedScope but still good enough.

## SqliteInMemoryPrivateScope

SqliteInMemoryPrivateScope combines the advantages of both previous examples. It is even a little bit faster than SqliteInMemorySharedScope and it is usable by tests that are executed in parallel. The master database is created in the memory at first. Each test uses only its own in-memory copy of the master database. Database duplication is achieved via [Sqlite Backup API](http://www.sqlite.org/backup.html) which has unfortunately no counterpart in System.Data.SQLite wrapper yet.

## MsSqlCeInFilePrivateScope

The same approach and comparable performance as SqliteInFilePrivateScope but using MS SQL Server Compact Edition as the database engine.

Configuration and session factory is shared between tests in all examples. The database is built from domain model and NH mappings using SchemaExport class.

All four approaches allocate some resources (memory, file) that should be freed at the end of each test. IDisposable pattern is suitable for such task.

```CSharp
// a new private copy of master database is created
using (var scope = new SqliteInMemoryPrivateScope()) 
{
    using (var session1 = scope.OpenSession())
    {
        // do something useful
    }
    using (var session2 = scope.OpenSession())
    {
        // do something useful in the same database as in the first session
    }
} // the database is removed from memory
```

Ayende Rahien describes how to do the same thing using inheritance on [his blog](https://ayende.com/blog/1772/unit-testing-with-nhibernate-active-record), but I prefer compositional way in this case because it is clearly more flexible.

## What to do better?

- Shared resources cleanup (like master database file, etc...).
- Determine how domain complexity affects database scopes performance (some reasonable domain model or more domain models is needed).
