using System.Net;
using System.Text;
using System.Text.Json;
using Elevator.Shared.Services.DTOs;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui.Services;

public class HttpDiscussionService : IDiscussionService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpDiscussionService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<DiscussionDto>> GetDiscussionsAsync(int? interventionId = null, int? protocolId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (interventionId.HasValue)
                queryParams.Add($"interventionId={interventionId.Value}");
            if (protocolId.HasValue)
                queryParams.Add($"protocolId={protocolId.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"discussions{queryString}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"discussions{queryString}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var discussions = JsonSerializer.Deserialize<IEnumerable<DiscussionDto>>(json, _jsonOptions);
            return discussions ?? new List<DiscussionDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve discussions: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving discussions", ex);
        }
    }

    public async Task<DiscussionDetailDto?> GetDiscussionAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"discussions/{id}");
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.GetAsync($"discussions/{id}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DiscussionDetailDto>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to retrieve discussion {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while retrieving discussion", ex);
        }
    }

    public async Task<DiscussionDto> CreateDiscussionAsync(CreateDiscussionDto discussion)
    {
        try
        {
            // Set the user ID from the current authenticated user
            if (string.IsNullOrEmpty(discussion.UserId))
            {
                discussion.UserId = _authService.CurrentUserId;
            }

            var json = JsonSerializer.Serialize(discussion, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("discussions", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PostAsync("discussions", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DiscussionDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to create discussion: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while creating discussion", ex);
        }
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentDto comment)
    {
        try
        {
            // Set the user ID from the current authenticated user
            if (string.IsNullOrEmpty(comment.UserId))
            {
                comment.UserId = _authService.CurrentUserId;
            }

            var json = JsonSerializer.Serialize(comment, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("discussions/comments", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PostAsync("discussions/comments", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CommentDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to add comment: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while adding comment", ex);
        }
    }

    public async Task<VoteDto> VoteAsync(CreateVoteDto vote)
    {
        try
        {
            // Set the user ID from the current authenticated user
            if (string.IsNullOrEmpty(vote.UserId))
            {
                vote.UserId = _authService.CurrentUserId;
            }

            var json = JsonSerializer.Serialize(vote, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("discussions/votes", content);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await HandleUnauthorizedAsync())
                {
                    response = await _httpClient.PostAsync("discussions/votes", content);
                }
                else
                {
                    throw new UnauthorizedAccessException("Authentication required");
                }
            }

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<VoteDto>(responseJson, _jsonOptions);
            return result ?? throw new ServiceException("Invalid response from server");
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Failed to vote: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ServiceException("Request timed out while voting", ex);
        }
    }

    private async Task<bool> HandleUnauthorizedAsync()
    {
        if (_authService is HttpAuthenticationService httpAuthService)
        {
            return await httpAuthService.HandleUnauthorizedAsync();
        }
        return false;
    }
}