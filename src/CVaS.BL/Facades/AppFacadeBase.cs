using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;

namespace CVaS.BL.Facades
{

    public abstract class AppFacadeBase
    {
        protected IUnitOfWorkProvider UnitOfWorkProvider;

        protected ICurrentUserProvider CurrentUserProvider;

        protected AppFacadeBase(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider)
        {
            UnitOfWorkProvider = unitOfWorkProvider;
            CurrentUserProvider = currentUserProvider;
        }
    }
}