using Simple.AutoMapper.Core;
using WebApiSample.Models;

namespace WebApiSample.Profiles;

/// <summary>
/// Profile for Product and Category entity and DTO mappings
/// </summary>
public class ProductProfile : Profile
{
    protected override void Configure()
    {
        // Product mappings
        CreateMap<ProductEntity, ProductDto>().ReverseMap();
        CreateMap<CreateProductDto, ProductEntity>();
        CreateMap<UpdateProductDto, ProductEntity>();

        // Category mappings
        CreateMap<CategoryEntity, CategoryDto>().ReverseMap();
        CreateMap<CreateCategoryDto, CategoryEntity>();
        CreateMap<UpdateCategoryDto, CategoryEntity>();
    }
}
