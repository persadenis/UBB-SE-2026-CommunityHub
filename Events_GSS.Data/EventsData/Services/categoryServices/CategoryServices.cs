using System;
using System.Collections.Generic;
using System.Text;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.categoriesRepository;

namespace ChatAndEvents.Data.EventsData.Services.categoryServices;

/// <summary>
/// Service for managing category operations.
/// </summary>
public class CategoryServices : ICategoryServices
{
    private readonly ICategoryRepository _categoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryServices"/> class.
    /// </summary>
    /// <param name="categoryRepository">The category repository.</param>
    public CategoryServices(ICategoryRepository categoryRepository)
    {
        this._categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Gets all categories asynchronously.
    /// </summary>
    /// <returns>A list of all categories.</returns>
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await this._categoryRepository.GetAllAsync();
    }

    /// <summary>
    /// Gets a category by identifier asynchronously.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The category if found; otherwise, null.</returns>
    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        return await this._categoryRepository.GetByIdAsync(categoryId);
    }
}
