using JapaneseLearningWeb.Models;
using Newtonsoft.Json;
namespace JapaneseLearningWeb.Models
{
    public class KanjiKanjiRadical
    {
        public int KanjiId { get; set; }
        [JsonIgnore]
        public Kanji Kanji { get; set; }

        public int KanjiRadicalId { get; set; }
        public KanjiRadical KanjiRadical { get; set; }
    }
}

