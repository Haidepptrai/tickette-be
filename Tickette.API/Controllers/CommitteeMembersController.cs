using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.CommitteeMembers.Command.AddCommitteeMember;
using Tickette.Application.Features.CommitteeMembers.Command.ChangeCommitteeMemberRole;
using Tickette.Application.Features.CommitteeMembers.Command.RemoveCommitteeMember;
using Tickette.Application.Features.CommitteeMembers.Query.GetAllCommitteeMemberOfEvent;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

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

        [HttpPost("members")]
        [SwaggerOperation("Get all committee members of event")]
        [Authorize(Policy = CommitteeMemberKeys.ManagerAccess)]
        public async Task<ResponseDto<GetAllCommitteeMemberOfEventResponse>> GetCommitteeMembersByEvent(GetAllCommitteeMemberOfEventQuery query, CancellationToken cancellation)
        {
            var result = await _queryDispatcher.Dispatch<GetAllCommitteeMemberOfEventQuery, GetAllCommitteeMemberOfEventResponse>(query, cancellation);

            return ResponseHandler.SuccessResponse(result, "Get all committee members successfully");
        }


        [HttpPost("add-member")]
        [SwaggerOperation(Summary = "Add new committee member to event", Description = "Will sent an email for member after add")]
        [Authorize(Policy = CommitteeMemberKeys.ManagerAccess)]
        public async Task<ResponseDto<CommitteeMemberDto>> AddNewMemberToEvent([FromBody] AddCommitteeMemberCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<AddCommitteeMemberCommand, CommitteeMemberDto>(command, cancellation);

            return ResponseHandler.SuccessResponse(result, "Add member to event successfully");
        }

        // Update member role
        [HttpPost("update-role")]
        [Authorize(Policy = CommitteeMemberKeys.ManagerAccess)]
        public async Task<ResponseDto<Unit>> ChangeMemberRole([FromBody] ChangeCommitteeMemberRoleCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<ChangeCommitteeMemberRoleCommand, Unit>(command, cancellation);

            return ResponseHandler.SuccessResponse(result, "Update member role successfully");
        }

        [HttpDelete]
        [Authorize(Policy = CommitteeMemberKeys.ManagerAccess)]
        public async Task<ResponseDto<Unit>> RemoveMemberFromEvent([FromBody] RemoveCommitteeMemberCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<RemoveCommitteeMemberCommand, Unit>(command, cancellation);

            return ResponseHandler.SuccessResponse(result, "Remove member from event successfully");
        }
    }
}
