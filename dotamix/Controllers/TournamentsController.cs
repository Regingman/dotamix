using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotamix.Data;
using dotamix.Models;
using dotamix.Services;
using OfficeOpenXml;
using System.ComponentModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace dotamix.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleFormsService _googleFormsService;

        public TournamentsController(ApplicationDbContext context, GoogleFormsService googleFormsService)
        {
            _context = context;
            _googleFormsService = googleFormsService;
        }

        // GET: Tournaments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tournaments
                .Include(t => t.Participants)
                .ToListAsync());
        }

        // GET: Tournaments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }

        // GET: Tournaments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tournaments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            _context.Add(tournament);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Tournaments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }

        // POST: Tournaments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartDate,EndDate,Status")] Tournament tournament)
        {
            if (id != tournament.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tournament);
        }

        // GET: Tournaments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        // POST: Tournaments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int tournamentId, int userId, bool isPaid)
        {
            var participant = await _context.TournamentParticipants
                .FirstOrDefaultAsync(p => p.TournamentId == tournamentId && p.UserId == userId);

            if (participant == null)
            {
                return NotFound();
            }

            participant.IsPaid = isPaid;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PaymentStatus), new { id = tournamentId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatusAjax(int participantId, bool isPaid)
        {
            var participant = await _context.TournamentParticipants
                .FirstOrDefaultAsync(p => p.Id == participantId);

            if (participant == null)
            {
                return Json(new { success = false, message = "Участник не найден" });
            }

            participant.IsPaid = isPaid;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCaptainStatusAjax(int participantId, bool isCaptain)
        {
            var participant = await _context.TournamentParticipants
                .Include(e => e.User)
                .Include(e => e.Tournament)
                .FirstOrDefaultAsync(p => p.Id == participantId);

            if (participant == null)
            {
                return Json(new { success = false, message = "Участник не найден" });
            }

            participant.IsCaptain = isCaptain;
            await _context.SaveChangesAsync();

            if (isCaptain == true)
            {
                if (!participant.IsCaptain)
                {
                    TempData["ErrorMessage"] = "Выбранный капитан недоступен";
                    return RedirectToAction(nameof(FormTeams), new { id = participant.TournamentId });
                }

                var team = new Team
                {
                    Name = "Team " + participant.User.Nickname,
                    TournamentId = participant.TournamentId,
                    CaptainId = participant.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                // Добавляем капитана в команду
                participant.TeamId = team.Id;
                await _context.SaveChangesAsync();
            }
            else
            {
                var players = await _context.TournamentParticipants.Where(e => e.TeamId == participant.TeamId && e.Id != participant.Id).ToListAsync();
                participant.TeamId = null;
                foreach (var temp in players)
                {
                    temp.TeamId = null;
                }
                await _context.SaveChangesAsync();

                var team = await _context.Teams.FirstOrDefaultAsync(e => e.CaptainId == participant.UserId);
                if (team != null)
                {
                    _context.Teams.Remove(team);
                    await _context.SaveChangesAsync();
                }

            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> PaymentStatus(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        [HttpGet]
        public IActionResult ImportParticipants(int id)
        {
            ViewBag.TournamentId = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportParticipants(int id, IFormFile excelFile)
        {
            try
            {
                if (excelFile == null || excelFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Пожалуйста, выберите файл для импорта.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var listPlayers = await _context.TournamentParticipants.Where(e => e.TournamentId == id).ToListAsync();
                _context.TournamentParticipants.RemoveRange(listPlayers);
                await _context.SaveChangesAsync();

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                            var nickname = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            var mmr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                            var positions = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                            var phoneNumber = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(nickname))
                                continue;

                            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Nickname == nickname);

                            User user;
                            if (existingUser == null)
                            {
                                user = new User
                                {
                                    Name = name,
                                    Nickname = nickname
                                };
                                _context.Users.Add(user);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                user = existingUser;
                            }

                            var existingParticipant = await _context.TournamentParticipants
                                .FirstOrDefaultAsync(p => p.TournamentId == id && p.UserId == user.Id);

                            if (existingParticipant == null)
                            {
                                var participant = new TournamentParticipant
                                {
                                    TournamentId = id,
                                    UserId = user.Id,
                                    MMR = int.TryParse(mmr, out int mmrValue) ? mmrValue : 0,
                                    IsPaid = false,
                                    CreatedAt = DateTime.UtcNow,
                                    PhoneNumber = phoneNumber
                                };

                                // Обработка позиций
                                if (!string.IsNullOrEmpty(positions))
                                {
                                    var positionsList = positions.Split(',')
                                        .Select(p => p.Trim())
                                        .Where(p => int.TryParse(p, out int pos) && pos >= 1 && pos <= 5)
                                        .Select(p => int.Parse(p))
                                        .Distinct()
                                        .ToList();
                                    participant.SetPositions(positionsList);
                                }

                                _context.TournamentParticipants.Add(participant);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Участники успешно импортированы из Excel.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при импорте участников: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SelectCaptains(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            // Получаем только оплаченных участников, которые еще не являются капитанами
            var potentialCaptains = tournament.Participants
                .Where(p => !p.IsCaptain)
                .ToList();

            ViewBag.PotentialCaptains = potentialCaptains;
            return View(tournament);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCaptain(int tournamentId, int participantId)
        {
            var participant = await _context.TournamentParticipants
                .FirstOrDefaultAsync(p => p.Id == participantId && p.TournamentId == tournamentId);

            if (participant == null)
            {
                return NotFound();
            }

            participant.IsCaptain = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SelectCaptains), new { id = tournamentId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCaptain(int tournamentId, int participantId)
        {
            var participant = await _context.TournamentParticipants
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == participantId && p.TournamentId == tournamentId);

            if (participant == null)
            {
                return NotFound();
            }

            // Проверяем, не является ли участник капитаном команды
            if (participant.Team != null && participant.Team.CaptainId == participant.UserId)
            {
                TempData["ErrorMessage"] = "Нельзя удалить капитана активной команды";
                return RedirectToAction(nameof(SelectCaptains), new { id = tournamentId });
            }

            participant.IsCaptain = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SelectCaptains), new { id = tournamentId });
        }

        [HttpGet]
        public async Task<IActionResult> FormTeams(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Captain)
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Players)
                        .ThenInclude(p => p.User)
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            // Получаем список доступных игроков (оплатившие участие, но не в команде)
            ViewBag.AvailablePlayers = tournament.Participants
                .Where(p => !p.TeamId.HasValue)
                .ToList();

            // Получаем список доступных капитанов (оплатившие участие и имеющие статус капитана)
            ViewBag.AvailableCaptains = tournament.Participants
                .Where(p => p.IsCaptain && !p.TeamId.HasValue)
                .ToList();

            return View(tournament);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam(int tournamentId, string teamName, int captainId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null)
            {
                return NotFound();
            }

            var captain = await _context.TournamentParticipants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == captainId && p.TournamentId == tournamentId);

            if (captain == null || !captain.IsCaptain)
            {
                TempData["ErrorMessage"] = "Выбранный капитан недоступен";
                return RedirectToAction(nameof(FormTeams), new { id = tournamentId });
            }

            var team = new Team
            {
                Name = teamName,
                TournamentId = tournamentId,
                CaptainId = captain.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Добавляем капитана в команду
            captain.TeamId = team.Id;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(FormTeams), new { id = tournamentId });
        }

        public class AddPlayerToTeamModel
        {
            public int TeamId { get; set; }
            public int UserId { get; set; }
            public int TournamentId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayerToTeam(int playerId, int teamId, int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return Json(new { success = false, message = "Турнир не найден" });
            }

            var team = tournament.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return Json(new { success = false, message = "Команда не найдена" });
            }

            var player = await _context.TournamentParticipants
                .FirstOrDefaultAsync(p => p.Id == playerId && p.TournamentId == tournamentId);

            if (player == null)
            {
                return Json(new { success = false, message = "Игрок не найден" });
            }

            // Проверяем, не состоит ли игрок уже в команде
            if (player.TeamId.HasValue)
            {
                return Json(new { success = false, message = "Игрок уже состоит в команде" });
            }

            // Проверяем количество игроков в команде
            var teamPlayers = await _context.TournamentParticipants
                .CountAsync(p => p.TeamId == teamId);

            if (teamPlayers >= 5)
            {
                return Json(new { success = false, message = "В команде уже максимальное количество игроков" });
            }

            // Добавляем игрока в команду
            player.TeamId = teamId;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemovePlayerFromTeam(int teamId, int playerId, int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return Json(new { success = false, message = "Турнир не найден" });
            }

            var team = tournament.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return Json(new { success = false, message = "Команда не найдена" });
            }

            var player = await _context.TournamentParticipants
                .FirstOrDefaultAsync(p => p.Id == playerId && p.TournamentId == tournamentId);

            if (player == null)
            {
                return Json(new { success = false, message = "Игрок не найден" });
            }

            // Проверяем, что игрок действительно в этой команде
            if (player.TeamId != teamId)
            {
                return Json(new { success = false, message = "Игрок не состоит в этой команде" });
            }

            // Удаляем игрока из команды
            player.TeamId = null;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTeamOrder(int teamId, int newIndex, string teamOrder, int tournamentId)
        {
            try
            {
                var tournament = await _context.Tournaments
                    .Include(t => t.Teams)
                    .FirstOrDefaultAsync(t => t.Id == tournamentId);

                if (tournament == null)
                    return Json(new { success = false, message = "Турнир не найден" });

                var team = tournament.Teams.FirstOrDefault(t => t.Id == teamId);
                if (team == null)
                    return Json(new { success = false, message = "Команда не найдена" });

                // Парсим порядок команд из JSON
                var orderArray = JsonSerializer.Deserialize<int[]>(teamOrder);

                // Обновляем порядок всех команд
                for (int i = 0; i < orderArray.Length; i++)
                {
                    var currentTeam = tournament.Teams.FirstOrDefault(t => t.Id == orderArray[i]);
                    if (currentTeam != null)
                    {
                        currentTeam.Order = i;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Произошла ошибка при обновлении порядка команд" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeam(int teamId, int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return Json(new { success = false, message = "Турнир не найден" });
            }

            var team = tournament.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return Json(new { success = false, message = "Команда не найдена" });
            }

            // Освобождаем игроков из команды
            var players = await _context.TournamentParticipants
                .Where(p => p.TeamId == teamId && p.TournamentId == tournamentId)
                .ToListAsync();

            foreach (var player in players)
            {
                player.TeamId = null;
            }

            // Удаляем команду
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBracket(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Players)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            var teams = tournament.Teams
                .Select(t => new
                {
                    Team = t,
                    AverageMMR = t.Players.Average(p => p.MMR)
                })
                .OrderByDescending(x => x.AverageMMR)
                .Select(x => x.Team)
                .ToList();

            if (teams.Count != 10)
            {
                return BadRequest("Для генерации сетки необходимо ровно 10 команд");
            }

            // Очищаем существующие матчи
            _context.Matches.RemoveRange(tournament.Matches);
            await _context.SaveChangesAsync();

            var matchTime = tournament.StartDate;
            var matchesLookup = new Dictionary<string, Match>();

            // --- Верхняя сетка ---
            // Раунд 1 (2 игры, 4 команды)
            var wb1 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime,
                HomeTeamId = teams[0].Id,
                AwayTeamId = teams[1].Id
            };
            _context.Matches.Add(wb1);
            matchesLookup["WB1"] = wb1;

            var wb2 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 2,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime,
                HomeTeamId = teams[2].Id,
                AwayTeamId = teams[3].Id
            };
            _context.Matches.Add(wb2);
            matchesLookup["WB2"] = wb2;

            // Раунд 2 (4 команды)
            var wb3 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(1),
                HomeTeamId = teams[4].Id,
                AwayTeamId = teams[5].Id
            };
            _context.Matches.Add(wb3);
            matchesLookup["WB3"] = wb3;

            var wb4 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 2,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(1),
                HomeTeamId = teams[6].Id,
                AwayTeamId = teams[7].Id
            };
            _context.Matches.Add(wb4);
            matchesLookup["WB4"] = wb4;

            // Раунд 3 (2 команды)
            var wb5 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(wb5);
            matchesLookup["WB5"] = wb5;

            // Раунд 4 (1 команда - финал верхней сетки)
            var wb6 = new Match
            {
                TournamentId = id,
                Round = 4,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(3)
            };
            _context.Matches.Add(wb6);
            matchesLookup["WB6"] = wb6;

            // --- Нижняя сетка ---
            // Раунд 1 (2 игры, 4 команды)
            var lb1 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(1)
            };
            _context.Matches.Add(lb1);
            matchesLookup["LB1"] = lb1;

            var lb2 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 2,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(1)
            };
            _context.Matches.Add(lb2);
            matchesLookup["LB2"] = lb2;

            // Раунд 2 (2 игры, 4 команды)
            var lb3 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(lb3);
            matchesLookup["LB3"] = lb3;

            var lb4 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 2,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(lb4);
            matchesLookup["LB4"] = lb4;

            // Раунд 3 (2 игры, 4 команды)
            var lb5 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(3)
            };
            _context.Matches.Add(lb5);
            matchesLookup["LB5"] = lb5;

            var lb6 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 2,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(3)
            };
            _context.Matches.Add(lb6);
            matchesLookup["LB6"] = lb6;

            // Раунд 4 (1 игра, 2 команды - финал нижней сетки)
            var lb7 = new Match
            {
                TournamentId = id,
                Round = 4,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(4)
            };
            _context.Matches.Add(lb7);
            matchesLookup["LB7"] = lb7;

            // --- Гранд-финал ---
            var gf = new Match
            {
                TournamentId = id,
                Round = 5,
                MatchNumber = 1,
                BracketType = BracketType.GrandFinal,
                IsGrandFinal = true,
                ScheduledTime = matchTime.AddDays(5)
            };
            _context.Matches.Add(gf);
            matchesLookup["GF"] = gf;

            // Сначала сохраняем все матчи
            await _context.SaveChangesAsync();

            // Теперь устанавливаем связи между матчами
            wb1.WinnerNextMatchId = wb6.Id;
            wb1.WinnerNextMatchPosition = 1;
            wb1.LoserNextMatchId = lb1.Id;
            wb1.LoserNextMatchPosition = 1;

            wb2.WinnerNextMatchId = wb6.Id;
            wb2.WinnerNextMatchPosition = 2;
            wb2.LoserNextMatchId = lb2.Id;
            wb2.LoserNextMatchPosition = 1;

            wb3.WinnerNextMatchId = wb5.Id;
            wb3.WinnerNextMatchPosition = 1;
            wb3.LoserNextMatchId = lb1.Id;
            wb3.LoserNextMatchPosition = 2;

            wb4.WinnerNextMatchId = wb5.Id;
            wb4.WinnerNextMatchPosition = 2;
            wb4.LoserNextMatchId = lb2.Id;
            wb4.LoserNextMatchPosition = 2;

            wb5.WinnerNextMatchId = wb6.Id;
            wb5.WinnerNextMatchPosition = 1;
            wb5.LoserNextMatchId = lb3.Id;
            wb5.LoserNextMatchPosition = 1;

            wb6.WinnerNextMatchId = gf.Id;
            wb6.WinnerNextMatchPosition = 1;
            wb6.LoserNextMatchId = lb7.Id;
            wb6.LoserNextMatchPosition = 1;

            // Связываем матчи нижней сетки
            lb1.WinnerNextMatchId = lb3.Id;
            lb1.WinnerNextMatchPosition = 1;

            lb2.WinnerNextMatchId = lb4.Id;
            lb2.WinnerNextMatchPosition = 1;

            lb3.WinnerNextMatchId = lb5.Id;
            lb3.WinnerNextMatchPosition = 1;

            lb4.WinnerNextMatchId = lb6.Id;
            lb4.WinnerNextMatchPosition = 1;

            lb5.WinnerNextMatchId = lb7.Id;
            lb5.WinnerNextMatchPosition = 1;

            lb6.WinnerNextMatchId = lb7.Id;
            lb6.WinnerNextMatchPosition = 2;

            lb7.WinnerNextMatchId = gf.Id;
            lb7.WinnerNextMatchPosition = 2;

            // Обновляем статус турнира
            tournament.Status = TournamentStatus.InProgress;
            _context.Tournaments.Update(tournament);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Bracket), new { id = tournament.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Bracket(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            if (!tournament.Matches.Any())
            {
                TempData["ErrorMessage"] = "Сетка турнира еще не сгенерирована";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Подготавливаем данные для представления
            var matches = tournament.Matches.ToList();

            var upperBracketMatches = matches
                .Where(m => m.BracketType == BracketType.UpperBracket)
                .GroupBy(m => m.Round)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.MatchNumber).ToList());

            var lowerBracketMatches = matches
                .Where(m => m.BracketType == BracketType.LowerBracket)
                .GroupBy(m => m.Round)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.MatchNumber).ToList());

            var grandFinals = matches
                .Where(m => m.BracketType == BracketType.GrandFinal)
                .OrderBy(m => m.IsSecondGrandFinal)
                .ToList();

            ViewBag.UpperBracketMatches = upperBracketMatches;
            ViewBag.LowerBracketMatches = lowerBracketMatches;
            ViewBag.GrandFinals = grandFinals;

            return View(tournament);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMatchResult(int matchId, int homeTeamScore, int awayTeamScore)
        {
            var match = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Tournament)
                    .ThenInclude(t => t.Matches)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                return NotFound();
            }

            match.HomeTeamScore = homeTeamScore;
            match.AwayTeamScore = awayTeamScore;
            match.IsCompleted = true;

            // Определяем победителя
            var winner = homeTeamScore > awayTeamScore ? match.HomeTeam : match.AwayTeam;
            var loser = homeTeamScore > awayTeamScore ? match.AwayTeam : match.HomeTeam;

            // Если это не финал, создаем матч следующего раунда
            if (match.Round < match.Tournament.Matches.Max(m => m.Round))
            {
                var nextRound = match.Round + 1;
                var nextMatchNumber = (match.MatchNumber + 1) / 2;
                var isUpperBracket = match.MatchNumber % 2 == 1;

                var nextMatch = match.Tournament.Matches
                    .FirstOrDefault(m => m.Round == nextRound && m.MatchNumber == nextMatchNumber);

                if (nextMatch != null)
                {
                    if (isUpperBracket)
                    {
                        nextMatch.HomeTeamId = winner.Id;
                    }
                    else
                    {
                        nextMatch.AwayTeamId = winner.Id;
                    }
                }
            }
            else
            {
                // Если это финал, отмечаем победителя
                winner.IsWinner = true;
                match.Tournament.Status = TournamentStatus.Completed;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Результат матча обновлен";
            return RedirectToAction(nameof(Bracket), new { id = match.TournamentId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMatchScore(int matchId, int homeTeamScore, int awayTeamScore)
        {
            var match = await _context.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
                return Json(new { success = false, message = "Матч не найден" });

            if (match.HomeTeamId == null || match.AwayTeamId == null)
                return Json(new { success = false, message = "В матче должны быть указаны обе команды" });

            match.HomeTeamScore = homeTeamScore;
            match.AwayTeamScore = awayTeamScore;
            match.IsCompleted = true;

            // Определяем победителя и проигравшего
            var winner = homeTeamScore > awayTeamScore ? match.HomeTeam : match.AwayTeam;
            var loser = homeTeamScore > awayTeamScore ? match.AwayTeam : match.HomeTeam;
            var winnerSeed = homeTeamScore > awayTeamScore ? match.HomeTeamSeed : match.AwayTeamSeed;
            var loserSeed = homeTeamScore > awayTeamScore ? match.AwayTeamSeed : match.HomeTeamSeed;

            // Продвижение победителя
            if (match.WinnerNextMatchId.HasValue)
            {
                var nextMatch = await _context.Matches.FirstOrDefaultAsync(m => m.Id == match.WinnerNextMatchId.Value);
                if (nextMatch != null)
                {
                    // Проверяем, есть ли уже команда в указанной позиции
                    if (match.WinnerNextMatchPosition == 1)
                    {
                        if (nextMatch.HomeTeamId == null)
                        {
                            nextMatch.HomeTeamId = winner.Id;
                            nextMatch.HomeTeamSeed = winnerSeed;
                        }
                        else if (nextMatch.AwayTeamId == null)
                        {
                            nextMatch.AwayTeamId = winner.Id;
                            nextMatch.AwayTeamSeed = winnerSeed;
                        }
                    }
                    else
                    {
                        if (nextMatch.AwayTeamId == null)
                        {
                            nextMatch.AwayTeamId = winner.Id;
                            nextMatch.AwayTeamSeed = winnerSeed;
                        }
                        else if (nextMatch.HomeTeamId == null)
                        {
                            nextMatch.HomeTeamId = winner.Id;
                            nextMatch.HomeTeamSeed = winnerSeed;
                        }
                    }
                    _context.Matches.Update(nextMatch);
                }
            }

            // Продвижение проигравшего
            if (match.LoserNextMatchId.HasValue)
            {
                var loserMatch = await _context.Matches.FirstOrDefaultAsync(m => m.Id == match.LoserNextMatchId.Value);
                if (loserMatch != null)
                {
                    // Проверяем, есть ли уже команда в указанной позиции
                    if (match.LoserNextMatchPosition == 1)
                    {
                        if (loserMatch.HomeTeamId == null)
                        {
                            loserMatch.HomeTeamId = loser.Id;
                            loserMatch.HomeTeamSeed = loserSeed;
                        }
                        else if (loserMatch.AwayTeamId == null)
                        {
                            loserMatch.AwayTeamId = loser.Id;
                            loserMatch.AwayTeamSeed = loserSeed;
                        }
                    }
                    else
                    {
                        if (loserMatch.AwayTeamId == null)
                        {
                            loserMatch.AwayTeamId = loser.Id;
                            loserMatch.AwayTeamSeed = loserSeed;
                        }
                        else if (loserMatch.HomeTeamId == null)
                        {
                            loserMatch.HomeTeamId = loser.Id;
                            loserMatch.HomeTeamSeed = loserSeed;
                        }
                    }
                    _context.Matches.Update(loserMatch);
                }
            }

            // Гранд-финал
            if (match.IsGrandFinal && !match.IsSecondGrandFinal)
            {
                var grandFinal = await _context.Matches
                    .Where(m => m.TournamentId == match.TournamentId && m.BracketType == BracketType.GrandFinal && m.IsSecondGrandFinal)
                    .FirstOrDefaultAsync();

                var upperFinal = await _context.Matches
                    .Where(m => m.TournamentId == match.TournamentId && m.BracketType == BracketType.UpperBracket)
                    .OrderByDescending(m => m.Round)
                    .FirstOrDefaultAsync();

                if (upperFinal != null)
                {
                    var upperWinner = homeTeamScore > awayTeamScore ? match.HomeTeam : match.AwayTeam;
                    if (upperFinal.HomeTeamId != null && upperFinal.HomeTeamId != upperWinner.Id)
                    {
                        // Проигравший из верхней выиграл гранд-финал => второй финал
                        if (grandFinal == null)
                        {
                            var secondFinal = new Match
                            {
                                TournamentId = match.TournamentId,
                                HomeTeamId = winner.Id,
                                HomeTeamSeed = winnerSeed,
                                AwayTeamId = loser.Id,
                                AwayTeamSeed = loserSeed,
                                Round = match.Round + 1,
                                MatchNumber = match.MatchNumber + 1,
                                BracketType = BracketType.GrandFinal,
                                ScheduledTime = match.ScheduledTime?.AddHours(2),
                                IsGrandFinal = true,
                                IsSecondGrandFinal = true
                            };
                            _context.Matches.Add(secondFinal);
                        }
                    }
                    else
                    {
                        // Победитель верхней выиграл финал
                        await SetTournamentWinner(match.TournamentId, winner.Id);
                    }
                }
            }
            else if (match.IsSecondGrandFinal)
            {
                await SetTournamentWinner(match.TournamentId, winner.Id);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMatchTeam(int matchId, int teamId, string position)
        {
            var match = await _context.Matches
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                return Json(new { success = false, message = "Матч не найден" });
            }

            if (match.Tournament.Status != TournamentStatus.InProgress)
            {
                return Json(new { success = false, message = "Обновление команд возможно только во время активного турнира" });
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId && t.TournamentId == match.TournamentId);

            if (team == null)
            {
                return Json(new { success = false, message = "Команда не найдена" });
            }

            // Проверяем, не участвует ли команда уже в этом матче
            if ((match.HomeTeamId == teamId && position == "away") ||
                (match.AwayTeamId == teamId && position == "home"))
            {
                return Json(new { success = false, message = "Команда уже участвует в этом матче" });
            }

            // Обновляем позицию команды
            if (position == "home")
            {
                match.HomeTeamId = teamId;
            }
            else if (position == "away")
            {
                match.AwayTeamId = teamId;
            }
            else
            {
                return Json(new { success = false, message = "Неверная позиция команды" });
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Произошла ошибка при обновлении матча" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBracket10Teams(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Players)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            var teams = tournament.Teams
                .Select(t => new
                {
                    Team = t,
                    AverageMMR = t.Players.Average(p => p.MMR)
                })
                .OrderByDescending(x => x.AverageMMR)
                .Select(x => x.Team)
                .ToList();

            if (teams.Count != 10)
            {
                return BadRequest("Для генерации сетки необходимо ровно 10 команд");
            }

            // Очищаем существующие матчи
            _context.Matches.RemoveRange(tournament.Matches);
            await _context.SaveChangesAsync();

            var matchTime = tournament.StartDate;
            var matchesLookup = new Dictionary<string, Match>();

            // --- Верхняя сетка ---
            // Раунд 1 (2 игры, 4 команды)
            var wb1 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime,
                HomeTeamId = teams[0].Id,
                AwayTeamId = teams[1].Id
            };
            _context.Matches.Add(wb1);
            matchesLookup["WB1"] = wb1;

            var wb2 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 2,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime,
                HomeTeamId = teams[2].Id,
                AwayTeamId = teams[3].Id
            };
            _context.Matches.Add(wb2);
            matchesLookup["WB2"] = wb2;

            // Раунд 2 (4 игры, 8 команд)
            var wb3 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(1),
                HomeTeamId = teams[4].Id,
                AwayTeamId = teams[5].Id
            };
            _context.Matches.Add(wb3);
            matchesLookup["WB3"] = wb3;

            var wb4 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 2,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(1),
                HomeTeamId = teams[6].Id,
                AwayTeamId = teams[7].Id
            };
            _context.Matches.Add(wb4);
            matchesLookup["WB4"] = wb4;

            var wb5 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 3,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(1),
                HomeTeamId = teams[8].Id,
                AwayTeamId = teams[9].Id
            };
            _context.Matches.Add(wb5);
            matchesLookup["WB5"] = wb5;

            var wb6 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 4,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(1)
            };
            _context.Matches.Add(wb6);
            matchesLookup["WB6"] = wb6;

            // Раунд 3 (2 игры, 4 команды)
            var wb7 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(wb7);
            matchesLookup["WB7"] = wb7;

            var wb8 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 2,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(wb8);
            matchesLookup["WB8"] = wb8;

            // Раунд 4 (1 игра, 2 команды - финал верхней сетки)
            var wb9 = new Match
            {
                TournamentId = id,
                Round = 4,
                MatchNumber = 1,
                BracketType = BracketType.UpperBracket,
                ScheduledTime = matchTime.AddDays(3)
            };
            _context.Matches.Add(wb9);
            matchesLookup["WB9"] = wb9;

            // --- Нижняя сетка ---
            // Раунд 1 (2 игры, 4 команды)
            var lb1 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(1)
            };
            _context.Matches.Add(lb1);
            matchesLookup["LB1"] = lb1;

            var lb2 = new Match
            {
                TournamentId = id,
                Round = 1,
                MatchNumber = 2,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(1)
            };
            _context.Matches.Add(lb2);
            matchesLookup["LB2"] = lb2;

            // Раунд 2 (2 игры, 4 команды)
            var lb3 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(lb3);
            matchesLookup["LB3"] = lb3;

            var lb4 = new Match
            {
                TournamentId = id,
                Round = 2,
                MatchNumber = 2,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(2)
            };
            _context.Matches.Add(lb4);
            matchesLookup["LB4"] = lb4;

            // Раунд 3 (2 игры, 4 команды)
            var lb5 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(3)
            };
            _context.Matches.Add(lb5);
            matchesLookup["LB5"] = lb5;

            var lb6 = new Match
            {
                TournamentId = id,
                Round = 3,
                MatchNumber = 2,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(3)
            };
            _context.Matches.Add(lb6);
            matchesLookup["LB6"] = lb6;

            // Раунд 4 (1 игра, 2 команды)
            var lb7 = new Match
            {
                TournamentId = id,
                Round = 4,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(4)
            };
            _context.Matches.Add(lb7);
            matchesLookup["LB7"] = lb7;

            // Раунд 5 (1 игра, 2 команды - финал нижней сетки)
            var lb8 = new Match
            {
                TournamentId = id,
                Round = 5,
                MatchNumber = 1,
                BracketType = BracketType.LowerBracket,
                ScheduledTime = matchTime.AddDays(5)
            };
            _context.Matches.Add(lb8);
            matchesLookup["LB8"] = lb8;

            // --- Гранд-финал ---
            var gf = new Match
            {
                TournamentId = id,
                Round = 6,
                MatchNumber = 1,
                BracketType = BracketType.GrandFinal,
                IsGrandFinal = true,
                ScheduledTime = matchTime.AddDays(6)
            };
            _context.Matches.Add(gf);
            matchesLookup["GF"] = gf;

            // Сначала сохраняем все матчи
            await _context.SaveChangesAsync();

            // Теперь устанавливаем связи между матчами
            wb1.WinnerNextMatchId = wb6.Id;
            wb1.WinnerNextMatchPosition = 1;
            wb1.LoserNextMatchId = lb1.Id;
            wb1.LoserNextMatchPosition = 1;

            wb2.WinnerNextMatchId = wb6.Id;
            wb2.WinnerNextMatchPosition = 2;
            wb2.LoserNextMatchId = lb2.Id;
            wb2.LoserNextMatchPosition = 1;

            wb3.WinnerNextMatchId = wb7.Id;
            wb3.WinnerNextMatchPosition = 1;
            wb3.LoserNextMatchId = lb1.Id;
            wb3.LoserNextMatchPosition = 2;

            wb4.WinnerNextMatchId = wb7.Id;
            wb4.WinnerNextMatchPosition = 2;
            wb4.LoserNextMatchId = lb2.Id;
            wb4.LoserNextMatchPosition = 2;

            wb5.WinnerNextMatchId = wb8.Id;
            wb5.WinnerNextMatchPosition = 1;
            wb5.LoserNextMatchId = lb3.Id;
            wb5.LoserNextMatchPosition = 1;

            wb6.WinnerNextMatchId = wb8.Id;
            wb6.WinnerNextMatchPosition = 2;
            wb6.LoserNextMatchId = lb4.Id;
            wb6.LoserNextMatchPosition = 1;

            wb7.WinnerNextMatchId = wb9.Id;
            wb7.WinnerNextMatchPosition = 1;
            wb7.LoserNextMatchId = lb5.Id;
            wb7.LoserNextMatchPosition = 1;

            wb8.WinnerNextMatchId = wb9.Id;
            wb8.WinnerNextMatchPosition = 2;
            wb8.LoserNextMatchId = lb6.Id;
            wb8.LoserNextMatchPosition = 1;

            wb9.WinnerNextMatchId = gf.Id;
            wb9.WinnerNextMatchPosition = 1;
            wb9.LoserNextMatchId = lb8.Id;
            wb9.LoserNextMatchPosition = 1;

            // Связываем матчи нижней сетки
            lb1.WinnerNextMatchId = lb3.Id;
            lb1.WinnerNextMatchPosition = 1;

            lb2.WinnerNextMatchId = lb4.Id;
            lb2.WinnerNextMatchPosition = 1;

            lb3.WinnerNextMatchId = lb5.Id;
            lb3.WinnerNextMatchPosition = 1;

            lb4.WinnerNextMatchId = lb6.Id;
            lb4.WinnerNextMatchPosition = 1;

            lb5.WinnerNextMatchId = lb7.Id;
            lb5.WinnerNextMatchPosition = 1;

            lb6.WinnerNextMatchId = lb7.Id;
            lb6.WinnerNextMatchPosition = 2;

            lb7.WinnerNextMatchId = lb8.Id;
            lb7.WinnerNextMatchPosition = 2;

            lb8.WinnerNextMatchId = gf.Id;
            lb8.WinnerNextMatchPosition = 2;

            // Обновляем все матчи с новыми связями
            _context.Matches.UpdateRange(wb1, wb2, wb3, wb4, wb5, wb6, wb7, wb8, wb9, lb1, lb2, lb3, lb4, lb5, lb6, lb7, lb8);

            // Обновляем статус турнира
            tournament.Status = TournamentStatus.InProgress;
            _context.Tournaments.Update(tournament);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Bracket), new { id = tournament.Id });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBracketWithTeamCount(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                    .ThenInclude(team => team.Players)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            var teams = tournament.Teams
                .Select(t => new
                {
                    Team = t,
                    AverageMMR = t.Players.Average(p => p.MMR)
                })
                .OrderByDescending(x => x.AverageMMR)
                .Select(x => x.Team)
                .ToList();

            if (teams.Count < 4)
            {
                return BadRequest("Для генерации сетки необходимо минимум 4 команды");
            }

            // Очищаем существующие матчи
            _context.Matches.RemoveRange(tournament.Matches);
            await _context.SaveChangesAsync();

            // В зависимости от количества команд вызываем соответствующий метод
            if (teams.Count == 10)
            {
                return await GenerateBracket10Teams(id);
            }
            else
            {
                return await GenerateBracket(id);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RandomizeTeams(int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return Json(new { success = false, message = "Турнир не найден" });
            }

            var teams = tournament.Teams.ToList();
            var random = new Random();

            // Перемешиваем команды
            for (int i = teams.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = teams[i];
                teams[i] = teams[j];
                teams[j] = temp;
            }

            // Обновляем порядок команд
            for (int i = 0; i < teams.Count; i++)
            {
                teams[i].Order = i;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool TournamentExists(int id)
        {
            return _context.Tournaments.Any(e => e.Id == id);
        }

        private async Task SetTournamentWinner(int tournamentId, int winnerTeamId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament != null)
            {
                var winnerTeam = tournament.Teams.FirstOrDefault(t => t.Id == winnerTeamId);
                if (winnerTeam != null)
                {
                    winnerTeam.IsWinner = true;
                    tournament.Status = TournamentStatus.Completed;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}