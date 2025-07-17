using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Interfaces;

public interface IDiscussionService
{
    Task<IEnumerable<DiscussionDto>> GetDiscussionsAsync(int? interventionId = null, int? protocolId = null);
    Task<DiscussionDetailDto?> GetDiscussionAsync(int id);
    Task<DiscussionDto> CreateDiscussionAsync(CreateDiscussionDto discussion);
    Task<CommentDto> AddCommentAsync(CreateCommentDto comment);
    Task<VoteDto> VoteAsync(CreateVoteDto vote);
}