namespace PJATK_APBD_Cw4_s18308.Entities;

public class PCComponent
{
    public int PCId { get; set; }
    public virtual PC PC { get; set; } = null!;

    public string ComponentCode { get; set; } = null!;
    public virtual Component Component { get; set; } = null!;

    public int Amount { get; set; }
}
