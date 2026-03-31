using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JapaneseLearningWeb.Controllers
{
    public class KanjiComposeController : Controller
    {
        private readonly AppDbContext _context;

        public KanjiComposeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /KanjiCompose/Index
        public IActionResult Index()
        {
            var model = new KanjiCompositionViewModel
            {
                Lessons = _context.Lessons.ToList()
            };

            return View(model);
        }

        // POST: /KanjiCompose/Generate
        [HttpPost]
        public IActionResult Generate(KanjiCompositionViewModel model)
        {
            if (model.SelectedLessonId == 0)
            {
                TempData["Error"] = "Vui lòng chọn bài học.";
                return RedirectToAction("Index");
            }

            // Redirect tới MatchTest view để người dùng ghép chữ
            return RedirectToAction("MatchTest", new { lessonId = model.SelectedLessonId });
        }

        // GET: /KanjiCompose/MatchTest
        [HttpGet]
        public IActionResult MatchTest(int lessonId)
        {
            var kanjis = _context.Kanjis
                .Include(k => k.KanjiKanjiRadicals)
                    .ThenInclude(kkr => kkr.KanjiRadical)
                .Where(k => k.LessonId == lessonId)
                .ToList();

            var radicals = kanjis
                .SelectMany(k => k.KanjiKanjiRadicals.Select(kr => kr.KanjiRadical))
                .Distinct()
                .ToList();

            var model = new KanjiMatchTestViewModel
            {
                LessonId = lessonId,
                LessonKanjis = kanjis,
                AvailableRadicals = radicals,
                UserAnswers = new Dictionary<int, List<int>>()
            };

            return View(model);
        }

        // POST: /KanjiCompose/MatchTest
        [HttpPost]
        public IActionResult MatchTest(KanjiMatchTestViewModel model)
        {
            var results = new Dictionary<int, bool>();

            var lessonKanjis = _context.Kanjis
                .Include(k => k.KanjiKanjiRadicals)
                .ThenInclude(kkr => kkr.KanjiRadical)
                .Where(k => k.LessonId == model.LessonId)
                .ToList();

            foreach (var kanji in lessonKanjis)
            {
                if (!model.UserAnswers.ContainsKey(kanji.Id))
                {
                    results[kanji.Id] = false;
                    continue;
                }

                var correctIds = kanji.KanjiKanjiRadicals.Select(x => x.KanjiRadicalId).OrderBy(x => x).ToList();
                var userIds = model.UserAnswers[kanji.Id].OrderBy(x => x).ToList();

                results[kanji.Id] = correctIds.SequenceEqual(userIds);
            }

            model.LessonKanjis = lessonKanjis;
            model.AvailableRadicals = lessonKanjis
                .SelectMany(k => k.KanjiKanjiRadicals.Select(kr => kr.KanjiRadical))
                .Distinct()
                .ToList();
            model.Results = results;

            return View(model);
        }
    }
}
