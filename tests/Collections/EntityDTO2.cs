namespace Mapper.Tests.Models.DTO.Collections;

public class EntityDTO2 : BaseEntity
{
    public Guid Entity1Id { get; set; }
    public EntityDTO1 Entity1 { get; set; }
}
