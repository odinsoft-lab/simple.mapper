using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;

namespace Simple.AutoMapper.Examples
{
    // ── Basic Mapping Models ──────────────────────────────────

    public class UserEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public AddressEntity Address { get; set; }
        public List<OrderEntity> Orders { get; set; }
    }

    public class AddressEntity
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class OrderEntity
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public AddressDTO Address { get; set; }
        public List<OrderDTO> Orders { get; set; }
    }

    public class AddressDTO
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class OrderDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // ── ForMember / Condition / NullSubstitute Models ─────────

    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int BirthYear { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string Nickname { get; set; }
        public string Department { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Department { get; set; }
    }

    // ── BeforeMap / AfterMap Models ───────────────────────────

    public class AuditSource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class AuditDest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime MappedAt { get; set; }
        public string Summary { get; set; }
    }

    // ── ConstructUsing Models ─────────────────────────────────

    public class SourceData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class ImmutableRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public ImmutableRecord() { }

        public ImmutableRecord(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public override string ToString() => $"ImmutableRecord(Id={Id}, Name={Name}, Email={Email})";
    }

    // ── Circular Reference Models ─────────────────────────────

    public class ParentNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ChildNode> Children { get; set; } = new();
    }

    public class ChildNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ParentNode Parent { get; set; }
    }

    public class ParentNodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ChildNodeDto> Children { get; set; } = new();
    }

    public class ChildNodeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ParentNodeDto Parent { get; set; }
    }

    // ── Patch Models ──────────────────────────────────────────

    public class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Default";
        public decimal Price { get; set; } = 0m;
        public int Stock { get; set; } = 100;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductDto
    {
        public string Name { get; set; }      // null = skip update
        public decimal? Price { get; set; }    // null = skip update
        public int? Stock { get; set; }        // null = skip update
        public bool? IsActive { get; set; }    // null = skip update
    }

    // ── Profile Models ────────────────────────────────────────

    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string AuthorName { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class ArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string AuthorName { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class ArticleSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }

    /// <summary>
    /// Profile groups related mapping configurations together.
    /// Override Configure() to define mappings.
    /// </summary>
    public class ArticleProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Article, ArticleDto>().ReverseMap();
            CreateMap<Article, ArticleSummaryDto>()
                .ForMember<string>(d => d.Author, opt => opt.MapFrom(s => s.AuthorName));
        }
    }
}
