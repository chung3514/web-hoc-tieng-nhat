

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace JapaneseLearningWeb.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một khóa học.")]
        [Range(1, int.MaxValue, ErrorMessage = "Khóa học không hợp lệ.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được dài quá 100 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        public string Content { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Thứ tự phải lớn hơn hoặc bằng 1.")]
        public int OrderInCourse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Course? Course { get; set; }

        // Thêm danh sách Vocabulary và Grammar
        public List<Vocabulary> Vocabularies { get; set; } = new List<Vocabulary>();
        public List<Grammar> Grammars { get; set; } = new List<Grammar>();
        public List<Kanji> Kanjis { get; set; } = new List<Kanji>();
        public List<KanjiRadical> Radicals { get; set; } = new List<KanjiRadical>();
        public List<KanjiKanjiRadical> KanjiKanjiRadicals { get; set; } = new();
        // Thêm các trường để lưu link YouTube
        [StringLength(200, ErrorMessage = "Link YouTube cho từ vựng không được dài quá 200 ký tự.")]
        public string VocabYouTubeLink { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Link YouTube cho ngữ pháp không được dài quá 200 ký tự.")]
        public string GrammarYouTubeLink { get; set; } = string.Empty;
    }

    
}