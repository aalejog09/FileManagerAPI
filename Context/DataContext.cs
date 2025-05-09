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
            // Índice único compuesto para evitar duplicados por Clave + Ruta + Nombre
            modelBuilder.Entity<FileRecord>()
                .HasIndex(f => new { f.Clave, f.FilePath, f.FileName })
                .IsUnique();

            modelBuilder.Entity<FileRecord>()
                .HasIndex(f => f.Clave); // opcional si solo lo usas para búsquedas

            modelBuilder.Entity<SupportedFile>()
                .HasIndex(f => f.Extension)
                .IsUnique();

            modelBuilder.Entity<SupportedFile>()
                .Property(f => f.MaxSizeKB)
                .HasColumnType("decimal(10,2)");

        }
    }
}
