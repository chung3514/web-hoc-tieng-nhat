using System.ComponentModel.DataAnnotations;

namespace JapaneseLearningWeb.Models
{
    public class KanjiStroke
    {
        [Key]
        public string Character { get; set; }

        [Required]
        public string StrokeDataJson { get; set; }
    }
}
