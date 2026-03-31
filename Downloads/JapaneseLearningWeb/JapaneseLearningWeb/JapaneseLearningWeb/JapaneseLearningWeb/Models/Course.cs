// File: Models/Course.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseLearningWeb.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được dài quá 100 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        [StringLength(500, ErrorMessage = "Mô tả không được dài quá 500 ký tự.")]
        public string Description { get; set; } = string.Empty;

        public bool IsFree { get; set; } = true;

        // Nếu IsFree = false, Price phải >= 0
        [Range(0, 999999999, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

       
        public int CategoryId { get; set; }

        public Category? Category { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property nếu bạn có liên quan tới Lesson
        public List<Lesson> Lessons { get; set; } = new();
    }
}
