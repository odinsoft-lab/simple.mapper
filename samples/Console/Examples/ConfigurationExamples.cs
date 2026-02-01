using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;

namespace Simple.AutoMapper.Examples
{
    /// <summary>
    /// Configuration APIs — Ignore, ForMember, Condition, NullSubstitute,
    /// BeforeMap/AfterMap, ConstructUsing, ReverseMap, PreserveReferences, Profile.
    /// </summary>
    public static class ConfigurationExamples
    {
        /// <summary>
        /// 9. Ignore — exclude specific properties from mapping.
        /// </summary>
        public static void IgnoreExample()
        {
            Console.WriteLine("=== 9. Ignore: .Ignore(d => d.Property) ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>()
                .Ignore(d => d.BirthDate);

            var user = new UserEntity
            {
                Id = 1, FirstName = "Jane", LastName = "Smith",
                BirthDate = new DateTime(1985, 5, 15)
            };

            var dto = Mapper.Map<UserEntity, UserDTO>(user);

            Console.WriteLine($"  Name: {dto.FirstName} {dto.LastName}");
            Console.WriteLine($"  BirthDate (ignored): {dto.BirthDate}");  // 0001-01-01
            Console.WriteLine();
        }

        /// <summary>
        /// 10. ForMember + MapFrom — custom property mapping with computed values.
        /// </summary>
        public static void ForMemberMapFromExample()
        {
            Console.WriteLine("=== 10. ForMember + MapFrom ===\n");
            Mapper.CreateMap<Employee, EmployeeDto>()
                .ForMember<string>(d => d.FullName, opt =>
                    opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember<int>(d => d.Age, opt =>
                    opt.MapFrom(s => DateTime.Now.Year - s.BirthYear));

            var emp = new Employee
            {
                Id = 1, FirstName = "John", LastName = "Doe",
                BirthYear = 1990, Department = "Engineering"
            };

            var dto = Mapper.Map<Employee, EmployeeDto>(emp);

            Console.WriteLine($"  FullName: {dto.FullName}");      // "John Doe" (computed)
            Console.WriteLine($"  Age: {dto.Age}");                 // computed from BirthYear
            Console.WriteLine($"  Department: {dto.Department}");   // auto-mapped by name
            Console.WriteLine();
        }

        /// <summary>
        /// 11. Condition — map a property only when a condition is met.
        /// </summary>
        public static void ConditionExample()
        {
            Console.WriteLine("=== 11. Condition: opt.Condition(s => bool) ===\n");
            Mapper.CreateMap<Employee, EmployeeDto>()
                .ForMember<string>(d => d.Email, opt =>
                {
                    opt.MapFrom(s => s.Email);
                    opt.Condition(s => s.IsEmailVerified);
                });

            var verified = new Employee { Id = 1, Email = "john@example.com", IsEmailVerified = true };
            var unverified = new Employee { Id = 2, Email = "spam@example.com", IsEmailVerified = false };

            var dto1 = Mapper.Map<Employee, EmployeeDto>(verified);
            var dto2 = Mapper.Map<Employee, EmployeeDto>(unverified);

            Console.WriteLine($"  Verified user email:   \"{dto1.Email}\"");
            Console.WriteLine($"  Unverified user email: \"{dto2.Email}\"");
            Console.WriteLine();
        }

        /// <summary>
        /// 12. NullSubstitute — provide a default value when source property is null.
        /// </summary>
        public static void NullSubstituteExample()
        {
            Console.WriteLine("=== 12. NullSubstitute: opt.NullSubstitute(value) ===\n");
            Mapper.CreateMap<Employee, EmployeeDto>()
                .ForMember<string>(d => d.DisplayName, opt =>
                {
                    opt.MapFrom(s => s.Nickname);
                    opt.NullSubstitute("Anonymous");
                });

            var withNick = new Employee { Id = 1, Nickname = "Johnny" };
            var withoutNick = new Employee { Id = 2, Nickname = null };

            var dto1 = Mapper.Map<Employee, EmployeeDto>(withNick);
            var dto2 = Mapper.Map<Employee, EmployeeDto>(withoutNick);

            Console.WriteLine($"  With nickname:    \"{dto1.DisplayName}\"");   // "Johnny"
            Console.WriteLine($"  Without nickname: \"{dto2.DisplayName}\"");   // "Anonymous"
            Console.WriteLine();
        }

        /// <summary>
        /// 13. BeforeMap / AfterMap — execute callbacks before and after mapping.
        /// </summary>
        public static void BeforeAfterMapExample()
        {
            Console.WriteLine("=== 13. BeforeMap / AfterMap ===\n");
            Mapper.CreateMap<AuditSource, AuditDest>()
                .BeforeMap((src, dest) => { dest.MappedAt = DateTime.UtcNow; })
                .AfterMap((src, dest) => { dest.Summary = $"[{dest.Id}] {dest.Title}"; });

            var source = new AuditSource { Id = 42, Title = "Test Article", Content = "Hello World" };
            var dest = Mapper.Map<AuditSource, AuditDest>(source);

            Console.WriteLine($"  Title: {dest.Title}");
            Console.WriteLine($"  MappedAt (BeforeMap): {dest.MappedAt:O}");
            Console.WriteLine($"  Summary (AfterMap): {dest.Summary}");
            Console.WriteLine();
        }

        /// <summary>
        /// 14. ConstructUsing — custom object construction.
        /// </summary>
        public static void ConstructUsingExample()
        {
            Console.WriteLine("=== 14. ConstructUsing ===\n");
            Mapper.CreateMap<SourceData, ImmutableRecord>()
                .ConstructUsing(src => new ImmutableRecord(src.Id, src.Name, src.Email));

            var source = new SourceData { Id = 1, Name = "Alice", Email = "alice@example.com" };
            var record = Mapper.Map<SourceData, ImmutableRecord>(source);

            Console.WriteLine($"  {record}");
            Console.WriteLine();
        }

        /// <summary>
        /// 15. ReverseMap — create bidirectional mapping with a single call.
        /// </summary>
        public static void ReverseMapExample()
        {
            Console.WriteLine("=== 15. ReverseMap ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>()
                .ReverseMap();

            var entity = new UserEntity { Id = 1, FirstName = "John", LastName = "Doe" };

            var dto = Mapper.Map<UserEntity, UserDTO>(entity);
            Console.WriteLine($"  Entity -> DTO: {dto.FirstName} {dto.LastName}");

            var backToEntity = Mapper.Map<UserDTO, UserEntity>(dto);
            Console.WriteLine($"  DTO -> Entity: {backToEntity.FirstName} {backToEntity.LastName}");
            Console.WriteLine();
        }

        /// <summary>
        /// 16. PreserveReferences + MaxDepth — handle circular references.
        /// </summary>
        public static void PreserveReferencesExample()
        {
            Console.WriteLine("=== 16. PreserveReferences + MaxDepth ===\n");

            // Configuration example:
            //   Mapper.CreateMap<ParentNode, ParentNodeDto>()
            //       .PreserveReferences()    // Track visited objects
            //       .MaxDepth(3);            // Limit recursion depth
            //
            // Use for EF Core entities with Parent <-> Child relationships.

            Console.WriteLine("  PreserveReferences() - enables reference tracking for circular graphs");
            Console.WriteLine("  MaxDepth(n) - limits recursion depth for deeply nested objects");
            Console.WriteLine("  Note: Full MaxDepth enforcement is a work-in-progress (see TASK.md)");
            Console.WriteLine();
        }

        /// <summary>
        /// 17. Profile — group related mappings into a reusable Profile class.
        /// </summary>
        public static void ProfileExample()
        {
            Console.WriteLine("=== 17. Profile ===\n");
            Mapper.AddProfile<ArticleProfile>();

            var article = new Article
            {
                Id = 1,
                Title = "Getting Started with Simple.AutoMapper",
                Body = "This article explains...",
                AuthorName = "John Doe",
                PublishedAt = DateTime.Now
            };

            var fullDto = Mapper.Map<Article, ArticleDto>(article);
            Console.WriteLine($"  Full DTO: {fullDto.Title} by {fullDto.AuthorName}");

            var summary = Mapper.Map<Article, ArticleSummaryDto>(article);
            Console.WriteLine($"  Summary: {summary.Title} by {summary.Author}");

            var backToArticle = Mapper.Map<ArticleDto, Article>(fullDto);
            Console.WriteLine($"  Reverse: {backToArticle.Title} by {backToArticle.AuthorName}");
            Console.WriteLine();
        }
    }
}
