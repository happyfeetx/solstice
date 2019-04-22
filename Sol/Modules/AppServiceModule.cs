#region USING DIRECTIVES

using Sol.Common;
using Sol.Database;
using Sol.Services;

#endregion USING DIRECTIVES

namespace Sol.Modules
{
    public abstract class AppServiceModule<TService> : AppModule where TService : IKioskService
    {
        protected TService Service { get; }


        protected AppServiceModule(TService service, SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.Service = service;
        }
    }
}
