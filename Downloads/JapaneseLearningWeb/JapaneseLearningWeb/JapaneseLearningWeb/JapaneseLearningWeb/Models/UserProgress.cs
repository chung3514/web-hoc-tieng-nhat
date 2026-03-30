using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JapaneseLearningWeb.Models
{
    public class UserProgress
    {
        public int Id { get; set; }

        [Required] public int LessonId { get; set; }

        // Lưu theo IdentityUser.Id (string GUID)
        [Required, StringLength(100)]
        public string UserId { get; set; } = string.Empty;

        public bool ReadVocab { get; set; }
        public bool ReadGrammar { get; set; }
        public bool ReadKanji { get; set; }

        // điểm cao nhất đã đạt cho quiz của bài này
        public int BestQuizScore { get; set; }

        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = "NotStarted";
        [ValidateNever]
        public Lesson Lesson { get; set; } = null!;
    }
}