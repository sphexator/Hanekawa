﻿// <auto-generated />
using System;
using Hanekawa.Addons.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Addons.Database.Migrations
{
    [DbContext(typeof(DbService))]
    [Migration("20190224222819_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Account.Account", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("UserId");

                    b.Property<bool>("Active");

                    b.Property<DateTime>("ChannelVoiceTime");

                    b.Property<int>("Class");

                    b.Property<int>("Credit");

                    b.Property<int>("CreditSpecial");

                    b.Property<DateTime>("DailyCredit");

                    b.Property<int>("Exp");

                    b.Property<DateTime?>("FirstMessage");

                    b.Property<int>("GameKillAmount");

                    b.Property<DateTime>("LastMessage");

                    b.Property<int>("Level");

                    b.Property<string>("ProfilePic");

                    b.Property<int>("Rep");

                    b.Property<DateTime>("RepCooldown");

                    b.Property<int>("Sessions");

                    b.Property<int>("StarGiven");

                    b.Property<int>("StarReceived");

                    b.Property<long>("StatMessages");

                    b.Property<TimeSpan>("StatVoiceTime");

                    b.Property<int>("TotalExp");

                    b.Property<DateTime>("VoiceExpTime");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Account.AccountGlobal", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Credit");

                    b.Property<int>("Exp");

                    b.Property<int>("Level");

                    b.Property<int>("Rep");

                    b.Property<int>("StarGive");

                    b.Property<int>("StarReceive");

                    b.Property<int>("TotalExp");

                    b.HasKey("UserId");

                    b.ToTable("AccountGlobals");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Account.Inventory", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("UserId");

                    b.Property<int>("ItemId");

                    b.Property<int>("Amount");

                    b.HasKey("GuildId", "UserId", "ItemId");

                    b.ToTable("Inventories");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Account.Item", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("DateAdded");

                    b.Property<long>("GuildId");

                    b.Property<long>("Role");

                    b.HasKey("Id");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementDifficulty", b =>
                {
                    b.Property<int>("DifficultyId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("DifficultyId");

                    b.ToTable("AchievementDifficulties");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementMeta", b =>
                {
                    b.Property<int>("AchievementId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AchievementNameId");

                    b.Property<string>("Description");

                    b.Property<int>("DifficultyId");

                    b.Property<bool>("Global");

                    b.Property<bool>("Hidden");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<bool>("Once");

                    b.Property<int>("Points");

                    b.Property<int>("Requirement");

                    b.Property<int?>("Reward");

                    b.Property<int>("TypeId");

                    b.HasKey("AchievementId");

                    b.HasIndex("AchievementNameId");

                    b.HasIndex("DifficultyId");

                    b.HasIndex("TypeId");

                    b.ToTable("Achievements");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementName", b =>
                {
                    b.Property<int>("AchievementNameId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.Property<bool>("Stackable");

                    b.HasKey("AchievementNameId");

                    b.ToTable("AchievementNames");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementTracker", b =>
                {
                    b.Property<int>("Type");

                    b.Property<long>("UserId");

                    b.Property<int>("Count");

                    b.HasKey("Type", "UserId");

                    b.ToTable("AchievementTrackers");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementType", b =>
                {
                    b.Property<int>("TypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("TypeId");

                    b.ToTable("AchievementTypes");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementUnlock", b =>
                {
                    b.Property<int>("AchievementId");

                    b.Property<long>("UserId");

                    b.Property<int>("TypeId");

                    b.HasKey("AchievementId", "UserId");

                    b.ToTable("AchievementUnlocks");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Administration.Blacklist", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Reason");

                    b.Property<long>("ResponsibleUser");

                    b.Property<DateTimeOffset?>("Unban");

                    b.HasKey("GuildId");

                    b.ToTable("Blacklists");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Administration.EventSchedule", b =>
                {
                    b.Property<int>("Id");

                    b.Property<long>("GuildId");

                    b.Property<string>("Description");

                    b.Property<long?>("DesignerClaim");

                    b.Property<long>("Host");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<bool>("Posted");

                    b.Property<DateTime>("Time");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("EventSchedules");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Administration.WhitelistDesign", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("UserId");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("WhitelistDesigns");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Administration.WhitelistEvent", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("UserId");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("WhitelistEvents");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.BoardConfig.Board", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("MessageId");

                    b.Property<DateTimeOffset?>("Boarded");

                    b.Property<int>("StarAmount");

                    b.Property<long>("UserId");

                    b.HasKey("GuildId", "MessageId");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.BotGame.GameClass", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChanceAvoid");

                    b.Property<int>("ChanceCrit");

                    b.Property<long>("LevelRequirement");

                    b.Property<double>("ModifierAvoidance");

                    b.Property<double>("ModifierCriticalChance");

                    b.Property<double>("ModifierDamage");

                    b.Property<double>("ModifierHealth");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("GameClasses");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.BotGame.GameConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DefaultDamage");

                    b.Property<int>("DefaultHealth");

                    b.HasKey("Id");

                    b.ToTable("GameConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.BotGame.GameEnemy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClassId");

                    b.Property<int>("CreditGain");

                    b.Property<int>("Damage");

                    b.Property<bool>("Elite");

                    b.Property<int>("ExpGain");

                    b.Property<int>("Health");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<bool>("Rare");

                    b.HasKey("Id");

                    b.ToTable("GameEnemies");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Club.ClubBlacklist", b =>
                {
                    b.Property<int>("ClubId");

                    b.Property<long>("GuildId");

                    b.Property<long>("BlackListUser");

                    b.Property<long>("IssuedUser");

                    b.Property<string>("Reason");

                    b.Property<DateTimeOffset>("Time");

                    b.HasKey("ClubId", "GuildId", "BlackListUser");

                    b.ToTable("ClubBlacklists");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Club.ClubInformation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AdMessage");

                    b.Property<bool>("AutoAdd");

                    b.Property<long?>("Channel");

                    b.Property<DateTimeOffset>("CreationDate");

                    b.Property<string>("Description");

                    b.Property<long>("GuildId");

                    b.Property<string>("IconUrl");

                    b.Property<string>("ImageUrl");

                    b.Property<long>("LeaderId");

                    b.Property<string>("Name");

                    b.Property<bool>("Public");

                    b.Property<long?>("Role");

                    b.HasKey("Id");

                    b.ToTable("ClubInfos");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Club.ClubUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClubId");

                    b.Property<long>("GuildId");

                    b.Property<DateTimeOffset>("JoinDate");

                    b.Property<int>("Rank");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("ClubPlayers");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.EventPayout", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("UserId");

                    b.Property<int>("Amount");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("EventPayouts");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.AdminConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("EmoteCountFilter");

                    b.Property<bool>("FilterAllInv");

                    b.Property<bool>("FilterInvites");

                    b.Property<int?>("FilterMsgLength");

                    b.Property<bool>("FilterUrls");

                    b.Property<bool>("IgnoreAllChannels");

                    b.Property<int?>("MentionCountFilter");

                    b.Property<long?>("MuteRole");

                    b.HasKey("GuildId");

                    b.ToTable("AdminConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.BoardConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("Channel");

                    b.Property<string>("Emote");

                    b.HasKey("GuildId");

                    b.ToTable("BoardConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.ChannelConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("DesignChannel");

                    b.Property<long?>("EventChannel");

                    b.Property<long?>("EventSchedulerChannel");

                    b.Property<long?>("ModChannel");

                    b.Property<long?>("QuestionAndAnswerChannel");

                    b.Property<long?>("ReportChannel");

                    b.HasKey("GuildId");

                    b.ToTable("ChannelConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.ClubConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AdvertisementChannel");

                    b.Property<bool>("AutoPrune");

                    b.Property<long?>("ChannelCategory");

                    b.Property<int>("ChannelRequiredAmount");

                    b.Property<int>("ChannelRequiredLevel");

                    b.Property<bool>("EnableVoiceChannel");

                    b.Property<bool>("RoleEnabled");

                    b.HasKey("GuildId");

                    b.ToTable("ClubConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.CurrencyConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CurrencyName");

                    b.Property<string>("CurrencySign");

                    b.Property<bool>("EmoteCurrency")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("SpecialCurrencyName");

                    b.Property<string>("SpecialCurrencySign");

                    b.Property<bool>("SpecialEmoteCurrency")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.HasKey("GuildId");

                    b.ToTable("CurrencyConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.LevelConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ExpMultiplier");

                    b.Property<bool>("StackLvlRoles");

                    b.HasKey("GuildId");

                    b.ToTable("LevelConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.LoggingConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("LogAutoMod");

                    b.Property<long?>("LogAvi");

                    b.Property<long?>("LogBan");

                    b.Property<long?>("LogJoin");

                    b.Property<long?>("LogMsg");

                    b.Property<long?>("LogWarn");

                    b.HasKey("GuildId");

                    b.ToTable("LoggingConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.SuggestionConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("Channel");

                    b.Property<string>("EmoteNo");

                    b.Property<string>("EmoteYes");

                    b.HasKey("GuildId");

                    b.ToTable("SuggestionConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.Guild.WelcomeConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AutoDelOnLeave");

                    b.Property<bool>("Banner");

                    b.Property<long?>("Channel");

                    b.Property<int>("Limit");

                    b.Property<string>("Message");

                    b.Property<TimeSpan?>("TimeToDelete");

                    b.HasKey("GuildId");

                    b.ToTable("WelcomeConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.GuildConfig", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AnimeAirChannel");

                    b.Property<bool>("AutomaticEventSchedule");

                    b.Property<int>("EmbedColor");

                    b.Property<long?>("MusicChannel");

                    b.Property<long?>("MusicVcChannel");

                    b.Property<string>("Prefix");

                    b.Property<bool>("Premium")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.IgnoreChannel", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("IgnoreChannels");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.LevelExpEvent", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ChannelId");

                    b.Property<long?>("MessageId");

                    b.Property<int>("Multiplier");

                    b.Property<DateTime>("Time");

                    b.HasKey("GuildId");

                    b.ToTable("LevelExpEvents");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.LevelExpReduction", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.Property<bool>("Category");

                    b.Property<bool>("Channel");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("LevelExpReductions");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.LevelReward", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<int>("Level");

                    b.Property<long>("Role");

                    b.Property<bool>("Stackable");

                    b.HasKey("GuildId", "Level");

                    b.ToTable("LevelRewards");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.LootChannel", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("LootChannels");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.NudeServiceChannel", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.Property<bool>("InHouse");

                    b.Property<int>("Tolerance");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("NudeServiceChannels");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.SelfAssignAbleRole", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("RoleId");

                    b.Property<bool>("Exclusive");

                    b.HasKey("GuildId", "RoleId");

                    b.ToTable("SelfAssignAbleRoles");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.SingleNudeServiceChannel", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.Property<bool>("InHouse");

                    b.Property<int?>("Level");

                    b.Property<int?>("Tolerance");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("SingleNudeServiceChannels");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.SpamIgnore", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("SpamIgnores");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.UrlFilter", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("UrlFilters");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Config.WelcomeBanner", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("UploadTimeOffset");

                    b.Property<long>("Uploader");

                    b.Property<string>("Url");

                    b.HasKey("GuildId", "Id");

                    b.ToTable("WelcomeBanners");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Moderation.ModLog", b =>
                {
                    b.Property<int>("Id");

                    b.Property<long>("GuildId");

                    b.Property<string>("Action");

                    b.Property<DateTime>("Date");

                    b.Property<long>("MessageId");

                    b.Property<long?>("ModId");

                    b.Property<string>("Response");

                    b.Property<long>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("ModLogs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Moderation.MuteTimer", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("GuildId");

                    b.Property<DateTime>("Time");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("MuteTimers");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Moderation.QuestionAndAnswer", b =>
                {
                    b.Property<int>("Id");

                    b.Property<long>("GuildId");

                    b.Property<DateTime>("Date");

                    b.Property<long?>("MessageId");

                    b.Property<string>("Response");

                    b.Property<long?>("ResponseUser");

                    b.Property<bool>("Status");

                    b.Property<long>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("QuestionAndAnswers");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Moderation.Report", b =>
                {
                    b.Property<int>("Id");

                    b.Property<long>("GuildId");

                    b.Property<string>("Attachment");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Message");

                    b.Property<long?>("MessageId");

                    b.Property<bool>("Status");

                    b.Property<long>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Moderation.Suggestion", b =>
                {
                    b.Property<int>("Id");

                    b.Property<long>("GuildId");

                    b.Property<DateTime>("Date");

                    b.Property<long?>("MessageId");

                    b.Property<string>("Response");

                    b.Property<long?>("ResponseUser");

                    b.Property<bool>("Status");

                    b.Property<long>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("Suggestions");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Moderation.Warn", b =>
                {
                    b.Property<int>("Id");

                    b.Property<long>("GuildId");

                    b.Property<decimal>("Moderator")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<TimeSpan?>("MuteTimer");

                    b.Property<string>("Reason");

                    b.Property<DateTime>("Time");

                    b.Property<int>("Type");

                    b.Property<long>("UserId");

                    b.Property<bool>("Valid");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("Warns");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Profile.Background", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BackgroundUrl");

                    b.HasKey("Id");

                    b.ToTable("Backgrounds");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Profile.ProfileConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<float>("Height");

                    b.Property<string>("Name");

                    b.Property<float>("NameWidth");

                    b.Property<string>("Value");

                    b.Property<float>("ValueWidth");

                    b.HasKey("Id");

                    b.ToTable("ProfileConfigs");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Stores.ServerStore", b =>
                {
                    b.Property<long>("GuildId");

                    b.Property<long>("RoleId");

                    b.Property<int>("Price");

                    b.Property<bool>("SpecialCredit");

                    b.HasKey("GuildId", "RoleId");

                    b.ToTable("ServerStores");
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementMeta", b =>
                {
                    b.HasOne("Hanekawa.Addons.Database.Tables.Achievement.AchievementName", "AchievementName")
                        .WithMany()
                        .HasForeignKey("AchievementNameId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hanekawa.Addons.Database.Tables.Achievement.AchievementDifficulty", "AchievementDifficulty")
                        .WithMany()
                        .HasForeignKey("DifficultyId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Hanekawa.Addons.Database.Tables.Achievement.AchievementType", "AchievementType")
                        .WithMany()
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Hanekawa.Addons.Database.Tables.Achievement.AchievementUnlock", b =>
                {
                    b.HasOne("Hanekawa.Addons.Database.Tables.Achievement.AchievementMeta", "Achievement")
                        .WithMany()
                        .HasForeignKey("AchievementId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
