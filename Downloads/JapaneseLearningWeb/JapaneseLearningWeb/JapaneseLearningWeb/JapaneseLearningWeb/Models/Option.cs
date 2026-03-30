using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JapaneseLearningWeb.Models
{
    public class Option
    {
        public int OptionId { get; set; }

        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Nội dung đáp án không được để trống.")]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        // Navigation property: Bỏ qua validation
        [ValidateNever]
        public Question Question { get; set; } = null!;
    }
}
