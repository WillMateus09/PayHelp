using Microsoft.EntityFrameworkCore;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<CannedMessage> CannedMessages => Set<CannedMessage>();
    public DbSet<FaqEntry> FaqEntries => Set<FaqEntry>();
    public DbSet<ReportEntry> Reports => Set<ReportEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Titulo).HasMaxLength(200);
            e.Property(t => t.Descricao).HasMaxLength(4000);

            e.HasMany(t => t.Mensagens)
                .WithOne()
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);


            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.SupportUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TicketMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Conteudo).HasMaxLength(4000);

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.RemetenteUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(200);
            e.Property(u => u.Nome).HasMaxLength(200);
        });

        modelBuilder.Entity<CannedMessage>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Titulo).HasMaxLength(200);
            e.Property(c => c.Conteudo).HasMaxLength(2000);
            e.Property(c => c.GatilhoPalavraChave).HasMaxLength(400);
        });

        modelBuilder.Entity<FaqEntry>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.TituloProblema).HasMaxLength(300);
            e.Property(f => f.DescricaoProblema).HasMaxLength(4000);
            e.Property(f => f.Solucao).HasMaxLength(4000);

            e.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(f => f.TicketId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReportEntry>(e =>
        {
            e.HasKey(r => r.TicketId);
            e.Property(r => r.SolicitanteEmail).HasMaxLength(200);

            e.HasOne<Ticket>()
                .WithOne()
                .HasForeignKey<ReportEntry>(r => r.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
