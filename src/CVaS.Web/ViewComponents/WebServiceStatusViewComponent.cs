using CVaS.Shared.Options;
using CVaS.Shared.Services.Broker;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace CVaS.Web.ViewComponents
{
    public class WebServiceStatusViewComponent : ViewComponent
    {
        private readonly IBrokerStatus _brokerStatus;
        private readonly IOptions<ModeOptions> _modeOptions;

        public WebServiceStatusViewComponent(IBrokerStatus brokerStatus, IOptions<ModeOptions> modeOptions)
        {
            _modeOptions = modeOptions;
            _brokerStatus = brokerStatus;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var status = new WebServiceStatusViewModel()
            {
                IsAnyAlgServerOnline = (await _brokerStatus.GetConnectedAlgServersCount()) > 0,
                IsLocalMode = _modeOptions.Value.IsLocal
            };

            return View(status);
        }
    }
}