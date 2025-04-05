using MatchMate.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class Match
{
    public int MatchId { get; set; }
    [StringLength(100)]
    public string MatchGuid { get; set; }
    public int GameId { get; set; }
    public string Player1Id { get; set; }
    public string Player2Id { get; set; }
    public DateTime MatchDate { get; set; }
    public string WinnerId { get; set; }
    public bool WaitingForOpponent { get; set; }
    public bool IsComputerWinner { get; set; } 

    public Game Game { get; set; }
    public ApplicationUser Player1 { get; set; }
    public ApplicationUser Player2 { get; set; }
    public ApplicationUser Winner { get; set; }
}