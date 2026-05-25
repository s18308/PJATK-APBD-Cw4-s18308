namespace PJATK_APBD_Cw4_s18308.DTOs;

public class PcWithComponentsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Weight { get; set; }
    public int Warranty { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Stock { get; set; }
    public List<PcComponentResponse> Components { get; set; } = new();
}

public class PcComponentResponse
{
    public int Amount { get; set; }
    public ComponentResponse Component { get; set; } = null!;
}

public class ComponentResponse
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ManufacturerResponse Manufacturer { get; set; } = null!;
    public ComponentTypeResponse Type { get; set; } = null!;
}

public class ManufacturerResponse
{
    public int Id { get; set; }
    public string Abbreviation { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateOnly FoundationDate { get; set; }
}

public class ComponentTypeResponse
{
    public int Id { get; set; }
    public string Abbreviation { get; set; } = null!;
    public string Name { get; set; } = null!;
}
