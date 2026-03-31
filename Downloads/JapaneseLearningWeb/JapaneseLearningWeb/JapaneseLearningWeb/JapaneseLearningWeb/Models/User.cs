namespace JapaneseLearningWeb.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; } // Lưu ý: Trong thực tế, cần mã hóa mật khẩu
    }
}