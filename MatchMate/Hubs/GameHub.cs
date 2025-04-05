using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using MatchMate.Models;
using Microsoft.AspNet.Identity;

namespace MatchMate.Hubs
{
    public class GameHub : Hub
    {
        private static Dictionary<string, List<string>> gameRooms = new Dictionary<string, List<string>>();
        private static Dictionary<string, string> userGameMapping = new Dictionary<string, string>();
        private ApplicationDbContext _context = new ApplicationDbContext();
        public void JoinGame(string gameId, string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"JOIN GAME - User {userId} joining game {gameId}");
                Groups.Add(Context.ConnectionId, gameId);

                if (!gameRooms.ContainsKey(gameId))
                {
                    gameRooms[gameId] = new List<string>();
                }

                if (!gameRooms[gameId].Contains(userId))
                {
                    gameRooms[gameId].Add(userId);
                    userGameMapping[Context.ConnectionId] = gameId;
                }

                if (gameRooms[gameId].Count == 1)
                {
                    System.Diagnostics.Debug.WriteLine($"First player {userId} joined. Waiting...");
                    Clients.Client(Context.ConnectionId).WaitingForOpponent();
                }
                else if (gameRooms[gameId].Count == 2)
                {
                    System.Diagnostics.Debug.WriteLine($"Second player {userId} joined. Starting game.");
                    UpdateMatchWithSecondPlayer(gameId, userId);
                    foreach (var playerId in gameRooms[gameId])
                    {
                        Clients.Group(gameId).PlayerJoined(playerId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in JoinGame: {ex.Message}");
                Clients.Caller.HandleError($"Error joining game: {ex.Message}");
            }
        }

        private void UpdateMatchWithSecondPlayer(string gameId, string player2Id)
        {
            try
            {
                var matchParts = gameId.Split('_');
                if (matchParts.Length > 1 && int.TryParse(matchParts[1], out int gameTypeId))
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var match = context.Matches.FirstOrDefault(m => m.MatchGuid == gameId);
                        if (match != null)
                        {
                            match.Player2Id = player2Id;
                            match.WaitingForOpponent = false;
                            context.SaveChanges();
                            System.Diagnostics.Debug.WriteLine($"Database updated: Match {gameId} now has player2 = {player2Id}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating match in database: {ex.Message}");
            }
        }

        public void MakeMove(string gameId, int index, string symbol)
        {
            try
            {
                Clients.Group(gameId).ReceiveMove(index, symbol);
            }
            catch (Exception ex)
            {
                Clients.Caller.HandleError($"Error making move: {ex.Message}");
            }
        }

        public void GameOver(string gameId, string winnerId)
        {
            try
            {
                Clients.Group(gameId).GameOver(winnerId);

                var match = _context.Matches.FirstOrDefault(m => m.MatchGuid == gameId);
                if (match != null)
                {
                    match.WinnerId = winnerId == "null" ? null : winnerId;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.HandleError($"Error processing game over: {ex.Message}");
            }
        }

        public void ExitGame(string gameId, string userId)
        {
            try
            {
                Groups.Remove(Context.ConnectionId, gameId);

                if (gameRooms.ContainsKey(gameId))
                {
                    gameRooms[gameId].Remove(userId);
                    if (gameRooms[gameId].Count == 0)
                    {
                        gameRooms.Remove(gameId);
                    }
                    else
                    {
                        Clients.Group(gameId).OpponentLeft(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.HandleError($"Error exiting game: {ex.Message}");
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var userId = Context.User.Identity.GetUserId();
                System.Diagnostics.Debug.WriteLine($"User disconnected: {userId}, Connection ID: {Context.ConnectionId}");

                if (userGameMapping.ContainsKey(Context.ConnectionId))
                {
                    var gameId = userGameMapping[Context.ConnectionId];

                    userGameMapping.Remove(Context.ConnectionId);

                    if (gameRooms.ContainsKey(gameId) && !string.IsNullOrEmpty(userId))
                    {
                        // Remove user from the game room
                        gameRooms[gameId].Remove(userId);

                        // Notify other players if anyone is left in the room
                        if (gameRooms[gameId].Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Notifying players in game {gameId} that {userId} has left");
                            Clients.Group(gameId).OpponentLeft(userId);
                        }

                        // Clean up empty rooms
                        if (gameRooms[gameId].Count == 0)
                        {
                            gameRooms.Remove(gameId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnDisconnected: {ex.Message}");
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}