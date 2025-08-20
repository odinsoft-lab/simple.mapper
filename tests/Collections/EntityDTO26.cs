namespace Mapper.Tests.Models.DTO.Collections;

public class EntityDTO26 : BaseEntity
{
    public EntityDTO26()
    {
        this.Entities20 = new List<EntityDTO20>();
    }

    public ICollection<EntityDTO20> Entities20 { get; set; }
}
