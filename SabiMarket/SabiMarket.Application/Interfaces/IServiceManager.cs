using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace SabiMarket.Application.Interfaces
{
    public interface IServiceManager
    {
        public IWaivedProductService IWaivedProductService { get; }
        public ISubscriptionService ISubscriptionService { get; }
        public ISubscriptionPlanService ISubscriptionPlanService { get; }
    }
}
