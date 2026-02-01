using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.AutoMapper.Examples
{
    /// <summary>
    /// Map vs Patch comparison and performance benchmarks.
    /// </summary>
    public static class ComparisonExamples
    {
        /// <summary>
        /// 18. Map vs Patch — shows the key difference:
        /// Map copies ALL properties (including null).
        /// Patch skips null source properties, preserving destination values.
        /// </summary>
        public static void MapVsPatchComparison()
        {
            Console.WriteLine("=== 18. Map vs Patch Comparison ===\n");
            Mapper.CreateMap<UpdateProductDto, ProductEntity>();

            var partial = new UpdateProductDto { Name = "New Name" };

            // Map: overwrites ALL properties (null source -> null/default destination)
            var mapTarget = new ProductEntity
            {
                Id = 1, Name = "Original", Price = 50m, Stock = 200, IsActive = true
            };
            Mapper.Map(partial, mapTarget);

            Console.WriteLine("  Map (full overwrite):");
            Console.WriteLine($"    Name={mapTarget.Name}, Price={mapTarget.Price}, Stock={mapTarget.Stock}, Active={mapTarget.IsActive}");

            // Patch: only updates non-null properties
            var patchTarget = new ProductEntity
            {
                Id = 1, Name = "Original", Price = 50m, Stock = 200, IsActive = true
            };
            Mapper.Patch(partial, patchTarget);

            Console.WriteLine("  Patch (null-skip):");
            Console.WriteLine($"    Name={patchTarget.Name}, Price={patchTarget.Price}, Stock={patchTarget.Stock}, Active={patchTarget.IsActive}");
            Console.WriteLine();
        }

        /// <summary>
        /// 19. Performance — expression tree compilation + caching.
        /// First call compiles; subsequent calls use cached delegate.
        /// </summary>
        public static void PerformanceExample()
        {
            Console.WriteLine("=== 19. Performance: Compilation + Caching ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();
            Mapper.CreateMap<OrderEntity, OrderDTO>();

            var entities = Enumerable.Range(0, 10000).Select(i => new UserEntity
            {
                Id = i,
                FirstName = $"User{i}",
                LastName = $"Last{i}",
                Address = new AddressEntity { Street = $"{i} Main St", City = "City", ZipCode = "12345" }
            }).ToList();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var first = Mapper.Map<UserEntity, UserDTO>(entities[0]);
            sw.Stop();
            Console.WriteLine($"  First mapping (compile + cache): {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            var all = Mapper.Map<UserEntity, UserDTO>(entities);
            sw.Stop();
            Console.WriteLine($"  10,000 mappings (cached): {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Average per entity: {sw.ElapsedMilliseconds / (double)entities.Count:F4}ms");
            Console.WriteLine();
        }
    }
}
