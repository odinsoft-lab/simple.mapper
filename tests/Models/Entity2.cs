namespace Mapper.Tests.Models;

public class Entity2 : BaseEntity
{
    public Guid Entity1Id { get; set; }
    public Entity1 Entity1 { get; set; }
}
