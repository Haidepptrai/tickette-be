using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.CommitteeMembers.Command.AddCommitteeMember;
using Tickette.Application.Features.CommitteeMembers.Command.ChangeCommitteeMemberRole;
using Tickette.Application.Features.CommitteeMembers.Command.RemoveCommitteeMember;
using Tickette.Application.Features.CommitteeMembers.Query.GetAllCommitteeMemberOfEvent;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Application.Wrappers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommitteeMembersController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public CommitteeMembersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        // GET: api/committeeMembers
        [HttpPost("members")]
        public async Task<ResponseDto<GetAllCommitteeMemberOfEventResponse>> GetCommitteeMembersByEvent(GetAllCommitteeMemberOfEventQuery query, CancellationToken cancellation)
        {
            var result = await _queryDispatcher.Dispatch<GetAllCommitteeMemberOfEventQuery, GetAllCommitteeMemberOfEventResponse>(query, cancellation);

            return ResponseHandler.SuccessResponse(result, "Get all committee members successfully");
        }


        // POST: api/committeeMembers
        [HttpPost("add-member")]
        public async Task<ResponseDto<object>> AddNewMemberToEvent([FromBody] AddCommitteeMemberCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<AddCommitteeMemberCommand, object>(command, cancellation);

            return ResponseHandler.SuccessResponse(result, "Add member to event successfully");
        }

        // PUT: api/committeeMembers/5
        // Update member role
        [HttpPost("update-role")]
        public async Task<ResponseDto<object>> ChangeMemberRole([FromBody] ChangeCommitteeMemberRoleCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<ChangeCommitteeMemberRoleCommand, object>(command, cancellation);

            return ResponseHandler.SuccessResponse(result, "Update member role successfully");
        }


        // DELETE:
        [HttpDelete]
        public async Task<ResponseDto<object>> RemoveMemberFromEvent([FromBody] RemoveCommitteeMemberCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<RemoveCommitteeMemberCommand, object>(command, cancellation);

            return ResponseHandler.SuccessResponse(result, "Remove member from event successfully");
        }
    }
}
