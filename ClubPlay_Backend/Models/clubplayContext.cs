using Microsoft.EntityFrameworkCore;

namespace ClubPlay_Backend.Models
{
    public partial class clubplayContext : DbContext
    {
        public clubplayContext()
        {
        }

        public clubplayContext(DbContextOptions<clubplayContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Club> Club { get; set; }
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<Player> Player { get; set; }
        public virtual DbSet<Subscription> Subscription { get; set; }
        public virtual DbSet<Token> Token { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=root;database=clubplay");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity<Club>(entity =>
            {
                entity.ToTable("club", "clubplay");

                entity.HasIndex(e => e.Email)
                    .HasName("email_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.OrgNumber)
                    .HasName("orgNumber_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.OrgNumber)
                    .IsRequired()
                    .HasColumnName("orgNumber")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("game", "clubplay");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EndAt).HasColumnName("endAt");

                entity.Property(e => e.Player1Id)
                    .HasColumnName("player1_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Player2Id)
                    .HasColumnName("player2_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ScorePlayer1)
                    .HasColumnName("score_player1")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ScorePlayer2)
                    .HasColumnName("score_player2")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Sensor)
                    .HasColumnName("sensor")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.StartAt).HasColumnName("startAt");
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("player", "clubplay");

                entity.HasIndex(e => e.Email)
                    .HasName("email_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscription", "clubplay");

                entity.HasIndex(e => e.ClubId)
                    .HasName("fk_subscriptions_clubs_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ClubId)
                    .HasColumnName("club_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExpireAt).HasColumnName("expireAt");

                entity.Property(e => e.Payment).HasColumnName("payment");

                entity.HasOne(d => d.Club)
                    .WithMany(p => p.Subscription)
                    .HasForeignKey(d => d.ClubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_subscriptions_clubs");
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.ToTable("token", "clubplay");

                entity.HasIndex(e => e.ClubId)
                    .HasName("fk_token_club1_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ClubId)
                    .HasColumnName("club_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ExpireAt).HasColumnName("expireAt");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.Club)
                    .WithMany(p => p.Token)
                    .HasForeignKey(d => d.ClubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_token_club1");
            });
        }
    }
}
