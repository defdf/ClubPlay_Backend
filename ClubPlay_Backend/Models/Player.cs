using System;
using System.Collections.Generic;

namespace ClubPlay_Backend.Models
{
    public partial class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
