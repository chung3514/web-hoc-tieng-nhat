using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using JapaneseLearningWeb.Models; // cần thêm dòng này
using JapaneseLearningWeb.Data;



namespace JapaneseLearningWeb.Controllers
{
    public class KanjiStrokeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        public KanjiStrokeController(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpPost]
        public JsonResult CheckKanji([FromBody] StrokeInput input)
        {
            var allKanji = _context.KanjiStrokes.ToList();

            var matched = allKanji.FirstOrDefault(k =>
            {
                var strokeData = JsonConvert.DeserializeObject<KanjiStrokeData>(k.StrokeDataJson);
                return strokeData.strokes.Count == input.Strokes.Count;
            });

            if (matched != null)
                return Json(new { success = true, message = $"Có thể là chữ: {matched.Character}" });

            return Json(new { success = false, message = "Không tìm thấy chữ phù hợp." });
        }

        public JsonResult GetStrokeData(string character)
        {
            var kanji = _context.KanjiStrokes.Find(character);

            if (kanji == null)
            {
                return Json(new { success = false, message = "Kanji not found in database." });
            }

            var strokeData = JsonConvert.DeserializeObject<KanjiStrokeData>(kanji.StrokeDataJson);
            return Json(new { success = true, data = strokeData });
        }

    }



}
