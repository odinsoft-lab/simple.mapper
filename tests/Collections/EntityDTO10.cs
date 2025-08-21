namespace Simple.AutoMapper.Tests.Models.DTO.Collections;

public class EntityDTO10 : BaseEntity
{
    public EntityDTO10()
    {
        this.Entities11 = new List<EntityDTO11>();
    }
    public ICollection<EntityDTO11> Entities11 { get; set; }
}
