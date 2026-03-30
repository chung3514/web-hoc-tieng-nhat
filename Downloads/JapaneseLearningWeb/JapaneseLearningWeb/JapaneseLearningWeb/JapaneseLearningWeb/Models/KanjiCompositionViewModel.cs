namespace JapaneseLearningWeb.Models
{
    public class KanjiCompositionViewModel
    {
        public int SelectedLessonId { get; set; }

        public List<Lesson> Lessons { get; set; } = new();

        public int RadicalCount { get; set; }

        public List<Kanji> GeneratedKanjis { get; set; } = new();

        public List<KanjiRadical> UsedRadicals { get; set; } = new();
        public List<KanjiRadical> AvailableRadicals { get; set; } = new();
        public List<int> SelectedRadicalIds { get; set; } = new();

    }
}
