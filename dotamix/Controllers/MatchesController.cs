using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotamix.Data;
using dotamix.Models;

namespace dotamix.Controllers
{
    public class MatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Tournament.Status == TournamentStatus.InProgress)
                .ToListAsync();
            return View(matches);
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == id && m.Tournament.Status == TournamentStatus.InProgress);

            if (match == null)
            {
                return NotFound();
            }
            return View(match);
        }

        // GET: Matches/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Tournaments = await _context.Tournaments.Where(t => t.Status == TournamentStatus.InProgress).ToListAsync();
            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View();
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TournamentId,HomeTeamId,AwayTeamId,ScheduledTime")] Match match)
        {
            if (ModelState.IsValid)
            {
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Tournaments = await _context.Tournaments.Where(t => t.Status == TournamentStatus.InProgress).ToListAsync();
            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(match);
        }

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id && m.Tournament.Status == TournamentStatus.InProgress);

            if (match == null)
            {
                return NotFound();
            }
            ViewBag.Tournaments = await _context.Tournaments.Where(t => t.Status == TournamentStatus.InProgress).ToListAsync();
            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(match);
        }

        // POST: Matches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TournamentId,HomeTeamId,AwayTeamId,ScheduledTime,HomeTeamScore,AwayTeamScore,IsCompleted")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == match.TournamentId);

            if (tournament == null || tournament.Status != TournamentStatus.InProgress)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.Id))
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
            ViewBag.Tournaments = await _context.Tournaments.Where(t => t.Status == TournamentStatus.InProgress).ToListAsync();
            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(match);
        }

        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MatchExists(int id)
        {
            return _context.Matches.Any(e => e.Id == id);
        }
    }
} 