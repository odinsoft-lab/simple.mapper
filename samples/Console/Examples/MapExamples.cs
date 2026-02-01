using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;

namespace Simple.AutoMapper.Examples
{
    /// <summary>
    /// Map API — 4 overloads for full property copy.
    /// </summary>
    public static class MapExamples
    {
        /// <summary>
        /// 1. Map&lt;TSource, TDestination&gt;(source) — create new object.
        /// </summary>
        public static void BasicMapExample()
        {
            Console.WriteLine("=== 1. Basic Map: Map<TSource, TDestination>(source) ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();
            Mapper.CreateMap<OrderEntity, OrderDTO>();

            var user = new UserEntity
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                Address = new AddressEntity { Street = "123 Main St", City = "New York", ZipCode = "10001" },
                Orders = new List<OrderEntity>
                {
                    new OrderEntity { OrderId = 101, OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 99.99m },
                    new OrderEntity { OrderId = 102, OrderDate = DateTime.Now.AddDays(-5), TotalAmount = 149.99m }
                }
            };

            var dto = Mapper.Map<UserEntity, UserDTO>(user);

            Console.WriteLine($"  User: {dto.FirstName} {dto.LastName}");
            Console.WriteLine($"  Address: {dto.Address.Street}, {dto.Address.City}");
            Console.WriteLine($"  Orders: {dto.Orders.Count}");
            Console.WriteLine();
        }

        /// <summary>
        /// 2. Map&lt;TDestination&gt;(object) — source type inferred via reflection.
        /// </summary>
        public static void TypeInferredMapExample()
        {
            Console.WriteLine("=== 2. Type-Inferred Map: Map<TDestination>(source) ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();

            var user = new UserEntity { Id = 2, FirstName = "Jane", LastName = "Smith" };

            var dto = Mapper.Map<UserDTO>(user);

            Console.WriteLine($"  User: {dto.FirstName} {dto.LastName}");
            Console.WriteLine();
        }

        /// <summary>
        /// 3. Map&lt;TSource, TDestination&gt;(IEnumerable) — map a collection.
        /// </summary>
        public static void CollectionMapExample()
        {
            Console.WriteLine("=== 3. Collection Map: Map<TSource, TDestination>(list) ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>();

            var users = new List<UserEntity>
            {
                new UserEntity { Id = 1, FirstName = "Alice", LastName = "Johnson" },
                new UserEntity { Id = 2, FirstName = "Bob", LastName = "Wilson" },
                new UserEntity { Id = 3, FirstName = "Charlie", LastName = "Brown" }
            };

            var dtos = Mapper.Map<UserEntity, UserDTO>(users);

            Console.WriteLine($"  Mapped {dtos.Count} users:");
            foreach (var d in dtos)
                Console.WriteLine($"    - {d.FirstName} {d.LastName}");
            Console.WriteLine();
        }

        /// <summary>
        /// 4. Map(source, destination) — in-place update of existing object.
        /// </summary>
        public static void InPlaceMapExample()
        {
            Console.WriteLine("=== 4. In-Place Map: Map(source, destination) ===\n");
            Mapper.CreateMap<UserEntity, UserDTO>();

            var existingDto = new UserDTO { Id = 1, FirstName = "Old", LastName = "Name" };
            var source = new UserEntity { Id = 1, FirstName = "John", LastName = "Updated" };

            Console.WriteLine($"  Before: {existingDto.FirstName} {existingDto.LastName}");
            Mapper.Map(source, existingDto);
            Console.WriteLine($"  After:  {existingDto.FirstName} {existingDto.LastName}");
            Console.WriteLine();
        }
    }
}
