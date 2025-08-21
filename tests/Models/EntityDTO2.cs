namespace Simple.AutoMapper.Tests.Models.DTO;

public class EntityDTO2 : BaseEntity
{
    public Guid Entity1Id { get; set; }
    public EntityDTO1 Entity1 { get; set; }
}
