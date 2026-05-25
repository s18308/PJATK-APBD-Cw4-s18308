namespace PJATK_APBD_Cw4_s18308.Entities;

public class ComponentManufacturer
{
    public int Id { get; set; }
    public string Abbreviation { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateOnly FoundationDate { get; set; }

    public virtual ICollection<Component> Components { get; set; } = new List<Component>();
}
