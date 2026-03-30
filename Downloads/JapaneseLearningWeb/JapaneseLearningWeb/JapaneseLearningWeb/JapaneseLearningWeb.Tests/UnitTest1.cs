using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JapaneseLearningWeb.Controllers;
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace JapaneseLearningWeb.Tests.Controllers
{
    public class HomeControllerTests : IDisposable
    {
        // Khai báo các biến giả lập (Mock) và đối tượng cần test
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly AppDbContext _dbContext;
        private readonly HomeController _controller;
        private readonly Mock<ISession> _sessionMock;

        public HomeControllerTests()
        {
            // --- 1. SETUP USER MANAGER ---
            // IdentityUser cần một UserStore để hoạt động, chúng ta Mock nó trước.
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            // Khởi tạo UserManager giả lập với các tham số mặc định là null (vì ta không dùng tới trong test này).
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // --- 2. SETUP SIGNIN MANAGER ---
            // SignInManager cần HttpContextAccessor và ClaimsFactory để xử lý Cookie đăng nhập.
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null!, null!, null!, null!);

            // --- 3. SETUP DATABASE (IN-MEMORY) ---
            // Thay vì dùng SQL thật, ta dùng RAM để lưu dữ liệu tạm thời cho mỗi lần chạy test.
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Guid giúp mỗi Test Case có 1 DB riêng sạch sẽ
                .Options;
            _dbContext = new AppDbContext(options);

            // --- 4. SETUP HTTPCONTEXT & SESSION ---
            // Đây là phần quan trọng nhất: Giả lập môi trường trình duyệt (Session) cho Controller.
            _sessionMock = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _sessionMock.Object; // Gán Session giả lập vào context

            // Khởi tạo Controller thật nhưng truyền vào các "linh kiện" giả lập đã tạo ở trên.
            _controller = new HomeController(_userManagerMock.Object, _signInManagerMock.Object, _dbContext)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }

        // --- TEST CASE: ĐĂNG NHẬP THÀNH CÔNG ---
        [Fact]
        public async Task Login_ValidCredentials_RedirectsToIndex()
        {
            // Arrange: Chuẩn bị dữ liệu mẫu
            var user = new IdentityUser { Email = "hocvien@nhatngu.com", UserName = "testuser" };
            
            // "Kịch bản": Khi Controller tìm email này -> Trả về User giả
            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            
            // "Kịch bản": Khi gọi đăng nhập -> Trả về trạng thái "Succeeded"
            _signInManagerMock.Setup(m => m.PasswordSignInAsync(user.UserName!, "Pass123!", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act: Chạy hàm Login thật trong Controller
            var result = await _controller.Login(user.Email, "Pass123!");

            // Assert: Xác nhận kết quả
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName); // Phải chuyển về trang Index
        }

        // --- TEST CASE: ĐĂNG KÝ KHI EMAIL ĐÃ TỒN TẠI ---
        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsViewWithError()
        {
            // Arrange: Giả lập một email đã có trong hệ thống
            var email = "admin@nhatngu.com";
            _userManagerMock.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(new IdentityUser());

            // Act: Cố tình đăng ký trùng email
            var result = await _controller.Register(email, "newuser", "password");

            // Assert: Kiểm tra xem có văng lỗi ra màn hình không
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Email đã được sử dụng.", _controller.ViewBag.Error);
        }

        // --- TEST CASE: ĐĂNG XUẤT ---
        [Fact]
        public async Task Logout_ClearsSession_And_SignOut()
        {
            // Act: Gọi hàm Logout
            var result = await _controller.Logout();

            // Assert: Kiểm tra hành vi (Behavioral Testing)
            // Xác nhận lệnh SignOutAsync() của Identity ĐÃ ĐƯỢC GỌI đúng 1 lần.
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
            
            // Xác nhận lệnh Session.Clear() ĐÃ ĐƯỢC GỌI để xóa thông tin phiên làm việc.
            _sessionMock.Verify(s => s.Clear(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        // Hàm dọn dẹp sau khi mỗi Test Case kết thúc
        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted(); // Xóa dữ liệu trong RAM
            _dbContext.Dispose();
        }
    }
}