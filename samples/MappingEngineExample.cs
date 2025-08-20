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
            // Step 1: Create and configure the mapping engine
            var engine = new MappingEngine();
            
            // Step 2: Create mappings (this is done once, typically at application startup)
            engine.CreateMap<UserEntity, UserDTO>();
            engine.CreateMap<AddressEntity, AddressDTO>();
            engine.CreateMap<OrderEntity, OrderDTO>();

            // Step 3: Create sample data
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

            // Step 4: Map entity to DTO
            var userDto = engine.Map<UserEntity, UserDTO>(userEntity);

            // The userDto now contains all mapped data including nested objects and collections
            Console.WriteLine($"Mapped User: {userDto.FirstName} {userDto.LastName}");
            Console.WriteLine($"Address: {userDto.Address.Street}, {userDto.Address.City}");
            Console.WriteLine($"Orders Count: {userDto.Orders.Count}");
        }

        public static void AdvancedConfigurationExample()
        {
            var engine = new MappingEngine();

            // Configure mapping with ignored properties
            engine.CreateMap<UserEntity, UserDTO>()
                .Ignore(dest => dest.BirthDate); // Ignore BirthDate property

            // Configure nested mappings
            engine.CreateMap<AddressEntity, AddressDTO>();
            engine.CreateMap<OrderEntity, OrderDTO>();

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

            var userDto = engine.Map<UserEntity, UserDTO>(userEntity);
            
            // BirthDate will be default value since it was ignored
            Console.WriteLine($"BirthDate (ignored): {userDto.BirthDate}");
        }

        public static void CollectionMappingExample()
        {
            var engine = new MappingEngine();
            
            // Configure mappings
            engine.CreateMap<UserEntity, UserDTO>();
            engine.CreateMap<AddressEntity, AddressDTO>();
            engine.CreateMap<OrderEntity, OrderDTO>();

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
            var userDtos = engine.MapList<UserEntity, UserDTO>(userEntities);

            Console.WriteLine($"Mapped {userDtos.Count} users");
            foreach (var dto in userDtos)
            {
                Console.WriteLine($"- {dto.FirstName} {dto.LastName}");
            }
        }

        public static void PerformanceBenefitsExample()
        {
            var engine = new MappingEngine();
            
            // Configure mappings once
            engine.CreateMap<UserEntity, UserDTO>();
            engine.CreateMap<AddressEntity, AddressDTO>();
            engine.CreateMap<OrderEntity, OrderDTO>();

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
            var firstDto = engine.Map<UserEntity, UserDTO>(entities[0]);
            sw.Stop();
            Console.WriteLine($"First mapping (with compilation): {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            // Subsequent mappings use the cached compiled expression - much faster!
            var allDtos = engine.MapList<UserEntity, UserDTO>(entities);
            sw.Stop();
            Console.WriteLine($"Mapping {entities.Count} entities (using cache): {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average time per entity: {sw.ElapsedMilliseconds / (double)entities.Count:F4}ms");
        }
    }
}