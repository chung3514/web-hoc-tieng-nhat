

using JapaneseLearningWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace JapaneseLearningWeb.Models
{
    public class Grammar
    {
        public int GrammarId { get; set; }

        public int LessonId { get; set; } // Khóa ngoại liên kết với Lesson

        [Required(ErrorMessage = "Quy tắc ngữ pháp không được để trống.")]
        [StringLength(100, ErrorMessage = "Quy tắc ngữ pháp không được dài quá 100 ký tự.")]
        public string Rule { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Ví dụ không được dài quá 200 ký tự.")]
        public string Example { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Cấp độ không được dài quá 50 ký tự.")]
        public string Level { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không được dài quá 500 ký tự.")]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Lesson? Lesson { get; set; } // Thuộc tính điều hướng
    }
}
