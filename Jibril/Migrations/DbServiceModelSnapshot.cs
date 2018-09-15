﻿// <auto-generated />
using System;
using Hanekawa.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hanekawa.Migrations
{
    [DbContext(typeof(DbService))]
    partial class DbServiceModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Account", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<bool>("Active");

                    b.Property<DateTime>("ChannelVoiceTime");

                    b.Property<int>("Class")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(1);

                    b.Property<uint>("Credit")
                        .HasMaxLength(999);

                    b.Property<uint>("CreditSpecial")
                        .HasMaxLength(999);

                    b.Property<ulong?>("CustomRoleId");

                    b.Property<DateTime>("DailyCredit");

                    b.Property<uint>("Exp")
                        .HasMaxLength(999);

                    b.Property<DateTime?>("FirstMessage");

                    b.Property<uint>("GameKillAmount")
                        .HasMaxLength(999);

                    b.Property<DateTime>("LastMessage");

                    b.Property<uint>("Level")
                        .HasMaxLength(999);

                    b.Property<uint>("MvpCounter")
                        .HasMaxLength(999);

                    b.Property<bool>("MvpIgnore");

                    b.Property<bool>("MvpImmunity");

                    b.Property<string>("ProfilePic");

                    b.Property<uint>("Rep")
                        .HasMaxLength(999);

                    b.Property<DateTime>("RepCooldown");

                    b.Property<uint>("Sessions")
                        .HasMaxLength(999);

                    b.Property<uint>("StarGiven")
                        .HasMaxLength(999);

                    b.Property<uint>("StarReceived")
                        .HasMaxLength(999);

                    b.Property<ulong>("StatMessages")
                        .HasMaxLength(999);

                    b.Property<TimeSpan>("StatVoiceTime");

                    b.Property<uint>("TotalExp")
                        .HasMaxLength(999);

                    b.Property<DateTime>("VoiceExpTime");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.AccountGlobal", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Credit");

                    b.Property<uint>("Exp")
                        .HasMaxLength(999);

                    b.Property<uint>("Level")
                        .HasMaxLength(999);

                    b.Property<uint>("Rep")
                        .HasMaxLength(999);

                    b.Property<uint>("StarGive");

                    b.Property<uint>("StarReceive");

                    b.Property<uint>("TotalExp")
                        .HasMaxLength(999);

                    b.HasKey("UserId");

                    b.ToTable("AccountGlobals");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Blacklist", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Reason");

                    b.Property<ulong>("ResponsibleUser");

                    b.Property<DateTimeOffset?>("Unban");

                    b.HasKey("GuildId");

                    b.ToTable("Blacklists");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Board", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("MessageId");

                    b.Property<DateTimeOffset?>("Boarded");

                    b.Property<uint>("StarAmount");

                    b.Property<ulong>("UserId");

                    b.HasKey("GuildId", "MessageId");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.ClubBlacklist", b =>
                {
                    b.Property<int>("ClubId");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("BlackListUser");

                    b.Property<ulong>("IssuedUser");

                    b.Property<string>("Reason");

                    b.Property<DateTimeOffset>("Time");

                    b.HasKey("ClubId", "GuildId", "BlackListUser");

                    b.ToTable("ClubBlacklists");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.ClubInfo", b =>
                {
                    b.Property<int>("Id");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("Leader");

                    b.Property<ulong?>("AdMessage");

                    b.Property<bool>("AutoAdd");

                    b.Property<ulong?>("Channel");

                    b.Property<DateTimeOffset>("CreationDate");

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<DateTime?>("InactiveTime");

                    b.Property<string>("Name");

                    b.Property<bool>("Public");

                    b.Property<ulong?>("RoleId");

                    b.HasKey("Id", "GuildId", "Leader");

                    b.ToTable("ClubInfos");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.ClubPlayer", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("ClubId");

                    b.Property<ulong>("GuildId");

                    b.Property<DateTimeOffset>("JoinDate");

                    b.Property<uint>("Rank");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id", "ClubId", "GuildId");

                    b.ToTable("ClubPlayers");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.EventPayout", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Amount");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("EventPayouts");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.EventSchedule", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<int>("Id");

                    b.Property<string>("Description");

                    b.Property<ulong>("Host");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<DateTime>("Time");

                    b.HasKey("GuildId", "Id");

                    b.ToTable("EventSchedules");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.GameClass", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChanceAvoid");

                    b.Property<int>("ChanceCrit");

                    b.Property<uint>("LevelRequirement");

                    b.Property<double>("ModifierAvoidance");

                    b.Property<double>("ModifierCriticalChance");

                    b.Property<double>("ModifierDamage");

                    b.Property<double>("ModifierHealth");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("GameClasses");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.GameConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DefaultDamage")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(10);

                    b.Property<int>("DefaultHealth")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(10);

                    b.HasKey("Id");

                    b.ToTable("GameConfigs");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.GameEnemy", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClassId");

                    b.Property<uint>("CreditGain");

                    b.Property<uint>("Damage");

                    b.Property<bool>("Elite")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<uint>("ExpGain");

                    b.Property<uint>("Health");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<bool>("Rare")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.HasKey("Id");

                    b.ToTable("GameEnemies");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.GuildConfig", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong?>("AnimeAirChannel");

                    b.Property<bool>("AutomaticEventSchedule");

                    b.Property<ulong?>("BoardChannel");

                    b.Property<string>("BoardEmote");

                    b.Property<ulong?>("ClubAdvertisementChannel");

                    b.Property<bool>("ClubAutoPrune");

                    b.Property<ulong?>("ClubChannelCategory");

                    b.Property<uint>("ClubChannelRequiredAmount");

                    b.Property<uint>("ClubChannelRequiredLevel");

                    b.Property<bool>("ClubEnableVoiceChannel");

                    b.Property<string>("CurrencyName");

                    b.Property<string>("CurrencySign");

                    b.Property<bool>("EmoteCurrency")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<ulong?>("EventChannel");

                    b.Property<ulong?>("EventSchedulerChannel");

                    b.Property<uint>("ExpMultiplier");

                    b.Property<bool>("FilterAllInv");

                    b.Property<bool>("FilterInvites");

                    b.Property<uint?>("FilterMsgLength");

                    b.Property<bool>("FilterUrls");

                    b.Property<bool>("IgnoreAllChannels");

                    b.Property<ulong?>("LogAvi");

                    b.Property<ulong?>("LogBan");

                    b.Property<ulong?>("LogJoin");

                    b.Property<ulong?>("LogMsg");

                    b.Property<ulong?>("LogWarn");

                    b.Property<ulong?>("ModChannel");

                    b.Property<ulong?>("MusicChannel");

                    b.Property<ulong?>("MusicVcChannel");

                    b.Property<ulong?>("MuteRole");

                    b.Property<string>("Prefix");

                    b.Property<bool>("Premium")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<ulong?>("ReportChannel");

                    b.Property<string>("SpecialCurrencyName");

                    b.Property<string>("SpecialCurrencySign");

                    b.Property<bool>("SpecialEmoteCurrency")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("StackLvlRoles");

                    b.Property<ulong?>("SuggestionChannel");

                    b.Property<string>("SuggestionEmoteNo");

                    b.Property<string>("SuggestionEmoteYes");

                    b.Property<bool>("WelcomeBanner");

                    b.Property<ulong?>("WelcomeChannel");

                    b.Property<TimeSpan?>("WelcomeDelete");

                    b.Property<uint>("WelcomeLimit");

                    b.Property<string>("WelcomeMessage");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.GuildInfo", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FaqOne");

                    b.Property<ulong>("FaqOneMessageId");

                    b.Property<string>("FaqTwo");

                    b.Property<ulong>("FaqTwoMessageId");

                    b.Property<ulong>("LevelMessageId");

                    b.Property<ulong>("LinkMessageId");

                    b.Property<ulong>("OtherChannelId");

                    b.Property<ulong>("RuleChannelId");

                    b.Property<ulong>("RuleMessageId");

                    b.Property<string>("Rules");

                    b.Property<ulong>("StaffMessageId");

                    b.HasKey("GuildId");

                    b.ToTable("GuildInfos");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.GuildInfoLink", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Link");

                    b.HasKey("GuildId");

                    b.ToTable("GuildInfoLinks");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.HungerGameConfig", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Live");

                    b.Property<ulong>("MessageId");

                    b.Property<uint>("Round");

                    b.Property<bool>("SignupStage");

                    b.Property<DateTime>("SignupTime");

                    b.HasKey("GuildId");

                    b.ToTable("HungerGameConfigs");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.HungerGameDefault", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("UserId");

                    b.ToTable("HungerGameDefaults");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.HungerGameLive", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Axe");

                    b.Property<bool>("Bleeding");

                    b.Property<uint>("Bow");

                    b.Property<uint>("Fatigue");

                    b.Property<uint>("Food");

                    b.Property<uint>("Health");

                    b.Property<uint>("Hunger");

                    b.Property<string>("Name");

                    b.Property<uint>("Pistol");

                    b.Property<uint>("Sleep");

                    b.Property<uint>("Stamina");

                    b.Property<bool>("Status");

                    b.Property<uint>("Thirst");

                    b.Property<uint>("TotalWeapons");

                    b.Property<uint>("Water");

                    b.HasKey("UserId");

                    b.ToTable("HungerGameLives");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.IgnoreChannel", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("IgnoreChannels");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Inventory", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<uint>("Amount");

                    b.Property<uint>("ItemId");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("Inventories");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.InventoryGlobal", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Amount");

                    b.Property<uint>("ItemId");

                    b.HasKey("UserId");

                    b.ToTable("InventoryGlobals");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.LevelExpEvent", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong?>("ChannelId");

                    b.Property<ulong?>("MessageId");

                    b.Property<uint>("Multiplier");

                    b.Property<DateTime>("Time");

                    b.HasKey("GuildId");

                    b.ToTable("LevelExpEvents");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.LevelReward", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<uint>("Level");

                    b.Property<ulong>("Role");

                    b.Property<bool>("Stackable");

                    b.HasKey("GuildId", "Level");

                    b.ToTable("LevelRewards");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.LootChannel", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("LootChannels");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.ModLog", b =>
                {
                    b.Property<uint>("Id");

                    b.Property<ulong>("GuildId");

                    b.Property<string>("Action");

                    b.Property<DateTime>("Date");

                    b.Property<ulong>("MessageId");

                    b.Property<ulong?>("ModId");

                    b.Property<string>("Response");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("ModLogs");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.MuteTimer", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<DateTime>("Time");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("MuteTimers");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.NudeServiceChannel", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("ChannelId");

                    b.Property<uint>("Tolerance");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("NudeServiceChannels");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Report", b =>
                {
                    b.Property<uint>("Id");

                    b.Property<ulong>("GuildId");

                    b.Property<string>("Attachment");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Message");

                    b.Property<ulong?>("MessageId");

                    b.Property<bool>("Status");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Shop", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Item");

                    b.Property<uint>("Price");

                    b.Property<string>("ROle");

                    b.Property<ulong?>("RoleId");

                    b.HasKey("Id");

                    b.ToTable("Shops");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.ShopEvent", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Item");

                    b.Property<uint>("Price");

                    b.Property<uint?>("Stock");

                    b.HasKey("Id");

                    b.ToTable("ShopEvents");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Suggestion", b =>
                {
                    b.Property<uint>("Id");

                    b.Property<ulong>("GuildId");

                    b.Property<DateTime>("Date");

                    b.Property<ulong?>("MessageId");

                    b.Property<string>("Response");

                    b.Property<ulong?>("ResponseUser");

                    b.Property<bool>("Status");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id", "GuildId");

                    b.ToTable("Suggestions");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.Warn", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<int>("Id");

                    b.Property<ulong>("Moderator");

                    b.Property<TimeSpan?>("MuteTimer");

                    b.Property<string>("Reason");

                    b.Property<DateTime>("Time");

                    b.Property<int>("Type");

                    b.Property<ulong>("UserId");

                    b.Property<bool>("Valid");

                    b.HasKey("GuildId", "Id");

                    b.ToTable("Warns");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.WarnMsgLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author");

                    b.Property<string>("Message");

                    b.Property<ulong>("MsgId");

                    b.Property<DateTime>("Time");

                    b.Property<ulong>("UserId");

                    b.Property<int>("WarnId");

                    b.HasKey("Id");

                    b.ToTable("WarnMsgLogs");
                });

            modelBuilder.Entity("Hanekawa.Services.Entities.Tables.WelcomeBanner", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<int>("Id");

                    b.Property<DateTimeOffset>("UploadTimeOffset");

                    b.Property<ulong>("Uploader");

                    b.Property<string>("Url");

                    b.HasKey("GuildId", "Id");

                    b.ToTable("WelcomeBanners");
                });
#pragma warning restore 612, 618
        }
    }
}
