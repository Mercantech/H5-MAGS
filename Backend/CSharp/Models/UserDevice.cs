namespace Models;

public class UserDevice : Common
{
    public string UserID { get; set; }
    public User User { get; set; }
    public string DeviceID { get; set; }
    public Device Device { get; set; }
}