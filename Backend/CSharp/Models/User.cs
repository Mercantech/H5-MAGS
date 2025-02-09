namespace Models;

public class User : Common
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public Status Status { get; set; }
    public List<UserDevice> UserDevices { get; set; }
}

public enum Role
{
    Admin,
    User
}

public enum Status
{
    Active,
    Inactive
}

public class UserCreateDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public Status Status { get; set; }
}
public class UserUpdateDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public Status Status { get; set; }
}
public class UserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public Status Status { get; set; }
}