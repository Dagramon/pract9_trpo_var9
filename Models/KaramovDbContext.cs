using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace pract8_trpo.Models;

public partial class KaramovDbContext : DbContext
{
    public KaramovDbContext()
    {
    }

    public KaramovDbContext(DbContextOptions<KaramovDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RevokedToken> RevokedTokens { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=sql.ects;Database=karamovDB;User Id=student_00;Password=student_00;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash, "IX_RefreshTokens").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_User");
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.HasIndex(e => e.Jti, "IX_RevokedTokens").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Jti).HasMaxLength(64);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("Task");

            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.OwnerLogin).IsUnicode(false);
            entity.Property(e => e.Title).IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login).IsUnicode(false);
            entity.Property(e => e.PasswordHash).IsUnicode(false);
            entity.Property(e => e.Role).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
