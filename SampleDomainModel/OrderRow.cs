using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateDatabaseScope.SampleDomainModel
{
    public class OrderRow
    {
        public virtual int Id { get; set; }
        public virtual decimal Price { get; set; }
        public virtual string Product { get; set; }
    }
}
