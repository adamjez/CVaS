using CVaS.BL.Core.Provider;
using CVaS.BL.Providers;

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