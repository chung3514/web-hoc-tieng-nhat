using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JapaneseLearningWeb.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khóa học.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bài học.")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property: Bỏ qua validation (không ép Required)
        [ValidateNever]
        public Course Course { get; set; } = null!;

        [ValidateNever]
        public Lesson Lesson { get; set; } = null!;

        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
 