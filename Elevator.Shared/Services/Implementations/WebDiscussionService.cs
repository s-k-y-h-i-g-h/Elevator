using Microsoft.EntityFrameworkCore;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Discussion;
using Elevator.Shared.Services.Interfaces;
using Elevator.Shared.Services.DTOs;

namespace Elevator.Shared.Services.Implementations;

public class WebDiscussionService : IDiscussionService
{
    private readonly ElevatorDbContext _context;

    public WebDiscussionService(ElevatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DiscussionDto>> GetDiscussionsAsync(int? interventionId = null, int? protocolId = null)
    {
        var query = _context.Discussions
            .Include(d => d.User)
            .Include(d => d.Comments)
            .Include(d => d.Votes)
            .Include(d => d.Intervention)
            .Include(d => d.Protocol)
            .AsQueryable();

        if (interventionId.HasValue)
            query = query.Where(d => d.InterventionId == interventionId);
        if (protocolId.HasValue)
            query = query.Where(d => d.ProtocolId == protocolId);

        var discussions = await query.ToListAsync();
        return discussions.Select(MapToDto);
    }

    public async Task<DiscussionDetailDto?> GetDiscussionAsync(int id)
    {
        var discussion = await _context.Discussions
            .Include(d => d.User)
            .Include(d => d.Comments)
                .ThenInclude(c => c.User)
            .Include(d => d.Comments)
                .ThenInclude(c => c.ChildComments)
                    .ThenInclude(cc => cc.User)
            .Include(d => d.Comments)
                .ThenInclude(c => c.Votes)
            .Include(d => d.Votes)
            .Include(d => d.Intervention)
            .Include(d => d.Protocol)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (discussion == null)
            return null;

        return MapToDetailDto(discussion);
    }

    public async Task<DiscussionDto> CreateDiscussionAsync(CreateDiscussionDto discussionDto)
    {
        var discussion = new Models.Discussion.Discussion
        {
            Title = discussionDto.Title,
            Content = discussionDto.Content,
            InterventionId = discussionDto.InterventionId,
            ProtocolId = discussionDto.ProtocolId,
            UserId = discussionDto.UserId ?? throw new ArgumentException("UserId is required"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Discussions.Add(discussion);
        await _context.SaveChangesAsync();

        // Reload with includes
        var createdDiscussion = await _context.Discussions
            .Include(d => d.User)
            .Include(d => d.Comments)
            .Include(d => d.Votes)
            .Include(d => d.Intervention)
            .Include(d => d.Protocol)
            .FirstAsync(d => d.Id == discussion.Id);

        return MapToDto(createdDiscussion);
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentDto commentDto)
    {
        var comment = new Comment
        {
            Content = commentDto.Content,
            DiscussionId = commentDto.DiscussionId,
            ParentCommentId = commentDto.ParentCommentId,
            UserId = commentDto.UserId ?? throw new ArgumentException("UserId is required"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Reload with includes
        var createdComment = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Votes)
            .Include(c => c.ChildComments)
            .FirstAsync(c => c.Id == comment.Id);

        return MapCommentToDto(createdComment);
    }

    public async Task<VoteDto> VoteAsync(CreateVoteDto voteDto)
    {
        var userId = voteDto.UserId ?? throw new ArgumentException("UserId is required");
        
        // Check if user already voted
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId &&
                                    v.DiscussionId == voteDto.DiscussionId &&
                                    v.CommentId == voteDto.CommentId);

        if (existingVote != null)
        {
            // Update existing vote
            existingVote.IsUpvote = voteDto.IsUpvote;
            await _context.SaveChangesAsync();
            return MapVoteToDto(existingVote);
        }

        // Create new vote
        var vote = new Vote
        {
            UserId = userId,
            DiscussionId = voteDto.DiscussionId,
            CommentId = voteDto.CommentId,
            IsUpvote = voteDto.IsUpvote,
            CreatedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        return MapVoteToDto(vote);
    }

    private static DiscussionDto MapToDto(Models.Discussion.Discussion discussion)
    {
        return new DiscussionDto
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Content = discussion.Content,
            UserId = discussion.UserId,
            UserName = discussion.User?.Email ?? "Unknown",
            InterventionId = discussion.InterventionId,
            InterventionName = discussion.Intervention?.Name,
            ProtocolId = discussion.ProtocolId,
            ProtocolName = discussion.Protocol?.Name,
            CreatedAt = discussion.CreatedAt,
            UpdatedAt = discussion.UpdatedAt,
            CommentCount = discussion.Comments.Count,
            UpvoteCount = discussion.Votes.Count(v => v.IsUpvote),
            DownvoteCount = discussion.Votes.Count(v => !v.IsUpvote)
        };
    }

    private static DiscussionDetailDto MapToDetailDto(Models.Discussion.Discussion discussion)
    {
        return new DiscussionDetailDto
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Content = discussion.Content,
            UserId = discussion.UserId,
            UserName = discussion.User?.Email ?? "Unknown",
            InterventionId = discussion.InterventionId,
            InterventionName = discussion.Intervention?.Name,
            ProtocolId = discussion.ProtocolId,
            ProtocolName = discussion.Protocol?.Name,
            CreatedAt = discussion.CreatedAt,
            UpdatedAt = discussion.UpdatedAt,
            CommentCount = discussion.Comments.Count,
            UpvoteCount = discussion.Votes.Count(v => v.IsUpvote),
            DownvoteCount = discussion.Votes.Count(v => !v.IsUpvote),
            Comments = discussion.Comments
                .Where(c => c.ParentCommentId == null) // Only top-level comments
                .Select(MapCommentToDto)
                .ToList()
        };
    }

    private static CommentDto MapCommentToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            UserId = comment.UserId,
            UserName = comment.User?.Email ?? "Unknown",
            DiscussionId = comment.DiscussionId,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            UpvoteCount = comment.Votes.Count(v => v.IsUpvote),
            DownvoteCount = comment.Votes.Count(v => !v.IsUpvote),
            Replies = comment.ChildComments.Select(MapCommentToDto).ToList()
        };
    }

    private static VoteDto MapVoteToDto(Vote vote)
    {
        return new VoteDto
        {
            Id = vote.Id,
            UserId = vote.UserId,
            DiscussionId = vote.DiscussionId,
            CommentId = vote.CommentId,
            IsUpvote = vote.IsUpvote,
            CreatedAt = vote.CreatedAt
        };
    }
}