using System.ComponentModel.DataAnnotations;

namespace PJATK_APBD_Cw4_s18308.DTOs;

public class PcRequest
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = null!;
    [Required]
    public double Weight { get; set; }
    [Required]
    public int Warranty { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public int Stock { get; set; }
}
