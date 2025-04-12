using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotamix.Data;
using dotamix.Models;

namespace dotamix.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .Include(t => t.Players)
                .Where(t => t.Tournament.Status == TournamentStatus.InProgress || t.Tournament.Status == TournamentStatus.TeamFormation)
                .ToListAsync();
            return View(teams);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .Include(t => t.Players)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id && 
                    (m.Tournament.Status == TournamentStatus.InProgress || 
                     m.Tournament.Status == TournamentStatus.TeamFormation));

            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // GET: Teams/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Tournaments = await _context.Tournaments
                .Where(t => t.Status == TournamentStatus.TeamFormation)
                .ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == team.TournamentId);

            if (tournament == null || tournament.Status != TournamentStatus.TeamFormation)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tournaments = await _context.Tournaments
                .Where(t => t.Status == TournamentStatus.TeamFormation)
                .ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id && 
                    (m.Tournament.Status == TournamentStatus.InProgress || 
                     m.Tournament.Status == TournamentStatus.TeamFormation));

            if (team == null)
            {
                return NotFound();
            }

            ViewBag.Tournaments = await _context.Tournaments
                .Where(t => t.Status == TournamentStatus.TeamFormation)
                .ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == team.TournamentId);

            if (tournament == null || 
                (tournament.Status != TournamentStatus.InProgress && 
                 tournament.Status != TournamentStatus.TeamFormation))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id))
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

            ViewBag.Tournaments = await _context.Tournaments
                .Where(t => t.Status == TournamentStatus.TeamFormation)
                .ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            // Очищаем TeamId у всех игроков команды
            foreach (var player in team.Players)
            {
                player.TeamId = null;
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
} 