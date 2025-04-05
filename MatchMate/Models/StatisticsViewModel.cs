using System;

namespace MatchMate.Models
{
    public class StatisticsViewModel
    {
        public GameStatsViewModel TicTacToe { get; set; }
        public GameStatsViewModel RockPaperScissors { get; set; }
    }

    public class GameStatsViewModel
    {
        public string GameName { get; set; }
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }

        public int ComputerGames { get; set; }
        public int ComputerWins { get; set; }
        public int ComputerLosses { get; set; }
        public int ComputerTies { get; set; }

        public int UserGames { get; set; }
        public int UserWins { get; set; }
        public int UserLosses { get; set; }
        public int UserTies { get; set; }
    }
}