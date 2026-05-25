namespace PJATK_APBD_Cw4_s18308.Entities;

public class PC
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Weight { get; set; }
    public int Warranty { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Stock { get; set; }

    public virtual ICollection<PCComponent> PCComponents { get; set; } = new List<PCComponent>();
}
