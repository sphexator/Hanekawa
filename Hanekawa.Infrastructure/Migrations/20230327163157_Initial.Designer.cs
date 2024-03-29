﻿// <auto-generated />
using System;
using Hanekawa.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hanekawa.Infrastructure.Migrations
{
    [DbContext(typeof(DbService))]
    [Migration("20230327163157_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Hanekawa.Entities.Configs.GreetConfig", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("Channel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("DmEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("DmMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("ImageEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("GuildId");

                    b.ToTable("GreetConfig");
                });

            modelBuilder.Entity("Hanekawa.Entities.Configs.GuildConfig", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("Hanekawa.Entities.Configs.LevelConfig", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("LevelEnabled")
                        .HasColumnType("boolean");

                    b.Property<bool>("LevelUpDmEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("LevelUpDmMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LevelUpMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("LevelUpMessageEnabled")
                        .HasColumnType("boolean");

                    b.HasKey("GuildId");

                    b.ToTable("LevelConfig");
                });

            modelBuilder.Entity("Hanekawa.Entities.Users.GuildUser", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset>("DailyClaimed")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DailyStreak")
                        .HasColumnType("integer");

                    b.Property<long>("Experience")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastSeen")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("TotalVoiceTime")
                        .HasColumnType("interval");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Hanekawa.Entities.Users.User", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTimeOffset?>("PremiumExpiration")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("UserId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Hanekawa.Infrastructure.Tables.Internal.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CallSite")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Exception")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Logger")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TimeStamp")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Hanekawa.Entities.Configs.GreetConfig", b =>
                {
                    b.HasOne("Hanekawa.Entities.Configs.GuildConfig", "GuildConfig")
                        .WithOne("GreetConfig")
                        .HasForeignKey("Hanekawa.Entities.Configs.GreetConfig", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildConfig");
                });

            modelBuilder.Entity("Hanekawa.Entities.Configs.LevelConfig", b =>
                {
                    b.HasOne("Hanekawa.Entities.Configs.GuildConfig", "GuildConfig")
                        .WithOne("LevelConfig")
                        .HasForeignKey("Hanekawa.Entities.Configs.LevelConfig", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildConfig");
                });

            modelBuilder.Entity("Hanekawa.Entities.Users.GuildUser", b =>
                {
                    b.HasOne("Hanekawa.Entities.Users.User", "User")
                        .WithMany("GuildUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Hanekawa.Entities.Configs.GuildConfig", b =>
                {
                    b.Navigation("GreetConfig")
                        .IsRequired();

                    b.Navigation("LevelConfig")
                        .IsRequired();
                });

            modelBuilder.Entity("Hanekawa.Entities.Users.User", b =>
                {
                    b.Navigation("GuildUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
