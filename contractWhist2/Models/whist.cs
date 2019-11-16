using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace contractWhist2.Models
{
    public class card
    {
        public int id { get; set; }
        public string name { get; set; }
        public string fullname { get; set; }
        public int valueInSuit { get; set; }
        public string suit { get; set; }
    }

    public class gameRound
    {
        public int id { get; set; }
        public int gameid { get; set; }
        public int roundNumber { get; set; }
        public int numCards { get; set; }
        public card trumpCards { get; set; }
        public string trumpSuit { get; set; }

    }

    public class playerRoundCard
    {
        public int id { get; set; }
        public int prID { get; set; }
        public int playerId { get; set; }
        public int roundid { get; set; }
        public card thisCard { get; set; }
        public string cardPlayed { get; set; }

    }

    public class game
    {
        public int id { get; set; }
        public DateTime crdate { get; set; }
        public gameRound currentRound { get; set; }
    }

    public class playerRound 
    {
        public int id { get; set; }
        public int gameId { get; set; }
        public int roundId { get; set; }
        public int playerId { get; set; }
        public int target { get; set; }
        public int tricksWon { get; set; }
        public int roundPoints { get; set; }
        public int gamePoints { get; set; }
        public playerRoundCard[] pCards { get; set; }
    }


}
