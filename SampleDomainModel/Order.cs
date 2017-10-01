using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateDatabaseScope.SampleDomainModel
{
    public class Order
    {
        public Order()
        {
            Rows = new Iesi.Collections.Generic.HashedSet<OrderRow>();
        }

        public virtual int Id { get; set; }
        public virtual string ShipName { get; set; }
        public virtual DateTime RequiredDate { get; set; }

        public virtual Iesi.Collections.Generic.ISet<OrderRow> Rows { get; set; }
    }
}
