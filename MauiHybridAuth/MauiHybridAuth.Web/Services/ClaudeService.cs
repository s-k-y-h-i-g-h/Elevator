using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MauiHybridAuth.Web.Services
{
    public class ClaudeService
    {
        private readonly AnthropicClient _client;
        private readonly ILogger<ClaudeService> _logger;
        private readonly IConfiguration _configuration;

        public ClaudeService(AnthropicClient client, ILogger<ClaudeService> logger, IConfiguration configuration)
        {
            _client = client;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ClaudeResponse> QueryAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending query to Claude: {Prompt}", prompt);

                var parameters = new MessageParameters
                {
                    Messages = new List<Message>
                    {
                        new Message(RoleType.User, prompt)
                    },
                    MaxTokens = 4000,
                    Model = "claude-3-5-sonnet-20241022",
                    Temperature = 0.7m
                };

                var response = await _client.Messages.GetClaudeMessageAsync(parameters);

                var claudeResponse = new ClaudeResponse
                {
                    Content = response.Message.ToString(),
                    InputTokens = response.Usage?.InputTokens ?? 0,
                    OutputTokens = response.Usage?.OutputTokens ?? 0,
                    Model = response.Model ?? "claude-3-5-sonnet-20241022",
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Received response from Claude. Input tokens: {InputTokens}, Output tokens: {OutputTokens}", 
                    claudeResponse.InputTokens, claudeResponse.OutputTokens);

                return claudeResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Claude with prompt: {Prompt}", prompt);
                throw;
            }
        }

        public async IAsyncEnumerable<string> QueryStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting streaming query to Claude: {Prompt}", prompt);

            var parameters = new MessageParameters
            {
                Messages = new List<Message>
                {
                    new Message(RoleType.User, prompt)
                },
                MaxTokens = 4000,
                Model = "claude-3-5-sonnet-20241022",
                Temperature = 0.7m,
                Stream = true
            };

            var stream = _client.Messages.StreamClaudeMessageAsync(parameters);

            await foreach (var chunk in stream.WithCancellation(cancellationToken))
            {
                if (chunk.Delta?.Text != null)
                {
                    yield return chunk.Delta.Text;
                }
            }
        }

        public async Task<ClaudeResponse> QueryWithContextAsync(IEnumerable<ClaudeMessage> messages, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending contextual query to Claude with {MessageCount} messages", messages.Count());

                var anthropicMessages = messages.Select(m => new Message(
                    m.Role.ToLowerInvariant() == "user" ? RoleType.User : RoleType.Assistant,
                    m.Content
                )).ToList();

                var parameters = new MessageParameters
                {
                    Messages = anthropicMessages,
                    MaxTokens = 4000,
                    Model = "claude-3-5-sonnet-20241022",
                    Temperature = 0.7m
                };

                var response = await _client.Messages.GetClaudeMessageAsync(parameters);

                var claudeResponse = new ClaudeResponse
                {
                    Content = response.Message.ToString(),
                    InputTokens = response.Usage?.InputTokens ?? 0,
                    OutputTokens = response.Usage?.OutputTokens ?? 0,
                    Model = response.Model ?? "claude-3-5-sonnet-20241022",
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Received contextual response from Claude. Input tokens: {InputTokens}, Output tokens: {OutputTokens}", 
                    claudeResponse.InputTokens, claudeResponse.OutputTokens);

                return claudeResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Claude with context");
                throw;
            }
        }
    }

    public class ClaudeResponse
    {
        public string Content { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public decimal CostUsd { get; set; }
        public string Model { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ClaudeMessage
    {
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 