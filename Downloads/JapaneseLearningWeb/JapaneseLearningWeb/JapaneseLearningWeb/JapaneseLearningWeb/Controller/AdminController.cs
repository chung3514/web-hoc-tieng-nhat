using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Hiển thị danh sách user + role hiện tại
    public async Task<IActionResult> UserManagement()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            });
        }

        return View(model);
    }

    // POST: cập nhật role cho user
    [HttpPost]
    public async Task<IActionResult> UpdateUserRole(string userId, string role)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        // Gỡ hết role cũ
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Thêm role mới
        if (await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        else
        {
            return BadRequest("Role không tồn tại");
        }

        return RedirectToAction("UserManagement");
    }
    // GET: Hiển thị form thêm người dùng
    public IActionResult CreateUser()
    {
        return View();
    }

    // POST: Thêm người dùng mới
    [HttpPost]
    public async Task<IActionResult> CreateUser(string username, string email, string password, string role)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
        {
            return BadRequest();
        }

        var user = new IdentityUser { UserName = username, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            else
            {
                return BadRequest("Role không tồn tại");
            }

            return RedirectToAction("UserManagement");
        }

        return View();
    }
    // POST: Xóa người dùng
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("UserManagement");
        }

        return BadRequest("Không thể xóa người dùng");
    }
    // POST: Cập nhật thông tin người dùng
    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join("<br>", result.Errors.Select(e => e.Description));
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            // Gỡ hết role cũ
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Thêm role mới
            if (await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            else
            {
                ViewBag.Error = "Role không tồn tại";
                return View(model);
            }

            return RedirectToAction("UserManagement");
        }
        return View(model);
    }
    // GET: Sửa thông tin người dùng
    public async Task<IActionResult> EditUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var model = new EditUserViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles
        };

        return View(model);
    }


}
