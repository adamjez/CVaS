using CVaS.BL.Providers;
using CVaS.Shared.Core.Provider;

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