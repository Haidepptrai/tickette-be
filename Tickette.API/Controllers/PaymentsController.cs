using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public PaymentsController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }


    }
}
