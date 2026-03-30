using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JapaneseLearningWeb.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int QuizId { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property: Bỏ qua validation
        [ValidateNever]
        public Quiz Quiz { get; set; } = null!;

        public List<Option> Options { get; set; } = new List<Option>();

        // Trường tạm để đánh dấu đáp án đúng trên UI, sẽ không lưu xuống database
        [NotMapped]
        public int CorrectAnswerIndex { get; set; }
    }
}
