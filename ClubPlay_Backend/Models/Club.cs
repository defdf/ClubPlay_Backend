using System;
using System.Collections.Generic;

namespace ClubPlay_Backend.Models
{
    public partial class Club
    {
        public Club()
        {
            Subscription = new HashSet<Subscription>();
            Token = new HashSet<Token>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string OrgNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Subscription> Subscription { get; set; }
        public virtual ICollection<Token> Token { get; set; }
    }
}
