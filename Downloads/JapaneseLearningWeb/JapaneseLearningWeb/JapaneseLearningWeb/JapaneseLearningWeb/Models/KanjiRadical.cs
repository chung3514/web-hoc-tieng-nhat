using JapaneseLearningWeb.Models;
using System.ComponentModel.DataAnnotations;

//public class KanjiRadical
//{
//    public int Id { get; set; }

//    [Required]
//    public string RadicalKanji { get; set; } = string.Empty;

//    [Required]
//    public string RadicalHanjaMeaning { get; set; } = string.Empty;

//    public int KanjiId { get; set; }
//    public Kanji? Kanji { get; set; }
//}
using Newtonsoft.Json;
namespace JapaneseLearningWeb.Models
{
    public class KanjiRadical
    {
        public int Id { get; set; }

        [Required]
        public string RadicalKanji { get; set; } = string.Empty;

        [Required]
        public string RadicalHanjaMeaning { get; set; } = string.Empty;
        [JsonIgnore]
        public List<KanjiKanjiRadical> KanjiKanjiRadicals { get; set; } = new();
    }


}

