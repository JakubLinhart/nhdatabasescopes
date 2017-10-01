using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace NHibernateDatabaseScope.SampleDomainModel
{
    public class OrderRowMap : ClassMap<OrderRow>
    {
        public OrderRowMap()
        {
            Id(x => x.Id);

            Map(x => x.Price);
            Map(x => x.Product);
        }
    }
}
