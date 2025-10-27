using Simple.AutoMapper.Core;
using System;
using System.Collections.Generic;

namespace Simple.AutoMapper.Examples
{
    /// <summary>
    /// Example usage of MappingEngine
    /// </summary>
    public class MappingEngineExample
    {
        // Example Entity classes
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

        // Example DTO classes
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

        public static void BasicUsageExample()
        {
            // Configure mappings (via public facade)
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();
            Mapper.CreateMap<OrderEntity, OrderDTO>();

            // Create sample data
            var userEntity = new UserEntity
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                Address = new AddressEntity
                {
                    Street = "123 Main St",
                    City = "New York",
                    ZipCode = "10001"
                },
                Orders = new List<OrderEntity>
                {
                    new OrderEntity { OrderId = 101, OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 99.99m },
                    new OrderEntity { OrderId = 102, OrderDate = DateTime.Now.AddDays(-5), TotalAmount = 149.99m }
                }
            };

            // Map entity to DTO (public API)
            var userDto = Mapper.Map<UserEntity, UserDTO>(userEntity);

            Console.WriteLine($"Mapped User: {userDto.FirstName} {userDto.LastName}");
            Console.WriteLine($"Address: {userDto.Address.Street}, {userDto.Address.City}");
            Console.WriteLine($"Orders Count: {userDto.Orders.Count}");
        }

        public static void AdvancedConfigurationExample()
        {
            // Configure mapping with ignored properties via facade
            Mapper.CreateMap<UserEntity, UserDTO>()
                .Ignore(dest => dest.BirthDate); // Ignore BirthDate property

            // Configure nested mappings
            Mapper.CreateMap<AddressEntity, AddressDTO>();
            Mapper.CreateMap<OrderEntity, OrderDTO>();

            var userEntity = new UserEntity
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                BirthDate = new DateTime(1985, 5, 15),
                Address = new AddressEntity
                {
                    Street = "456 Oak Ave",
                    City = "Los Angeles",
                    ZipCode = "90001"
                }
            };

            var userDto = Mapper.Map<UserEntity, UserDTO>(userEntity);
            
            // BirthDate will be default value since it was ignored
            Console.WriteLine($"BirthDate (ignored): {userDto.BirthDate}");
        }

        public static void CollectionMappingExample()
        {
            // Configure mappings
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();
            Mapper.CreateMap<OrderEntity, OrderDTO>();

            // Create a list of entities
            var userEntities = new List<UserEntity>
            {
                new UserEntity 
                { 
                    Id = 1, 
                    FirstName = "Alice", 
                    LastName = "Johnson",
                    BirthDate = DateTime.Now.AddYears(-30)
                },
                new UserEntity 
                { 
                    Id = 2, 
                    FirstName = "Bob", 
                    LastName = "Wilson",
                    BirthDate = DateTime.Now.AddYears(-25)
                },
                new UserEntity 
                { 
                    Id = 3, 
                    FirstName = "Charlie", 
                    LastName = "Brown",
                    BirthDate = DateTime.Now.AddYears(-35)
                }
            };

            // Map the entire list
            var userDtos = Mapper.Map<UserEntity, UserDTO>(userEntities);

            Console.WriteLine($"Mapped {userDtos.Count} users");
            foreach (var dto in userDtos)
            {
                Console.WriteLine($"- {dto.FirstName} {dto.LastName}");
            }
        }

        public static void PerformanceBenefitsExample()
        {
            // Configure mappings once
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();
            Mapper.CreateMap<OrderEntity, OrderDTO>();

            // Create many entities
            var entities = new List<UserEntity>();
            for (int i = 0; i < 10000; i++)
            {
                entities.Add(new UserEntity
                {
                    Id = i,
                    FirstName = $"User{i}",
                    LastName = $"LastName{i}",
                    BirthDate = DateTime.Now.AddYears(-20 - (i % 50)),
                    Address = new AddressEntity
                    {
                        Street = $"{i} Main St",
                        City = "City",
                        ZipCode = "12345"
                    }
                });
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            // First mapping - will compile and cache the mapping expression
            var firstDto = Mapper.Map<UserEntity, UserDTO>(entities[0]);
            sw.Stop();
            Console.WriteLine($"First mapping (with compilation): {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            // Subsequent mappings use the cached compiled expression - much faster!
            var allDtos = Mapper.Map<UserEntity, UserDTO>(entities);
            sw.Stop();
            Console.WriteLine($"Mapping {entities.Count} entities (using cache): {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average time per entity: {sw.ElapsedMilliseconds / (double)entities.Count:F4}ms");
        }

        /// <summary>
        /// Demonstrates in-place updates: copy values from source into an existing destination instance
        /// </summary>
        public static void InPlaceUpdateExample()
        {
            // Configure the mappings used for nested objects
            Mapper.CreateMap<UserEntity, UserDTO>();
            Mapper.CreateMap<AddressEntity, AddressDTO>();

            // Existing destination instance (e.g., tracked entity or cached DTO)
            var existingDto = new UserDTO
            {
                Id = 1,
                FirstName = "Old",
                LastName = "Name",
                BirthDate = new DateTime(1980, 1, 1),
                Address = new AddressDTO
                {
                    Street = "Old St",
                    City = "OldCity",
                    ZipCode = "00000"
                }
            };

            // Source with updated values
            var source = new UserEntity
            {
                Id = 1,
                FirstName = "John",
                LastName = "Updated",
                BirthDate = new DateTime(1990, 1, 1),
                Address = new AddressEntity
                {
                    Street = "123 Main St",
                    City = "New York",
                    ZipCode = "10001"
                }
            };

            Console.WriteLine("Before in-place update:");
            Console.WriteLine($"  Name: {existingDto.FirstName} {existingDto.LastName}");
            Console.WriteLine($"  Address: {existingDto.Address?.Street}, {existingDto.Address?.City} {existingDto.Address?.ZipCode}");

            // Perform in-place update
            Mapper.Map(source, existingDto);

            Console.WriteLine("After in-place update:");
            Console.WriteLine($"  Name: {existingDto.FirstName} {existingDto.LastName}");
            Console.WriteLine($"  Address: {existingDto.Address?.Street}, {existingDto.Address?.City} {existingDto.Address?.ZipCode}");
        }
    }
}