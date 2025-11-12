
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PayHelp.Infrastructure;

#nullable disable

namespace PayHelp.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PayHelp.Domain.Entities.CannedMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Conteudo")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("GatilhoPalavraChave")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.ToTable("CannedMessages");
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.FaqEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DataCriacao")
                        .HasColumnType("datetime2");

                    b.Property<string>("DescricaoProblema")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Solucao")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<Guid?>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("TituloProblema")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.HasKey("Id");

                    b.HasIndex("TicketId");

                    b.ToTable("FaqEntries");
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.ReportEntry", b =>
                {
                    b.Property<Guid>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CriadoEmUtc")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan?>("Duracao")
                        .HasColumnType("time");

                    b.Property<DateTime?>("EncerradoEmUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("SolicitanteEmail")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("SolicitanteRole")
                        .HasColumnType("int");

                    b.Property<int>("StatusFinal")
                        .HasColumnType("int");

                    b.HasKey("TicketId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.Ticket", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CriadoEmUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<DateTime?>("EncerradoEmUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid?>("SupportUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("SupportUserId");

                    b.HasIndex("UserId");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.TicketMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Automatica")
                        .HasColumnType("bit");

                    b.Property<string>("Conteudo")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<DateTime>("EnviadoEmUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("RemetenteUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RemetenteUserId");

                    b.HasIndex("TicketId");

                    b.ToTable("TicketMessages");
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("NumeroInscricao")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("SenhaHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.FaqEntry", b =>
                {
                    b.HasOne("PayHelp.Domain.Entities.Ticket", null)
                        .WithMany()
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.ReportEntry", b =>
                {
                    b.HasOne("PayHelp.Domain.Entities.Ticket", null)
                        .WithOne()
                        .HasForeignKey("PayHelp.Domain.Entities.ReportEntry", "TicketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.Ticket", b =>
                {
                    b.HasOne("PayHelp.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("SupportUserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("PayHelp.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.TicketMessage", b =>
                {
                    b.HasOne("PayHelp.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("RemetenteUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("PayHelp.Domain.Entities.Ticket", null)
                        .WithMany("Mensagens")
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PayHelp.Domain.Entities.Ticket", b =>
                {
                    b.Navigation("Mensagens");
                });
#pragma warning restore 612, 618
        }
    }
}
