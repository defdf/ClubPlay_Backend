using System;
using System.Collections.Generic;

namespace ClubPlay_Backend.Models
{
    public partial class Token
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public DateTime ExpireAt { get; set; }
        public int ClubId { get; set; }

        public virtual Club Club { get; set; }
    }
}
