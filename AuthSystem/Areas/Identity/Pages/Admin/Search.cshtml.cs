using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class SearchModel : PageModel
{
    private readonly AuthDbContext _context;
    private readonly HttpClient _httpClient;

    public SearchModel(AuthDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    [BindProperty]
    public string SearchTerm { get; set; }

    public List<string> Results { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(SearchTerm))
        {
            Results = new List<string>();
            return Page();
        }

        // Join and filter data using the search term
        var query = from user in _context.Users
                    join message in _context.ChatMessages on user.Id equals message.SenderId
                    join room in _context.ChatRooms on message.ChatRoomId equals room.Id
                    where user.UserName.Contains(SearchTerm) || message.Content.Contains(SearchTerm) || room.Name.Contains(SearchTerm)
                    select new
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        MessageContent = message.Content,
                        ChatRoom = room.Name,
                        Timestamp = message.Timestamp
                    };

        var queryResult = await query.ToListAsync();

        // Combine relevant data for AI analysis
        var combinedData = queryResult.Select(qr => $"UserName: {qr.UserName}, MessageContent: {qr.MessageContent}, ChatRoom: {qr.ChatRoom}").ToList();

        // Get AI-generated response based on the search data
        string aiResponse = await GetRelevantDataAsync(SearchTerm, combinedData);

        // Set Results to only include AI response
        Results = new List<string>();

        if (!string.IsNullOrEmpty(aiResponse))
        {
            Results.Add($"AI Analysis: {aiResponse}");
        }

        return Page();
    }

    private async Task<string> GetRelevantDataAsync(string searchTerm, List<string> combinedData, string customTask = null)
    {
        string languagePrompt;

        if (!string.IsNullOrEmpty(customTask))
        {
            languagePrompt = $@"
                {customTask}
                The Given data is : {string.Join(", ", combinedData)}

                Note: Don't give question number after analysis; if required, please give details for questions.
            ";
        }
        else
        {
            languagePrompt = $@"
                Please analyze the given data and give positive smart analysis with emojis.
                The Given data is : {string.Join(", ", combinedData)}

                Note: Don't give question number after analysis; if required, please give details for questions.
                Give full analysis in a maximum of 100 words only.
                Start text with heading 'Analysis using AI'.
            ";
        }

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "You are processing search queries against a dataset." },
                new { role = "user", content = languagePrompt }
            },
            temperature = 0.7
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Add("Authorization", $"Bearer ");

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(jsonResponse);
                var chatGptResponse = jsonDocument.RootElement.GetProperty("choices")[0]
                    .GetProperty("message").GetProperty("content").GetString();

                return chatGptResponse;
            }
            else
            {
                Console.WriteLine($"API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing API request: " + ex.Message);
            return null;
        }
    }
}
