namespace MyApp.Api.Models;

public sealed class Device
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
