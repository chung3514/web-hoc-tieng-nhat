using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace JapaneseLearningWeb.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Grammar> Grammars { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserCourseEnrollments> UserCourseEnrollments { get; set; }
        public DbSet<Kanji> Kanjis { get; set; }
        public DbSet<VocabularyKanji> VocabularyKanjis { get; set; }
        public DbSet<KanjiStrokeData> KanjiStrokeDatas { get; set; }
        public DbSet<KanjiStroke> KanjiStrokes { get; set; }
        public DbSet<KanjiRadical> KanjiRadicals { get; set; }
        public DbSet<KanjiKanjiRadical> KanjiKanjiRadicals { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<AsrWeeklyRecord> AsrWeeklyRecords { get; set; }
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<UserVoucher> UserVouchers => Set<UserVoucher>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quiz -> Question
            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(q => q.Quiz)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question -> Option
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> Course, Lesson
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany()
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Lesson)
                .WithMany()
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Title không bắt buộc
            modelBuilder.Entity<Quiz>()
                .Property(q => q.Title)
                .IsRequired(false);

            // Vocabulary <-> Kanji (Many-to-Many)
            modelBuilder.Entity<VocabularyKanji>()
                .HasKey(vk => new { vk.VocabularyId, vk.KanjiId });

            modelBuilder.Entity<VocabularyKanji>()
                .HasOne(vk => vk.Vocabulary)
                .WithMany(v => v.VocabularyKanjis)
                .HasForeignKey(vk => vk.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocabularyKanji>()
                .HasOne(vk => vk.Kanji)
                .WithMany(k => k.VocabularyKanjis)
                .HasForeignKey(vk => vk.KanjiId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kanji <-> Radical (Many-to-Many)
            modelBuilder.Entity<KanjiKanjiRadical>()
                .HasKey(kkr => new { kkr.KanjiId, kkr.KanjiRadicalId });

            modelBuilder.Entity<KanjiKanjiRadical>()
                .HasOne(kkr => kkr.Kanji)
                .WithMany(k => k.KanjiKanjiRadicals)
                .HasForeignKey(kkr => kkr.KanjiId);

            modelBuilder.Entity<KanjiKanjiRadical>()
                .HasOne(kkr => kkr.KanjiRadical)
                .WithMany(r => r.KanjiKanjiRadicals)
                .HasForeignKey(kkr => kkr.KanjiRadicalId);

            // Kanji -> Lesson
            modelBuilder.Entity<Kanji>()
                .HasOne(k => k.Lesson)
                .WithMany(l => l.Kanjis)
                .HasForeignKey(k => k.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            //course user
            modelBuilder.Entity<UserCourse>()
        .HasKey(uc => new { uc.UserId, uc.CourseId });

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany() // hoặc .WithMany(u => u.UserCourses) nếu mở rộng IdentityUser
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany()
                .HasForeignKey(uc => uc.CourseId);
        }
    }
}