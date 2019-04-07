using System;
using System.Collections.Generic;

namespace ClubPlay_Backend.Models
{
    public partial class Game
    {
        public int Id { get; set; }
        public byte Sensor { get; set; }
        public int Player1Id { get; set; }
        public int? Player2Id { get; set; }
        public int? ScorePlayer1 { get; set; }
        public int? ScorePlayer2 { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
