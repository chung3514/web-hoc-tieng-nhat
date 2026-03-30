
using JapaneseLearningWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace JapaneseLearningWeb.Models
{
    public class Vocabulary
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kanji không được để trống.")]
        [StringLength(50, ErrorMessage = "Kanji không được dài quá 50 ký tự.")]
        public string Kanji { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hiragana không được để trống.")]
        [StringLength(50, ErrorMessage = "Hiragana không được dài quá 50 ký tự.")]
        public string Hiragana { get; set; } = string.Empty;

        [Required(ErrorMessage = "Romaji không được để trống.")]
        [StringLength(50, ErrorMessage = "Romaji không được dài quá 50 ký tự.")]
        public string Romaji { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nghĩa không được để trống.")]
        [StringLength(100, ErrorMessage = "Nghĩa không được dài quá 100 ký tự.")]
        public string Meaning { get; set; } = string.Empty;

        public int LessonId { get; set; } // Khóa ngoại liên kết với Lesson
        public Lesson? Lesson { get; set; } // Thuộc tính điều hướng
        public List<VocabularyKanji> VocabularyKanjis { get; set; } = new List<VocabularyKanji>(); // Many-to-many with Kanji
    }
}
