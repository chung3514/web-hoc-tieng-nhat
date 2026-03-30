namespace JapaneseLearningWeb.Models
{
    public class KanjiTestViewModel
    {
        public int LessonId { get; set; }

        public List<KanjiRadical> AllRadicals { get; set; } = new();

        public List<Kanji> LessonKanjis { get; set; } = new();

        public List<int> SelectedRadicalIds { get; set; } = new(); // Người dùng chọn

        public Kanji? MatchedKanji { get; set; } // Nếu khớp
       

    }

}
