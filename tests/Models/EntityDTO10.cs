namespace Simple.AutoMapper.Tests.Models.DTO;

public class EntityDTO10 : BaseEntity
{
    public EntityDTO10()
    {
        // Don't create circular reference in constructor
        // this.Entities11 = new EntityDTO11();
    }
    public EntityDTO11 Entities11 { get; set; }
}
