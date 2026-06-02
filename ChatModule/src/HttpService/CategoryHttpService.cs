using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.categoryServices;

public class CategoryHttpService : ICategoryServices
{
    private readonly HttpClient _httpClient;

    public CategoryHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var categories = await _httpClient.GetFromJsonAsync<List<Category>>("api/Categories");
        return categories ?? new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        var response = await _httpClient.GetAsync($"api/Categories/{categoryId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>();
    }
}
