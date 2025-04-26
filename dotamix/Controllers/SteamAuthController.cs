using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotamix.Services;
using dotamix.Models;
using Microsoft.EntityFrameworkCore;
using dotamix.Data;
using Microsoft.Extensions.Configuration;

namespace dotamix.Controllers
{
    public class SteamAuthController : Controller
    {
        private readonly SteamService _steamService;
        private readonly ApplicationDbContext _context;
        private readonly string _steamApiKey;

        public SteamAuthController(SteamService steamService, ApplicationDbContext context, IConfiguration configuration)
        {
            _steamService = steamService;
            _context = context;
            _steamApiKey = configuration["Steam:ApiKey"] ?? string.Empty;
        }

        public IActionResult Login()
        {
            // URL для авторизации через Steam
            var returnUrl = Url.Action("Callback", "SteamAuth", null, Request.Scheme);
            var steamLoginUrl = $"https://steamcommunity.com/openid/login?" +
                $"openid.ns=http://specs.openid.net/auth/2.0&" +
                $"openid.mode=checkid_setup&" +
                $"openid.return_to={Uri.EscapeDataString(returnUrl)}&" +
                $"openid.realm={Uri.EscapeDataString(Request.Scheme + "://" + Request.Host)}&" +
                $"openid.identity=http://specs.openid.net/auth/2.0/identifier_select&" +
                $"openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select";

            return Redirect(steamLoginUrl);
        }

        public async Task<IActionResult> Callback()
        {
            // В реальном приложении здесь должна быть проверка подписи OpenID
            // Для тестового примера мы просто извлекаем Steam ID из параметров

            var steamId = Request.Query["openid.claimed_id"].ToString().Split('/').Last();

            // Получаем информацию о пользователе из Steam
            var userInfo = await _steamService.GetUserInfoAsync(steamId);
            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Не удалось получить информацию о пользователе Steam";
                return RedirectToAction("Index", "Home");
            }

            // Получаем статистику Dota 2
            var dota2Info = await _steamService.GetDota2PlayerInfoAsync(steamId);

            // Ищем пользователя в базе данных или создаем нового
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);
            if (user == null)
            {
                user = new User
                {
                    SteamId = steamId,
                    Nickname = userInfo.Nickname,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Обновляем информацию о пользователе
                user.Nickname = userInfo.Nickname;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            // В реальном приложении здесь должна быть установка аутентификационной куки
            // Для тестового примера мы просто перенаправляем на страницу профиля
            TempData["SuccessMessage"] = $"Добро пожаловать, {user.Nickname}!";
            return RedirectToAction("Profile", new { steamId = user.SteamId });
        }

        public async Task<IActionResult> Profile(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Получаем информацию о пользователе
            var userInfo = await _steamService.GetUserInfoAsync(steamId);
            if (userInfo == null)
            {
                TempData["ErrorMessage"] = "Не удалось получить информацию о пользователе Steam";
                return RedirectToAction("Index", "Home");
            }

            // Получаем статистику Dota 2
            var dota2Info = await _steamService.GetDota2PlayerInfoAsync(steamId);

            // Передаем данные в представление
            ViewBag.UserInfo = userInfo;
            ViewBag.Dota2Info = dota2Info;

            return View();
        }
    }
}