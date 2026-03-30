using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.IO;

namespace JapaneseLearningWeb.Data
{
    public static class KanjiSeed
    {
        public static void SeedKanjiStrokes(AppDbContext context, IWebHostEnvironment env)
        {
            string kanjiDir = Path.Combine(env.ContentRootPath, "Data", "Kanji");
            if (!Directory.Exists(kanjiDir)) return;

            var jsonFiles = Directory.GetFiles(kanjiDir, "*.json");

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var data = JsonConvert.DeserializeObject<KanjiStrokeData>(json);

                    if (data != null && data.kanji?.Count > 0)
                    {
                        string character = data.kanji[0];
                        bool exists = context.KanjiStrokes.Any(k => k.Character == character);

                        if (!exists)
                        {
                            var entity = new KanjiStroke
                            {
                                Character = character,
                                StrokeDataJson = json
                            };

                            context.KanjiStrokes.Add(entity);
                        }
                    }
                }
                catch
                {
                    // Optional: log error
                }
            }

            context.SaveChanges();
        }
    }
}
