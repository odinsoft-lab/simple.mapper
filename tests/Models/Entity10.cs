namespace Simple.AutoMapper.Tests.Models;

public class Entity10 : BaseEntity
{
    public Entity10()
    {
        // Don't create circular reference in constructor
        // this.Entities11 = new Entity11();
    }
    public Entity11 Entities11 { get; set; }
}
