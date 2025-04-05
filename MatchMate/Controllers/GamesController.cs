using MatchMate.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MatchMate.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index()
        {
            var games = _context.Games.ToList();
            return View(games);
        }

        public ActionResult Play(int id)
        {
            var game = _context.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            string waitingRoomId = $"waitingroom_{id}";

            var waitingMatch = _context.Matches
                .FirstOrDefault(m => m.GameId == id && m.Player2Id == null && m.WaitingForOpponent == true);

            string matchGuid;
            if (waitingMatch != null)
            {
                matchGuid = waitingMatch.MatchGuid;
            }
            else
            {
                matchGuid = $"{Guid.NewGuid()}_{id}";
                var newMatch = new Match
                {
                    MatchGuid = matchGuid,
                    GameId = id,
                    Player1Id = User.Identity.GetUserId(),
                    MatchDate = DateTime.Now,
                    WaitingForOpponent = true
                };
                _context.Matches.Add(newMatch);
                _context.SaveChanges();
            }

            ViewBag.GameId = matchGuid;
            ViewBag.GameName = game.GameName;
            return View();
        }

        [HttpGet]
        public JsonResult GetUsername(string playerId)
        {
            var user = _context.Users.Find(playerId);
            return Json(new { username = user?.Username ?? "Unknown" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Statistics()
        {
            try
            {
                string userId = User.Identity.GetUserId();

                var tictactoe = _context.Games.FirstOrDefault(g => g.GameName == "Tic-Tac-Toe");
                var rps = _context.Games.FirstOrDefault(g => g.GameName == "Rock-Paper-Scissors");

                var model = new StatisticsViewModel();

                if (tictactoe != null)
                {
                    var tttMatches = _context.Matches.Where(m => m.GameId == tictactoe.GameId &&
                        (m.Player1Id == userId || m.Player2Id == userId)).ToList();

                    model.TicTacToe = new GameStatsViewModel
                    {
                        TotalGames = tttMatches.Count,
                        Wins = tttMatches.Count(m => m.WinnerId == userId),
                        Losses = tttMatches.Count(m => m.WinnerId != null && m.WinnerId != userId) +
                                 tttMatches.Count(m => m.Player2Id == null && m.IsComputerWinner),
                        Ties = tttMatches.Count(m => m.WinnerId == null && !m.IsComputerWinner),

                        ComputerGames = tttMatches.Count(m => m.Player2Id == null),
                        ComputerWins = tttMatches.Count(m => m.Player2Id == null && m.WinnerId == userId),
                        ComputerLosses = tttMatches.Count(m => m.Player2Id == null && m.IsComputerWinner),
                        ComputerTies = tttMatches.Count(m => m.Player2Id == null && m.WinnerId == null && !m.IsComputerWinner),

                        UserGames = tttMatches.Count(m => m.Player2Id != null),
                        UserWins = tttMatches.Count(m => m.Player2Id != null && m.WinnerId == userId),
                        UserLosses = tttMatches.Count(m => m.Player2Id != null && m.WinnerId != null && m.WinnerId != userId),
                        UserTies = tttMatches.Count(m => m.Player2Id != null && m.WinnerId == null)
                    };

                    ViewBag.TicTacToe = model.TicTacToe;
                }

                if (rps != null)
                {
                    var rpsMatches = _context.Matches.Where(m => m.GameId == rps.GameId &&
                        (m.Player1Id == userId || m.Player2Id == userId)).ToList();

                    model.RockPaperScissors = new GameStatsViewModel
                    {
                        TotalGames = rpsMatches.Count,
                        Wins = rpsMatches.Count(m => m.WinnerId == userId),
                        Losses = rpsMatches.Count(m => m.WinnerId != null && m.WinnerId != userId),
                        Ties = rpsMatches.Count(m => m.WinnerId == null),

                        ComputerGames = rpsMatches.Count(m => m.Player2Id == null || m.Player2Id == "computer-opponent"),
                        ComputerWins = rpsMatches.Count(m => (m.Player2Id == null || m.Player2Id == "computer-opponent") && m.WinnerId == userId),
                        ComputerLosses = rpsMatches.Count(m => (m.Player2Id == null || m.Player2Id == "computer-opponent") && m.WinnerId != userId && m.WinnerId != null),
                        ComputerTies = rpsMatches.Count(m => (m.Player2Id == null || m.Player2Id == "computer-opponent") && m.WinnerId == null),

                        UserGames = rpsMatches.Count(m => m.Player2Id != null && m.Player2Id != "computer-opponent"),
                        UserWins = rpsMatches.Count(m => m.Player2Id != null && m.Player2Id != "computer-opponent" && m.WinnerId == userId),
                        UserLosses = rpsMatches.Count(m => m.Player2Id != null && m.Player2Id != "computer-opponent" && m.WinnerId != null && m.WinnerId != userId),
                        UserTies = rpsMatches.Count(m => m.Player2Id != null && m.Player2Id != "computer-opponent" && m.WinnerId == null)
                    };

                    ViewBag.RockPaperScissors = model.RockPaperScissors;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Statistics: " + ex.Message);
                ViewBag.ErrorMessage = "An error occurred while loading your statistics.";
                return View(new StatisticsViewModel());
            }
        }


        [HttpGet]
        public JsonResult GetGameStats(int gameId)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var game = _context.Games.Find(gameId);

                if (game == null)
                {
                    return Json(new { success = false, message = "Game not found" }, JsonRequestBehavior.AllowGet);
                }

                _context.Database.Connection.Open();

                var matches = _context.Matches
                    .Where(m => m.GameId == gameId && (m.Player1Id == userId || m.Player2Id == userId))
                    .ToList();

                int totalGames = matches.Count;
                int wins = matches.Count(m => m.WinnerId == userId);
                int losses = matches.Count(m => m.WinnerId != null && m.WinnerId != userId);
                int ties = matches.Count(m => m.WinnerId == null);

                var computerMatches = matches.Where(m => m.Player2Id == "computer-opponent").ToList();
                int computerGames = computerMatches.Count;
                int computerWins = computerMatches.Count(m => m.WinnerId == userId);

                var humanMatches = matches.Where(m => m.Player2Id != "computer-opponent" && m.Player2Id != null).ToList();
                int humanGames = humanMatches.Count;
                int humanWins = humanMatches.Count(m => m.WinnerId == userId);

                var stats = new
                {
                    total = totalGames,
                    wins = wins,
                    losses = losses,
                    ties = ties,
                    computerGames = computerGames,
                    computerWins = computerWins,
                    computerWinRate = computerGames > 0 ? (int)(100.0 * computerWins / computerGames) : 0,
                    humanGames = humanGames,
                    humanWins = humanWins,
                    humanWinRate = humanGames > 0 ? (int)(100.0 * humanWins / humanGames) : 0
                };

                return Json(new { success = true, stats = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    details = ex.InnerException?.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveTTTResult(string gameId, string result, bool isComputer)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                System.Diagnostics.Debug.WriteLine($"SAVETTTRESULT - GameId: {gameId}, Result: {result}, IsComputer: {isComputer}, User: {userId}");

                int gameTypeId = 1;
                if (!string.IsNullOrEmpty(gameId) && gameId.Contains("_"))
                {
                    int.TryParse(gameId.Split('_')[1], out gameTypeId);
                }

                Match match;
                if (isComputer)
                {
                    string newMatchGuid = Guid.NewGuid().ToString() + "_" + gameTypeId;
                    match = new Match
                    {
                        MatchGuid = newMatchGuid,
                        GameId = gameTypeId,
                        Player1Id = userId,
                        Player2Id = null,
                        MatchDate = DateTime.Now,
                        WaitingForOpponent = false
                    };
                }
                else
                {
                    match = _context.Matches.FirstOrDefault(m => m.MatchGuid == gameId);
                    if (match == null)
                    {
                        return Json(new { success = false, message = "Match not found" }, JsonRequestBehavior.AllowGet);
                    }
                }

                switch (result.ToLower())
                {
                    case "win":
                        match.WinnerId = userId;
                        match.IsComputerWinner = false;
                        break;
                    case "loss":
                        if (match.Player1Id == userId)
                            match.WinnerId = match.Player2Id;
                        else
                            match.WinnerId = match.Player1Id;

                        match.IsComputerWinner = isComputer;
                        break;
                    case "tie":
                        match.WinnerId = null;
                        match.IsComputerWinner = false;
                        break;
                    default:
                        match.WinnerId = null;
                        match.IsComputerWinner = false;
                        break;
                }

                if (isComputer)
                {
                    _context.Matches.Add(match);
                }
                int rowsAffected = _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Match saved/updated. ID: {match.MatchId}. Rows affected: {rowsAffected}");

                return Json(new { success = true, message = "Game result saved", matchId = match.MatchId });
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                string errorDetails = ex.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine($"DbUpdateException: {errorDetails}");
                return Json(new { success = false, message = "Database error: " + errorDetails });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SaveTTTResult: {ex.Message}");
                return Json(new { success = false, message = "Unexpected error: " + ex.Message });
            }
        }

        // ...

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveRPSResult(string result)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var rps = _context.Games.FirstOrDefault(g => g.GameName == "Rock-Paper-Scissors");

                System.Diagnostics.Debug.WriteLine($"[RPS SAVE] User: {userId}, Result: {result}, Game found: {rps != null}");

                if (rps != null)
                {
                    var match = new Match
                    {
                        MatchGuid = $"{Guid.NewGuid()}_{rps.GameId}",
                        GameId = rps.GameId,
                        Player1Id = userId,
                        Player2Id = null, 
                        MatchDate = DateTime.Now,
                        WaitingForOpponent = false
                    };

                    if (result == "win")
                    {
                        match.WinnerId = userId;
                        match.IsComputerWinner = false;
                    }
                    else if (result == "lose")
                    {
                        match.WinnerId = null;
                        match.IsComputerWinner = true;
                    }
                    else 
                    {
                        match.WinnerId = null;
                        match.IsComputerWinner = false;
                    }

                    System.Diagnostics.Debug.WriteLine($"[RPS SAVE] Adding match: GameId={match.GameId}, " +
                        $"Player1Id={match.Player1Id}, WinnerId={match.WinnerId}, " +
                        $"IsComputerWinner={match.IsComputerWinner}");

                    _context.Matches.Add(match);

                    try
                    {
                        int rowsAffected = _context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"[RPS SAVE] Success! Rows affected: {rowsAffected}");
                        return Json(new { success = true, matchId = match.MatchId });
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {

                        string fullError = ex.Message;
                        Exception innerEx = ex.InnerException;
                        while (innerEx != null)
                        {
                            fullError += " | Inner: " + innerEx.Message;
                            innerEx = innerEx.InnerException;
                        }

                        System.Diagnostics.Debug.WriteLine($"[RPS SAVE ERROR] {fullError}");
                        return Json(new { success = false, message = fullError });
                    }
                }

                return Json(new { success = false, message = "Game not found" });
            }
            catch (Exception ex)
            {
                string fullError = $"Exception: {ex.Message}";
                if (ex.InnerException != null)
                {
                    fullError += $" | Inner: {ex.InnerException.Message}";
                }

                System.Diagnostics.Debug.WriteLine($"[RPS SAVE CRITICAL] {fullError}");
                return Json(new { success = false, message = fullError });
            }
        }

    }
}