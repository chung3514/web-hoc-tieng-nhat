namespace JapaneseLearningWeb.Models
{
    public class VocabularyKanji
    {
        public int VocabularyId { get; set; }
        public Vocabulary? Vocabulary { get; set; }

        public int KanjiId { get; set; }
        public Kanji? Kanji { get; set; }
    }
}