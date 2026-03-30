using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseLearningWeb.Models
{
    [NotMapped]
    public class KanjiStrokeData
    {
        public List<string> kanji { get; set; }
        public List<Stroke> strokes { get; set; }
    }
}
