﻿// <auto-generated />

using System;
using Hanekawa.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    [DbContext(typeof(DbService))]
    [Migration("20180802200735_botgameUpdate")]
    partial class botgameUpdate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Account", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<bool>("Active");

                    b.Property<DateTime>("ChannelVoiceTime");

                    b.Property<string>("Class");

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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.AccountGlobal", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Exp")
                        .HasMaxLength(999);

                    b.Property<uint>("Level")
                        .HasMaxLength(999);

                    b.Property<uint>("Rep")
                        .HasMaxLength(999);

                    b.Property<uint>("TotalExp")
                        .HasMaxLength(999);

                    b.HasKey("UserId");

                    b.ToTable("AccountGlobals");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Board", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("MessageId");

                    b.Property<DateTimeOffset>("Boarded");

                    b.Property<uint>("StarAmount");

                    b.Property<ulong>("UserId");

                    b.HasKey("GuildId", "MessageId");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.ClubInfo", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong?>("Channel");

                    b.Property<DateTime>("CreationDate");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("Leader");

                    b.Property<string>("Name");

                    b.Property<ulong>("RoleId");

                    b.HasKey("Id");

                    b.ToTable("ClubInfos");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.ClubPlayer", b =>
                {
                    b.Property<uint>("ClubId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("JoinDate");

                    b.Property<string>("Name");

                    b.Property<uint>("Rank");

                    b.Property<ulong>("UserId");

                    b.HasKey("ClubId");

                    b.ToTable("ClubPlayers");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.GameClass", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChanceAvoid");

                    b.Property<int>("ChanceCrit");

                    b.Property<double>("ModifierAvoidance");

                    b.Property<double>("ModifierCriticalChance");

                    b.Property<double>("ModifierDamage");

                    b.Property<double>("ModifierHealth");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("GameClasses");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.GameConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DefaultDamage");

                    b.Property<int>("DefaultHealth");

                    b.HasKey("Id");

                    b.ToTable("GameConfigs");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.GameEnemy", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Class");

                    b.Property<uint>("CreditGain");

                    b.Property<uint>("Damage");

                    b.Property<bool>("Elite");

                    b.Property<uint>("ExpGain");

                    b.Property<uint>("Health");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<bool>("Rare");

                    b.HasKey("Id");

                    b.ToTable("GameEnemies");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.GuildConfig", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong?>("BoardChannel");

                    b.Property<ulong?>("BoardEmote");

                    b.Property<ulong?>("EventChannel");

                    b.Property<ulong?>("EventSchedulerChannel");

                    b.Property<uint>("ExpMultiplier");

                    b.Property<bool>("FilterInvites");

                    b.Property<bool>("IgnoreAllChannels");

                    b.Property<ulong?>("LogAvi");

                    b.Property<ulong?>("LogBan");

                    b.Property<ulong?>("LogJoin");

                    b.Property<ulong?>("LogMsg");

                    b.Property<ulong?>("ModChannel");

                    b.Property<ulong?>("MusicChannel");

                    b.Property<ulong?>("MusicVcChannel");

                    b.Property<ulong?>("MuteRole");

                    b.Property<string>("Prefix");

                    b.Property<ulong?>("ReportChannel");

                    b.Property<bool>("StackLvlRoles");

                    b.Property<ulong?>("SuggestionChannel");

                    b.Property<string>("SuggestionEmoteNo");

                    b.Property<string>("SuggestionEmoteYes");

                    b.Property<bool>("WelcomeBanner");

                    b.Property<ulong?>("WelcomeChannel");

                    b.Property<uint>("WelcomeLimit");

                    b.Property<string>("WelcomeMessage");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.GuildInfo", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FaqOne");

                    b.Property<ulong>("FaqOneMessageId");

                    b.Property<string>("FaqTwo");

                    b.Property<ulong>("FaqTwoMessageId");

                    b.Property<ulong>("InviteMessageId");

                    b.Property<ulong>("LevelMessageId");

                    b.Property<ulong>("RuleMessageId");

                    b.Property<string>("Rules");

                    b.Property<ulong>("StaffMessageId");

                    b.HasKey("GuildId");

                    b.ToTable("GuildInfos");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.HungerGameConfig", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.HungerGameDefault", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("UserId");

                    b.ToTable("HungerGameDefaults");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.HungerGameLive", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.IgnoreChannel", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("IgnoreChannels");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Inventory", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<uint>("Amount");

                    b.Property<uint>("ItemId");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("Inventories");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.InventoryGlobal", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Amount");

                    b.Property<uint>("ItemId");

                    b.HasKey("UserId");

                    b.ToTable("InventoryGlobals");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.LevelReward", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<uint>("Level");

                    b.Property<ulong>("Role");

                    b.Property<bool>("Stackable");

                    b.HasKey("GuildId", "Level");

                    b.ToTable("LevelRewards");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.LootChannel", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("ChannelId");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("LootChannels");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.ModLog", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.MuteTimer", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("GuildId");

                    b.Property<DateTime>("Time");

                    b.HasKey("UserId", "GuildId");

                    b.ToTable("MuteTimers");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.NudeServiceChannel", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("ChannelId");

                    b.Property<uint>("Tolerance");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("NudeServiceChannels");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Report", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Shop", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.ShopEvent", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Item");

                    b.Property<uint>("Price");

                    b.Property<uint?>("Stock");

                    b.HasKey("Id");

                    b.ToTable("ShopEvents");
                });

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Suggestion", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.Warn", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.WarnMsgLog", b =>
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

            modelBuilder.Entity("Jibril.Services.Entities.Tables.WelcomeBanner", b =>
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
