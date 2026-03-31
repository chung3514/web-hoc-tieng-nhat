using System.Collections.Generic;

public class UserProfileViewModel
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public List<string> EnrolledCourses { get; set; } = new();

}
