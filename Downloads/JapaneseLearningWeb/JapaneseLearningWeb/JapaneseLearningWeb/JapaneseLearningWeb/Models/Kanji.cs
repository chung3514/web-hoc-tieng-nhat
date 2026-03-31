

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace JapaneseLearningWeb.Models
{
    public class Kanji
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kanji không được để trống.")]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "Kanji phải là một ký tự.")]
        public string Character { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Âm On không được dài quá 100 ký tự.")]
        public string OnYomi { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Âm Kun không được dài quá 100 ký tự.")]
        public string KunYomi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nghĩa không được để trống.")]
        [StringLength(200, ErrorMessage = "Nghĩa không được dài quá 200 ký tự.")]
        public string Meaning { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Nghĩa Hán Việt không được dài quá 100 ký tự.")]
        public string HanjaMeaning { get; set; } = string.Empty;

        

        [Range(1, int.MaxValue, ErrorMessage = "Bài học không hợp lệ.")]
        public int LessonId { get; set; }

        // Navigation properties
        public Lesson? Lesson { get; set; }
        public List<Vocabulary> Vocabularies { get; set; } = new List<Vocabulary>();
        public List<VocabularyKanji> VocabularyKanjis { get; set; } = new List<VocabularyKanji>();
        public List<KanjiKanjiRadical> KanjiKanjiRadicals { get; set; } = new();


    }
}
