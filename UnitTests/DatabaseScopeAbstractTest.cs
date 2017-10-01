using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernateDatabaseScope.SampleDomainModel;
using NHibernateDatabaseScope.DatabaseScopes;
using System;
using System.Linq;

namespace NHibernateDatabaseScope.UnitTests
{
    [TestClass]
    public abstract class DatabaseScopeAbstractTest
    {
        protected abstract IDatabaseScope CreateDatabaseScope();

        [TestInitialize]
        public void Initialize()
        {
            using (var scope = CreateDatabaseScope())
            {
                using (var session = scope.OpenSession())
                {
                    Assert.AreEqual(0, session.CreateQuery("from Order").List().Count);
                    Assert.AreEqual(0, session.CreateQuery("from OrderRow").List().Count);
                }
            }
        }

        [TestMethod]
        public void SaveOrderRowTest()
        {
            using (var scope = CreateDatabaseScope())
            {
                using (var session1 = scope.OpenSession())
                {
                    var row = new OrderRow()
                    {
                        Product = "Product1",
                        Price = 10,
                    };

                    session1.Save(row);
                    session1.Flush();
                }

                using (var session2 = scope.OpenSession())
                {
                    var query = session2.CreateQuery("from OrderRow where Product='Product1'");
                    var row = query.UniqueResult<OrderRow>();

                    Assert.IsNotNull(row);
                    Assert.AreEqual("Product1", row.Product);
                    Assert.AreEqual(10, row.Price);
                }
            }
        }

        [TestMethod]
        public void SaveOrderTest()
        {
            using (var scope = CreateDatabaseScope())
            {
                using (var session1 = scope.OpenSession())
                {
                    var order = new Order()
                    {
                        Rows = new Iesi.Collections.Generic.HashedSet<OrderRow> {
                            new OrderRow()
                            {
                                Price = 10,
                                Product = "Product1"
                            }
                        },

                        ShipName = "ShipName1",
                        RequiredDate = DateTime.Now,
                    };

                    session1.Save(order);
                    session1.Flush();
                }

                using (var session2 = scope.OpenSession())
                {
                    var query = session2.CreateQuery("from Order where ShipName='ShipName1'");
                    var order = query.UniqueResult<Order>();

                    Assert.IsNotNull(order);
                    Assert.AreEqual("ShipName1", order.ShipName);
                    Assert.AreEqual(1, order.Rows.Count);
                    Assert.AreEqual("Product1", order.Rows.First().Product);
                    Assert.AreEqual(10, order.Rows.First().Price);
                }
            }
        }
    }
}
