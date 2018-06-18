using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Jibril
{
    public partial class hanekawaContext : DbContext
    {
        public hanekawaContext()
        {
        }

        public hanekawaContext(DbContextOptions<hanekawaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Banlog> Banlog { get; set; }
        public virtual DbSet<Botconfig> Botconfig { get; set; }
        public virtual DbSet<Botinfo> Botinfo { get; set; }
        public virtual DbSet<Classes> Classes { get; set; }
        public virtual DbSet<Enemyidentity> Enemyidentity { get; set; }
        public virtual DbSet<Eventshop> Eventshop { get; set; }
        public virtual DbSet<Exp> Exp { get; set; }
        public virtual DbSet<Fleet> Fleet { get; set; }
        public virtual DbSet<Fleetinfo> Fleetinfo { get; set; }
        public virtual DbSet<Guildinfo> Guildinfo { get; set; }
        public virtual DbSet<Hungergame> Hungergame { get; set; }
        public virtual DbSet<Hungergameconfig> Hungergameconfig { get; set; }
        public virtual DbSet<Hungergamedefault> Hungergamedefault { get; set; }
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<Modlog> Modlog { get; set; }
        public virtual DbSet<Mute> Mute { get; set; }
        public virtual DbSet<Muteconfig> Muteconfig { get; set; }
        public virtual DbSet<Reaction> Reaction { get; set; }
        public virtual DbSet<Shipgame> Shipgame { get; set; }
        public virtual DbSet<Shop> Shop { get; set; }
        public virtual DbSet<Suggestion> Suggestion { get; set; }
        public virtual DbSet<Waifu> Waifu { get; set; }
        public virtual DbSet<Warnings> Warnings { get; set; }
        public virtual DbSet<Warnmsglog> Warnmsglog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(DbInfo.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Banlog>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("banlog");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Counter).HasColumnName("counter");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.UnbanDate)
                    .HasColumnName("unbanDate")
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Botconfig>(entity =>
            {
                entity.HasKey(e => e.Guild);

                entity.ToTable("botconfig");

                entity.Property(e => e.Guild)
                    .HasColumnName("guild")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Announcement)
                    .HasColumnName("announcement")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Board)
                    .HasColumnName("board")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Botgame)
                    .HasColumnName("botgame")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Botspam)
                    .HasColumnName("botspam")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Log)
                    .HasColumnName("log")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Modlog)
                    .HasColumnName("modlog")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Msglog)
                    .HasColumnName("msglog")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Raid)
                    .HasColumnName("raid")
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.RecruitmentStatus)
                    .HasColumnName("recruitmentStatus")
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Rules)
                    .HasColumnName("rules")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Suggestion)
                    .HasColumnName("suggestion")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Welcome)
                    .HasColumnName("welcome")
                    .HasColumnType("varchar(99)");
            });

            modelBuilder.Entity<Botinfo>(entity =>
            {
                entity.HasKey(e => e.Guild);

                entity.ToTable("botinfo");

                entity.Property(e => e.Guild)
                    .HasColumnName("guild")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.About)
                    .HasColumnName("about")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Invlink)
                    .HasColumnName("invlink")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Owner)
                    .HasColumnName("owner")
                    .HasColumnType("varchar(45)");
            });

            modelBuilder.Entity<Classes>(entity =>
            {
                entity.HasKey(e => e.Level);

                entity.ToTable("classes");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("int(55)");

                entity.Property(e => e.Class)
                    .HasColumnName("class")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasColumnType("bigint(99)");
            });

            modelBuilder.Entity<Enemyidentity>(entity =>
            {
                entity.ToTable("enemyidentity");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CurrencyGain)
                    .HasColumnName("currencyGain")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Damage)
                    .HasColumnName("damage")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EnemyClass)
                    .HasColumnName("enemyClass")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.EnemyName)
                    .HasColumnName("enemyName")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.ExpGain)
                    .HasColumnName("expGain")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Health)
                    .HasColumnName("health")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ImageName)
                    .HasColumnName("imageName")
                    .HasColumnType("varchar(99)");
            });

            modelBuilder.Entity<Eventshop>(entity =>
            {
                entity.ToTable("eventshop");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Item)
                    .HasColumnName("item")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Stock)
                    .HasColumnName("stock")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<Exp>(entity =>
            {
                entity.ToTable("exp");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(99)");

                entity.Property(e => e.Cooldown)
                    .HasColumnName("cooldown")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.Daily)
                    .HasColumnName("daily")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.EventTokens)
                    .HasColumnName("event_tokens")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Firstmsg)
                    .HasColumnName("firstmsg")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.FleetName)
                    .HasColumnName("fleetName")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("'o'");

                entity.Property(e => e.Hasrole)
                    .HasColumnName("hasrole")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("'no'");

                entity.Property(e => e.Joindate)
                    .HasColumnName("joindate")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.Lastmsg)
                    .HasColumnName("lastmsg")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.MvpCounter)
                    .HasColumnName("mvpCounter")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Mvpignore)
                    .HasColumnName("mvpignore")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Mvpimmunity)
                    .HasColumnName("mvpimmunity")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Profilepic)
                    .HasColumnName("profilepic")
                    .HasColumnType("varchar(99)")
                    .HasDefaultValueSql("'o'");

                entity.Property(e => e.Rep)
                    .HasColumnName("rep")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Repcd)
                    .HasColumnName("repcd")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.ShipClass)
                    .HasColumnName("shipClass")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("'ShipGirl'");

                entity.Property(e => e.Tokens)
                    .HasColumnName("tokens")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TotalXp)
                    .HasColumnName("total_xp")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Toxicityavg)
                    .HasColumnName("toxicityavg")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Toxicitymsgcount)
                    .HasColumnName("toxicitymsgcount")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Toxicityvalue)
                    .HasColumnName("toxicityvalue")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.VoiceTimer)
                    .HasColumnName("voice_timer")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.Xp)
                    .HasColumnName("xp")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'1'");
            });

            modelBuilder.Entity<Fleet>(entity =>
            {
                entity.ToTable("fleet");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(99)");

                entity.Property(e => e.Clubid)
                    .HasColumnName("clubid")
                    .HasColumnType("int(99)");

                entity.Property(e => e.Clubname)
                    .HasColumnName("clubname")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Joindate)
                    .HasColumnName("joindate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Rank)
                    .HasColumnName("rank")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Userid).HasColumnName("userid");
            });

            modelBuilder.Entity<Fleetinfo>(entity =>
            {
                entity.ToTable("fleetinfo");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(99)");

                entity.Property(e => e.Channel)
                    .HasColumnName("channel")
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Channelid)
                    .HasColumnName("channelid")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Clubname)
                    .HasColumnName("clubname")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Creationdate)
                    .HasColumnName("creationdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Leader).HasColumnName("leader");

                entity.Property(e => e.Members)
                    .HasColumnName("members")
                    .HasColumnType("int(99)");

                entity.Property(e => e.Roleid)
                    .HasColumnName("roleid")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<Guildinfo>(entity =>
            {
                entity.HasKey(e => e.Guild);

                entity.ToTable("guildinfo");

                entity.Property(e => e.Guild)
                    .HasColumnName("guild")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Faq)
                    .HasColumnName("faq")
                    .HasColumnType("longtext");

                entity.Property(e => e.Faq2)
                    .HasColumnName("faq2")
                    .HasColumnType("longtext");

                entity.Property(e => e.Faq2msgid).HasColumnName("faq2msgid");

                entity.Property(e => e.Faqmsgid).HasColumnName("faqmsgid");

                entity.Property(e => e.Rules)
                    .HasColumnName("rules")
                    .HasColumnType("longtext");

                entity.Property(e => e.Rulesmsgid).HasColumnName("rulesmsgid");

                entity.Property(e => e.Staffmsgid).HasColumnName("staffmsgid");
            });

            modelBuilder.Entity<Hungergame>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.Userid });

                entity.ToTable("hungergame");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(99)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Userid).HasColumnName("userid");

                entity.Property(e => e.Arrows)
                    .HasColumnName("arrows")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Axe)
                    .HasColumnName("axe")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Bandages)
                    .HasColumnName("bandages")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Beans)
                    .HasColumnName("beans")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Bleeding)
                    .HasColumnName("bleeding")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Bow)
                    .HasColumnName("bow")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Bullets)
                    .HasColumnName("bullets")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Coke)
                    .HasColumnName("coke")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.DamageTaken)
                    .HasColumnName("damageTaken")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Fatigue)
                    .HasColumnName("fatigue")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Fish)
                    .HasColumnName("fish")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Health)
                    .HasColumnName("health")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'100'");

                entity.Property(e => e.Hunger)
                    .HasColumnName("hunger")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Mountaindew)
                    .HasColumnName("mountaindew")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Pasta)
                    .HasColumnName("pasta")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Pistol)
                    .HasColumnName("pistol")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Ramen)
                    .HasColumnName("ramen")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Redbull)
                    .HasColumnName("redbull")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Sleep)
                    .HasColumnName("sleep")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Stamina)
                    .HasColumnName("stamina")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'100'");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Thirst)
                    .HasColumnName("thirst")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Totaldrink)
                    .HasColumnName("totaldrink")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Totalfood)
                    .HasColumnName("totalfood")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Totalweapons)
                    .HasColumnName("totalweapons")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Trap)
                    .HasColumnName("trap")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Water)
                    .HasColumnName("water")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<Hungergameconfig>(entity =>
            {
                entity.HasKey(e => e.Live);

                entity.ToTable("hungergameconfig");

                entity.Property(e => e.Live)
                    .HasColumnName("live")
                    .HasColumnType("tinyint(4)");

                entity.Property(e => e.Guild).HasColumnName("guild");

                entity.Property(e => e.MsgId).HasColumnName("msgId");

                entity.Property(e => e.Round)
                    .HasColumnName("round")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SignupDuration)
                    .HasColumnName("signupDuration")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");

                entity.Property(e => e.Signupstage)
                    .HasColumnName("signupstage")
                    .HasColumnType("tinyint(4)");
            });

            modelBuilder.Entity<Hungergamedefault>(entity =>
            {
                entity.HasKey(e => e.Userid);

                entity.ToTable("hungergamedefault");

                entity.Property(e => e.Userid).HasColumnName("userid");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(45)");
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("inventory");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Customrole)
                    .HasColumnName("customrole")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.DamageBoost)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Gift)
                    .HasColumnName("gift")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.RepairKit)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Shield)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(99)");
            });

            modelBuilder.Entity<Modlog>(entity =>
            {
                entity.ToTable("modlog");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Msgid)
                    .HasColumnName("msgid")
                    .HasColumnType("varchar(95)");

                entity.Property(e => e.Responduser)
                    .HasColumnName("responduser")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Response)
                    .HasColumnName("response")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(95)");
            });

            modelBuilder.Entity<Mute>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("mute");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Guildid).HasColumnName("guildid");

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0001-01-01 00:00:00'");
            });

            modelBuilder.Entity<Muteconfig>(entity =>
            {
                entity.HasKey(e => e.Guildid);

                entity.ToTable("muteconfig");

                entity.Property(e => e.Guildid)
                    .HasColumnName("guildid")
                    .HasColumnType("bigint(99)");

                entity.Property(e => e.Muterole)
                    .IsRequired()
                    .HasColumnName("muterole")
                    .HasColumnType("varchar(45)");
            });

            modelBuilder.Entity<Reaction>(entity =>
            {
                entity.ToTable("reaction");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Chid)
                    .HasColumnName("chid")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Counter)
                    .HasColumnName("counter")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Msgid)
                    .HasColumnName("msgid")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Sent)
                    .HasColumnName("sent")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("'no'");
            });

            modelBuilder.Entity<Shipgame>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("shipgame");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Combatstatus)
                    .HasColumnName("combatstatus")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Damagetaken)
                    .HasColumnName("damagetaken")
                    .HasColumnType("int(55)");

                entity.Property(e => e.EnemyDamageTaken)
                    .HasColumnName("enemyDamageTaken")
                    .HasColumnType("int(55)");

                entity.Property(e => e.Enemyhealth)
                    .HasColumnName("enemyhealth")
                    .HasColumnType("int(55)");

                entity.Property(e => e.Enemyid)
                    .HasColumnName("enemyid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Health)
                    .HasColumnName("health")
                    .HasColumnType("int(55)");

                entity.Property(e => e.KillAmount)
                    .HasColumnName("killAmount")
                    .HasColumnType("int(99)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<Shop>(entity =>
            {
                entity.ToTable("shop");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Item)
                    .HasColumnName("item")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Suggestion>(entity =>
            {
                entity.ToTable("suggestion");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Msgid)
                    .HasColumnName("msgid")
                    .HasColumnType("varchar(95)");

                entity.Property(e => e.Responduser)
                    .HasColumnName("responduser")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Response)
                    .HasColumnName("response")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(95)");
            });

            modelBuilder.Entity<Waifu>(entity =>
            {
                entity.HasKey(e => e.User);

                entity.ToTable("waifu");

                entity.Property(e => e.User).HasColumnName("user");

                entity.Property(e => e.Claim).HasColumnName("claim");

                entity.Property(e => e.Claimname)
                    .HasColumnName("claimname")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(99)");

                entity.Property(e => e.Rank)
                    .HasColumnName("rank")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Timer)
                    .HasColumnName("timer")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Warnings>(entity =>
            {
                entity.ToTable("warnings");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(55)");

                entity.Property(e => e.TotalWarnings)
                    .HasColumnName("total_warnings")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Warnings1)
                    .HasColumnName("warnings")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<Warnmsglog>(entity =>
            {
                entity.HasKey(e => e.Warnmsglog1);

                entity.ToTable("warnmsglog");

                entity.Property(e => e.Warnmsglog1)
                    .HasColumnName("Warnmsglog")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Author)
                    .HasColumnName("author")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Datetime)
                    .HasColumnName("datetime")
                    .HasColumnType("datetime");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(99)");

                entity.Property(e => e.Msg)
                    .HasColumnName("msg")
                    .HasColumnType("longtext");

                entity.Property(e => e.Msgid).HasColumnName("msgid");

                entity.Property(e => e.Userid).HasColumnName("userid");
            });
        }
    }

    public class DbInfo
    {
        public static string ConnectionString { get; private set; }

        public DbInfo(IConfiguration config)
        {
            var config1 = config;

            ConnectionString = config1["connectionString"];
        }
    }
}
