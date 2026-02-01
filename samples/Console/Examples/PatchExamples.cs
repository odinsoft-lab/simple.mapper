using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;

namespace Simple.AutoMapper.Examples
{
    /// <summary>
    /// Patch API — 4 overloads with null-skip semantics.
    /// Null source properties are skipped, preserving destination values.
    /// </summary>
    public static class PatchExamples
    {
        /// <summary>
        /// 5. Patch&lt;TSource, TDestination&gt;(source) — new object with null-skip.
        /// </summary>
        public static void PatchNewObjectExample()
        {
            Console.WriteLine("=== 5. Patch (New Object): Patch<TSource, TDestination>(source) ===\n");
            Mapper.CreateMap<UpdateProductDto, ProductEntity>();

            // Only Name and Price are set; Stock and IsActive are null → skipped
            var update = new UpdateProductDto { Name = "Widget", Price = 29.99m };

            var entity = Mapper.Patch<UpdateProductDto, ProductEntity>(update);

            Console.WriteLine($"  Name: {entity.Name}");            // "Widget" (from source)
            Console.WriteLine($"  Price: {entity.Price}");           // 29.99 (from source)
            Console.WriteLine($"  Stock: {entity.Stock}");           // 100 (constructor default, preserved)
            Console.WriteLine($"  IsActive: {entity.IsActive}");     // true (constructor default, preserved)
            Console.WriteLine();
        }

        /// <summary>
        /// 6. Patch&lt;TDestination&gt;(object) — type-inferred with null-skip.
        /// </summary>
        public static void PatchTypeInferredExample()
        {
            Console.WriteLine("=== 6. Patch (Type-Inferred): Patch<TDestination>(source) ===\n");
            Mapper.CreateMap<UpdateProductDto, ProductEntity>();

            var update = new UpdateProductDto { Name = "Gadget" };

            var entity = Mapper.Patch<ProductEntity>(update);

            Console.WriteLine($"  Name: {entity.Name}");            // "Gadget" (from source)
            Console.WriteLine($"  Price: {entity.Price}");           // 0 (default, preserved)
            Console.WriteLine($"  Stock: {entity.Stock}");           // 100 (default, preserved)
            Console.WriteLine();
        }

        /// <summary>
        /// 7. Patch&lt;TSource, TDestination&gt;(IEnumerable) — collection with null-skip.
        /// </summary>
        public static void PatchCollectionExample()
        {
            Console.WriteLine("=== 7. Patch (Collection): Patch<TSource, TDestination>(list) ===\n");
            Mapper.CreateMap<UpdateProductDto, ProductEntity>();

            var updates = new List<UpdateProductDto>
            {
                new UpdateProductDto { Name = "Item A", Price = 10.00m },
                new UpdateProductDto { Name = "Item B", Stock = 50 },
                new UpdateProductDto { Name = "Item C", IsActive = false }
            };

            var entities = Mapper.Patch<UpdateProductDto, ProductEntity>(updates);

            foreach (var e in entities)
                Console.WriteLine($"  {e.Name}: Price={e.Price}, Stock={e.Stock}, Active={e.IsActive}");
            Console.WriteLine();
        }

        /// <summary>
        /// 8. Patch(source, destination) — in-place partial update (HTTP PATCH scenario).
        /// </summary>
        public static void PatchInPlaceExample()
        {
            Console.WriteLine("=== 8. Patch (In-Place): Patch(source, destination) ===\n");
            Mapper.CreateMap<UpdateProductDto, ProductEntity>();

            var existing = new ProductEntity
            {
                Id = 1,
                Name = "Original Widget",
                Price = 50.00m,
                Stock = 200,
                IsActive = true
            };

            var patch = new UpdateProductDto { Name = "Updated Widget", Price = 39.99m };

            Console.WriteLine($"  Before: Name={existing.Name}, Price={existing.Price}, Stock={existing.Stock}");
            Mapper.Patch(patch, existing);
            Console.WriteLine($"  After:  Name={existing.Name}, Price={existing.Price}, Stock={existing.Stock}");
            Console.WriteLine();
        }
    }
}
