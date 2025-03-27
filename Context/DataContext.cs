using FileManagerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FileStorageApi.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<FileRecord> Files { get; set; }
        public DbSet<SupportedFile> SupportedFiles{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileRecord>()
                .HasIndex(f => f.Clave);

            modelBuilder.Entity<SupportedFile>()
            .HasIndex(f => f.Extension)
            .IsUnique();

            modelBuilder.Entity<SupportedFile>()
            .Property(f => f.MaxSizeKB)
            .HasColumnType("decimal(10,2)");

        }
    }
}
