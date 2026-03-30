using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JapaneseLearningWeb.Models
{
    public class UserCourseEnrollments
    {
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Status { get; set; } = "Enrolled"; // Enrolled, InProgress, Completed

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        //public User User { get; set; } = null!;
        public Course Course { get; set; } = null!;

        // Danh sách tiến trình bài học liên quan
        public List<UserProgress> Progresses { get; set; } = new List<UserProgress>();
    }
}