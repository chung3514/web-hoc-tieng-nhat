namespace JapaneseLearningWeb.Models.DTO
{
    public class SubmitQuizDto
    {
        public int LessonId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }
    }
}
