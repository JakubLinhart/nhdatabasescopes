using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace NHibernateDatabaseScope.SampleDomainModel
{
    public class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Id(x => x.Id);

            HasMany(x => x.Rows).Cascade.AllDeleteOrphan();
            Map(x => x.RequiredDate);
            Map(x => x.ShipName);
        }
    }
}
