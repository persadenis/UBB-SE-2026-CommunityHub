using ChatAndEvents.Data.EventsData.Services.categoryServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryServices _categoryServices;

    public CategoriesController(ICategoryServices categoryServices)
    {
        _categoryServices = categoryServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryServices.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetCategory(int categoryId)
    {
        var category = await _categoryServices.GetCategoryByIdAsync(categoryId);

        if (category == null)
            return NotFound();

        return Ok(category);
    }
}