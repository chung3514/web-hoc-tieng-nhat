using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Identity;

public class UserCourse
{
    public string UserId { get; set; }
    public int CourseId { get; set; }

    // Navigation properties
    public IdentityUser User { get; set; }
    public Course Course { get; set; }

}
