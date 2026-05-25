namespace PJATK_APBD_Cw4_s18308.Entities;

public class Component
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public int ComponentManufacturersId { get; set; }
    public virtual ComponentManufacturer ComponentManufacturer { get; set; } = null!;

    public int ComponentTypesId { get; set; }
    public virtual ComponentType ComponentType { get; set; } = null!;

    public virtual ICollection<PCComponent> PCComponents { get; set; } = new List<PCComponent>();
}
