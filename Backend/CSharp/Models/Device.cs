namespace Models;

public class Device : Common
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<UserDevice>? UserDevices { get; set; }
    public List<DeviceData>? Data { get; set; }
}
public class DeviceCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class DeviceUpdateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}
public class DeviceDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}