using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using JapaneseLearningWeb.Models.MoMo;
using JapaneseLearningWeb.Services.Momo;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<WeeklyAsrService>();

//connectMoMoAPI
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
// Add services
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<SendGridEmailSender>();
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();  // Cấu hình SendGrid hoặc SMTP

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Đăng ký HttpClient cho AIService (nếu gọi API)
builder.Services.AddHttpClient(); // Thêm HttpClient để hỗ trợ gọi API nếu cần

// Đăng ký cấu hình OpenRouter từ appsettings.json
builder.Services.Configure<OpenRouterSettings>(builder.Configuration.GetSection("OpenRouter"));
builder.Services.AddSingleton<OpenRouterQwenService>();


// Đăng ký dịch vụ gọi AI từ OpenRouter (Qwen)
builder.Services.AddSingleton<OpenRouterQwenService>();

builder.Services.AddHttpClient<GeminiService>();


var app = builder.Build();

// Seed Kanji strokes
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var env = services.GetRequiredService<IWebHostEnvironment>();
    KanjiSeed.SeedKanjiStrokes(context, env);
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
// Thêm route cho reset password với email & token trên path
app.MapControllerRoute(
    name: "resetpassword",
    pattern: "Account/ResetPassword/{email}/{token}",
    defaults: new { controller = "Account", action = "ResetPassword" }
);
// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.EnsureCreated();

    // Roles
    string[] roles = { "Admin", "Employee", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Default users
    async Task CreateUser(string username, string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new IdentityUser { UserName = username, Email = email };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }

    await CreateUser("admin", "admin@example.com", "Admin@123", "Admin");
    await CreateUser("employee", "employee@example.com", "  ", "Employee");
    await CreateUser("user", "user@example.com", "User@123", "User");

    // Seed Category
    if (!context.Categories.Any(c => c.Name == "N5"))
    {
        context.Categories.Add(new Category
        {
            Name = "N5",
            Description = "Danh mục cho các khóa học và nội dung trình độ JLPT N5",
            CreatedAt = DateTime.Now
        });
        context.SaveChanges();
    }

    // Seed Course
    var n5Category = context.Categories.First(c => c.Name == "N5");
    if (!context.Courses.Any(c => c.Title == "Khóa học Tiếng Nhật N5"))
    {
        context.Courses.Add(new Course
        {
            Title = "Khóa học Tiếng Nhật N5",
            Description = "Khóa học cơ bản dành cho người mới bắt đầu học tiếng Nhật.",
            IsFree = true,
            Price = 0,
            CreatedAt = DateTime.Now,
            CategoryId = n5Category.CategoryId
        });
        context.SaveChanges();

    }

    var n5Course = context.Courses.First(c => c.Title == "Khóa học Tiếng Nhật N5");

    // Seed Lessons
    if (!context.Lessons.Any(l => l.CourseId == n5Course.CourseId))
    {
        var now = DateTime.Now;
        var lessons = new[]
        {
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 1 – Nước, Người & Ngôn ngữ", Content = "...", OrderInCourse = 1, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 2 – Cách gọi tên người Nhật", Content = "...", OrderInCourse = 2, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 3 – Gia đình", Content = "...", OrderInCourse = 3, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 4 – Thời gian", Content = "...", OrderInCourse = 4, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 5 – Nghề nghiệp", Content = "...", OrderInCourse = 5, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 6 – Màu sắc", Content = "...", OrderInCourse = 6, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 7 – Địa điểm", Content = "...", OrderInCourse = 7, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 8 – Sở thích", Content = "...", OrderInCourse = 8, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 9 – Âm nhạc, Thể thao & Điện ảnh", Content = "...", OrderInCourse = 9, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 10 – Trong nhà", Content = "...", OrderInCourse = 10, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 11 – Số lượng", Content = "...", OrderInCourse = 11, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 12 – Lễ hội & Địa danh", Content = "...", OrderInCourse = 12, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 13 – Trong khu phố", Content = "...", OrderInCourse = 13, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 14 – Nhà ga", Content = "...", OrderInCourse = 14, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 15 – Nghề nghiệp", Content = "...", OrderInCourse = 15, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 16 – Hướng dẫn & yêu cầu", Content = "...", OrderInCourse = 16, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 17 – Sức khỏe", Content = "...", OrderInCourse = 17, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 18 – Mùa & Thiên nhiên", Content = "...", OrderInCourse = 18, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 19 – Ẩm thực", Content = "...", OrderInCourse = 19, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 20 – Tặng quà & cảm xúc", Content = "...", OrderInCourse = 20, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 21 – Chức danh", Content = "...", OrderInCourse = 21, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 22 – Quần áo", Content = "...", OrderInCourse = 22, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 23 – Giao thông & Đường xá", Content = "...", OrderInCourse = 23, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 24 – Gặp gỡ & Giao tiếp", Content = "...", OrderInCourse = 24, CreatedAt = now },
        new Lesson { CourseId = n5Course.CourseId, Title = "Bài 25 – Tổng kết & chia tay", Content = "...", OrderInCourse = 25, CreatedAt = now }
    };

        context.Lessons.AddRange(lessons);
        context.SaveChanges();
    }


    var vocabLesson = context.Lessons.First(l => l.Title == "Bài 1 – Nước, Người & Ngôn ngữ" && l.CourseId == n5Course.CourseId);

    // Seed Kanji + Radical + Link table
    if (!context.Kanjis.Any(k => k.LessonId == vocabLesson.LessonId))
    {
        var kanjisToSeed = new[]
        {
        ("人", "ジン、ニン", "ひと", "Người", "NHÂN", new[] { ("人", "NHÂN") }),
        ("学", "ガク", "まな.ぶ", "Học", "HỌC", new[] { ("子", "TỬ") }),
        ("生", "セイ", "い.きる", "Sinh", "SINH", new[] { ("生", "SINH") }),
        ("先", "セン", "さき", "Trước", "TIÊN", new[] { ("儿", "NHI") }),
        ("社", "シャ", "やしろ", "Xã", "XÃ", new[] { ("礻", "THỊ") }),
        ("医", "イ", "", "Y (Bác sĩ)", "Y", new[] { ("匚", "PHƯƠNG") }),
        ("者", "シャ", "もの", "Giả (người)", "GIẢ", new[] { ("耂", "LÃO") }),

        // ➕ 3 chữ mới với nhiều bộ thủ
        ("働", "ドウ", "はたら.く", "Làm việc", "ĐỘNG", new[] { ("人", "NHÂN"), ("生", "SINH") }),
        ("児", "ジ", "こ", "Trẻ con", "NHI", new[] { ("儿", "NHI"), ("子", "TỬ") }),
        ("存", "ソン", "", "Tồn tại", "TỒN", new[] { ("子", "TỬ"), ("人", "NHÂN") })
    };

        foreach (var (charac, on, kun, mean, hanja, radicals) in kanjisToSeed)
        {
            var kanji = new Kanji
            {
                Character = charac,
                OnYomi = on,
                KunYomi = kun,
                Meaning = mean,
                HanjaMeaning = hanja,
                LessonId = vocabLesson.LessonId
            };
            context.Kanjis.Add(kanji);
            context.SaveChanges();

            foreach (var (radicalChar, radicalMean) in radicals)
            {
                var radical = context.KanjiRadicals.FirstOrDefault(r => r.RadicalKanji == radicalChar);
                if (radical == null)
                {
                    radical = new KanjiRadical
                    {
                        RadicalKanji = radicalChar,
                        RadicalHanjaMeaning = radicalMean
                    };
                    context.KanjiRadicals.Add(radical);
                    context.SaveChanges();
                }

                context.KanjiKanjiRadicals.Add(new KanjiKanjiRadical
                {
                    KanjiId = kanji.Id,
                    KanjiRadicalId = radical.Id
                });
                context.SaveChanges();
            }
        }
    }


    // Seed Vocabularies
    if (!context.Vocabularies.Any(v => v.LessonId == vocabLesson.LessonId))
    {
        var vocabList = new[]
        {
        ("私", "わたし", "Watashi", "Tôi"),
        ("あなた", "あなた", "Anata", "Bạn / anh / chị"),
        ("あの人", "あのひと", "Ano hito", "Người kia"),
        ("～さん", "～さん", "~san", "Hậu tố lịch sự"),
        ("～ちゃん", "～ちゃん", "~chan", "Hậu tố cho trẻ em"),
        ("～じん", "～じん", "~jin", "Người nước ~"),
        ("先生", "せんせい", "Sensei", "Thầy / Cô"),
        ("教師", "きょうし", "Kyoushi", "Giáo viên"),
        ("学生", "がくせい", "Gakusei", "Học sinh / Sinh viên"),
        ("会社員", "かいしゃいん", "Kaishain", "Nhân viên công ty"),
        ("社員", "しゃいん", "Shain", "Nhân viên công ty (cụ thể)"),
        ("銀行員", "ぎんこういん", "Ginkouin", "Nhân viên ngân hàng"),
        ("医者", "いしゃ", "Isha", "Bác sĩ"),
        ("研究者", "けんきゅうしゃ", "Kenkyuusha", "Nhà nghiên cứu"),
        ("大学", "だいがく", "Daigaku", "Đại học"),
        ("病院", "びょういん", "Byouin", "Bệnh viện"),
        ("誰", "だれ（どなた）", "Dare (Donata)", "Ai / Vị nào"),
        ("～歳", "～さい", "~sai", "~ tuổi"),
        ("何歳", "なんさい", "Nansai", "Mấy tuổi"),
        ("はい", "はい", "Hai", "Vâng / Dạ"),
        ("いいえ", "いいえ", "Iie", "Không")
    };

        foreach (var (kanji, hira, roma, mean) in vocabList)
        {
            context.Vocabularies.Add(new Vocabulary
            {
                Kanji = kanji,
                Hiragana = hira,
                Romaji = roma,
                Meaning = mean,
                LessonId = vocabLesson.LessonId
            });
        }

        context.SaveChanges();
    }


    if (!context.Grammars.Any(g => g.LessonId == vocabLesson.LessonId))
    {
        context.Grammars.AddRange(
            new Grammar
            {
                LessonId = vocabLesson.LessonId,
                Rule = "Danh từ は Danh từ です",
                Example = "わたしはマイク・ミラーです。",
                Level = "N5",
                Notes = "Trợ từ は biểu thị rằng danh từ đứng trước nó là chủ đề của câu.\nNgười nói đặt は sau chủ đề để xây dựng thông tin liên quan đến chủ đề đó.",
                CreatedAt = DateTime.Now
            },
            new Grammar
            {
                LessonId = vocabLesson.LessonId,
                Rule = "Danh từ は Danh từ ではありません",
                Example = "サントスさんはがくせいではありません。",
                Level = "N5",
                Notes = "じゃ（では）ありません là thể phủ định của です.\nDùng trong hội thoại hàng ngày. ではありません là cách nói trang trọng hơn.\nVí dụ: サントスさんはがくせいではありません。",
                CreatedAt = DateTime.Now
            },
            new Grammar
            {
                LessonId = vocabLesson.LessonId,
                Rule = "Danh từ は Danh từ ですか",
                Example = "ミラーさんはアメリカじんですか。",
                Level = "N5",
                Notes = "Câu nghi vấn được tạo bằng cách thêm か vào cuối câu khẳng định.\nVí dụ: ミラーさんはアメリカじんですか？ – Vâng, anh ấy là người Mỹ.",
                CreatedAt = DateTime.Now
            },
            new Grammar
            {
                LessonId = vocabLesson.LessonId,
                Rule = "～さん",
                Example = "ミラーさん",
                Level = "N5",
                Notes = "～さん là hậu tố lịch sự thêm sau tên người.\nKhông dùng với tên của chính mình.\nVí dụ: ミラーさん là cách gọi lịch sự với anh Miller.",
                CreatedAt = DateTime.Now
            },
            new Grammar
            {
                LessonId = vocabLesson.LessonId,
                Rule = "Danh từ1 の Danh từ2",
                Example = "IMCのしゃいん",
                Level = "N5",
                Notes = "Trợ từ の nối hai danh từ và biểu thị quan hệ sở hữu.\nVí dụ: IMCのしゃいん – nhân viên của công ty IMC.",
                CreatedAt = DateTime.Now
            }
        );
        context.SaveChanges();
    }

}

app.Run();
