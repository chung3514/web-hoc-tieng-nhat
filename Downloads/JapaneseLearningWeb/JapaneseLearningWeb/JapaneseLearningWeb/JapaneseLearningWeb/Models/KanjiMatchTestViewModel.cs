namespace JapaneseLearningWeb.Models
{
    public class KanjiMatchTestViewModel
    {
        public int LessonId { get; set; }


        public List<Kanji> LessonKanjis { get; set; } = new();


        public List<KanjiRadical> AvailableRadicals { get; set; } = new();

        // Dictionary<kanjiId, List<radicalId>> selected by user
        public Dictionary<int, List<int>> UserAnswers { get; set; } = new();

        // Kết quả sau khi kiểm tra
        public Dictionary<int, bool> Results { get; set; } = new();
        public bool IsSubmitted { get; set; }

    }

}
