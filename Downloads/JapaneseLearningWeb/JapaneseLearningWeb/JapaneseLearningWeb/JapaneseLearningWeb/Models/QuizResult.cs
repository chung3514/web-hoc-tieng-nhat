using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JapaneseLearningWeb.Models
{
    public class QuizResult
    {
        public int Id { get; set; }

        // (tuỳ bạn có dùng LessonId không, nhưng nên lưu lại để thống kê)
        public int LessonId { get; set; }

        public int Score { get; set; }
        public DateTime DateTaken { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = default!;     // FK -> AspNetUsers.Id

        [ValidateNever]
        public IdentityUser? User { get; set; }            // navigation chuẩn tới AspNetUsers
    }
}
