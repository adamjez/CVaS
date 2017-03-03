using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Facades
{
    public class RuleFacade : AppFacadeBase
    {
        private readonly RuleRepository _ruleRepository;

        public RuleFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, RuleRepository ruleRepository)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _ruleRepository = ruleRepository;
        }

        public async Task<List<RuleDTO>> GetAll()
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                return await uow.Context.Rules
                    .Select(r => new RuleDTO
                    {
                        Regex = r.Regex,
                        IsEnabled = r.IsEnabled,
                        Id = r.Id
                    }).ToListAsync();
            }
        }

        public async Task Insert(RuleDTO rule)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                _ruleRepository.Insert(new Rule()
                {
                    IsEnabled = true,
                    Regex = rule.Regex
                });

                await uow.CommitAsync();
            }
        }

        public async Task Remove(int id)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var rule = new Rule() {Id = id};
                uow.Context.Attach(rule);

                _ruleRepository.Delete(rule);

                await uow.CommitAsync();
            }
        }

        public async Task SetEnabled(int id, bool isEnabled)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var rule = await _ruleRepository.GetById(id);

                rule.IsEnabled = isEnabled;

                await uow.CommitAsync();
            }
        }

        public async Task<bool> Validate(string emailAddress)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var rules = await uow.Context.Rules.Where(r => r.IsEnabled).ToListAsync();

                return rules.All(rule => Regex.IsMatch(emailAddress, rule.Regex, RegexOptions.Singleline));
            }
        }
    }
}