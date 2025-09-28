using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Repository.Contexts;

public class FinancasContext : DbContext
{
    public DbSet<TipoCategoria> TipoCategoria { get; set; }
    public DbSet<Periodo> Periodos { get; set; }
    public DbSet<Instituicao> Instituicoes { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }

    public FinancasContext(DbContextOptions<FinancasContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("unaccent");

        ConfigTiposCategoria(modelBuilder);
        ConfigPeriodos(modelBuilder);
        ConfigInstituicoes(modelBuilder);
        ConfigCategorias(modelBuilder);
        ConfigMovimentacoes(modelBuilder);
    }

    private void ConfigMovimentacoes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movimentacao>(e =>
        {
            ConfigRegistro(e);

            e.Property(tc => tc.Data)
                .IsRequired();

            e.Property(tc => tc.Descricao)
                .IsRequired()
                .HasMaxLength(90);

            e.Property(tc => tc.Valor)
                .IsRequired();

            e.HasOne(e => e.Instituicao)
                .WithMany()
                .HasForeignKey(e => e.InstituicaoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(e => e.Categoria)
                .WithMany()
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(e => e.Periodo)
                .WithMany()
                .HasForeignKey(e => e.PeriodoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigCategorias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(e =>
        {
            ConfigRegistro(e);

            e.Property(tc => tc.Nome)
                    .IsRequired()
                    .HasMaxLength(80);

            e.HasOne(e => e.Parent)
                .WithMany(e => e.Filhos)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(e => e.TipoCategoria)
                .WithMany()
                .HasForeignKey(e => e.TipoCategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigInstituicoes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instituicao>(e =>
        {
            ConfigRegistro(e);
            e.Property(tc => tc.Nome)
                    .IsRequired()
                    .HasMaxLength(25);

            e.Property(tc => tc.SaldoInicial)
                    .IsRequired();

            e.Property(tc => tc.SaldoAtual)
                    .IsRequired();

            e.Property(tc => tc.DataSaldoInicial)
                    .IsRequired();

            e.Property(tc => tc.InstituicaoCredito)            
                    .IsRequired();

            e.HasIndex(tc => tc.Nome)
                .IsUnique();
        });
    }

    private void ConfigPeriodos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Periodo>(e =>
        {
            ConfigRegistro(e);

            e.Property(tc => tc.Nome)
                .IsRequired()
                .IsFixedLength(true)
                .HasMaxLength(6);

            e.Property(tc => tc.Inicio)
                .IsRequired();

            e.Property(tc => tc.Fim)
                .IsRequired();

            e.HasIndex(tc => tc.Nome)
                .IsUnique();
        });
    }

    private void ConfigTiposCategoria(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TipoCategoria>(e =>
        {
            ConfigRegistro(e);

            e.ToTable("tipo_categoria");

            e.Property(tc => tc.Nome)
                .IsRequired()
                .HasMaxLength(30);

            e.HasIndex(tc => tc.Nome)
                .IsUnique();
        });
    }

    private void ConfigRegistro<T>(EntityTypeBuilder<T> e)
        where T : Registro
    {
        e.HasKey(r => r.Id);
        e.Property(r => r.Id).ValueGeneratedOnAdd();

        e.Ignore(i => i.Criacao);
        e.Ignore(i => i.Atualizacao);
    }


    public override int SaveChanges()
    {
        AjustaDatas();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AjustaDatas();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AjustaDatas();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AjustaDatas();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AjustaDatas()
    {
        var entidades = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entidades)
        {
            if (entry.Entity is Registro registro)
            {
                if (entry.State == EntityState.Added)
                {
                    registro.Criacao = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    registro.Atualizacao = DateTime.UtcNow;
                }
            }
        }
    }
}
