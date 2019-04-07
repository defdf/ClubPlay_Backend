using System;
using System.Collections.Generic;

namespace ClubPlay_Backend.Models
{
    public partial class Subscription
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public float Payment { get; set; }
        public DateTime ExpireAt { get; set; }

        public virtual Club Club { get; set; }
    }
}
