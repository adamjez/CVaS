using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace CVaS.BL.Core.Registry
{
    /// <summary>
    /// A storage for unit of work objects which persists data in HttpContext.Items collection.
    /// </summary>
    public class HttpContextUnitOfWorkRegistry : UnitOfWorkRegistryBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UnitOfWorkRegistryBase alternateRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextUnitOfWorkRegistry"/> class.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="alternateRegistry">An alternate storage that will be used for threads not associated with any HTTP request.</param>
        public HttpContextUnitOfWorkRegistry(IHttpContextAccessor httpContextAccessor, UnitOfWorkRegistryBase alternateRegistry)
        {
            _httpContextAccessor = httpContextAccessor;
            this.alternateRegistry = alternateRegistry;
        }

        /// <summary>
        /// Gets the stack of currently active unit of work objects.
        /// </summary>
        protected internal override Stack<IUnitOfWork> GetStack()
        {
            if (_httpContextAccessor.HttpContext == null)
            {

                return alternateRegistry.GetStack();
            }
            else
            {
                var stack = _httpContextAccessor.HttpContext.Items[typeof(HttpContextUnitOfWorkRegistry)] as Stack<IUnitOfWork>;
                if (stack == null)
                {
                    stack = new Stack<IUnitOfWork>();
                    _httpContextAccessor.HttpContext.Items[typeof(HttpContextUnitOfWorkRegistry)] = stack;
                }

                return stack;
            }
        }
    }
}