using Microsoft.EntityFrameworkCore;

using CapManagement.Shared.Models;
using CapManagement.Shared.Models.AppicationUserModels;
using CapManagement.Server.AppicationUserModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace CapManagement.Server.DbContexts
{
    public class FleetDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {

        // i removed : DbContext

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Earning> Earnings { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<DriverExpense> DriverExpenses { get; set; }

        public DbSet<Company> Company { get; set; }

        public DbSet<UserInvite> UserInvites { get; set; }
        public FleetDbContext(DbContextOptions<FleetDbContext> options)
            : base(options) { }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft delete query filters
            modelBuilder.Entity<Car>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Driver>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Contract>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Earning>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Settlement>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<DriverExpense>().HasQueryFilter(de => !de.IsDeleted);
            modelBuilder.Entity<Expense>().HasQueryFilter(ex => !ex.IsDeleted);
            modelBuilder.Entity<Company>().HasQueryFilter(co => co.IsActive);


            // below is the companyEmail 
            modelBuilder.Entity<Company>()
           .HasIndex(c => c.CompanyEmail)
          .IsUnique();
            // Filtered unique index: Enforces one Active contract per car/company
            modelBuilder.Entity<Contract>()
         .HasIndex(e => new { e.CarId, e.CompanyId })
         .IsUnique()
         .HasFilter("[Status] = 1"); //  Fixed: Use 1 for Active



            // Contract ↔ Driver
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Driver)
                .WithMany(d => d.Contracts)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contract ↔ Car
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Car)
                .WithMany(car => car.Contracts)
                .HasForeignKey(c => c.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contract ↔ Settlement / Earning
            modelBuilder.Entity<Earning>()
                .HasOne(e => e.Contract)
                .WithMany()
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Earning>()
                .HasOne(e => e.Settlement)
                .WithMany(s => s.Earnings)
                .HasForeignKey(e => e.SettlementId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Settlement>()
                .HasOne(s => s.Contract)
                .WithMany()
                .HasForeignKey(s => s.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            // DriverExpense ↔ Driver / Contract
            modelBuilder.Entity<DriverExpense>()
                .HasOne(de => de.Driver)
                .WithMany()
                .HasForeignKey(de => de.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverExpense>()
                .HasOne(de => de.Contract)
                .WithMany()
                .HasForeignKey(de => de.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense ↔ Car
            modelBuilder.Entity<Expense>()
                .HasOne(ex => ex.Car)
                .WithMany()
                .HasForeignKey(ex => ex.CarId)
                .OnDelete(DeleteBehavior.Restrict);




            // below is for usermangement 

           
         


            modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
             .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
              .HasQueryFilter(u => !u.IsDeleted);



            // Company ↔ Entities
            modelBuilder.Entity<Car>().HasOne<Company>().WithMany().HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Driver>().HasOne<Company>().WithMany().HasForeignKey(d => d.CompanyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Contract>().HasOne<Company>().WithMany().HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Settlement>().HasOne<Company>().WithMany().HasForeignKey(s => s.CompanyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Expense>().HasOne<Company>().WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<Car>().HasIndex(c => c.NumberPlate).IsUnique();
            modelBuilder.Entity<Contract>().HasIndex(c => new { c.CarId, c.Status }).IsUnique().HasFilter("[Status] = 0");
            modelBuilder.Entity<Driver>().HasIndex(d => new { d.UserId, d.CompanyId }).IsUnique();
            modelBuilder.Entity<Company>().HasIndex(c => c.VATNumber).IsUnique();
            // Add unique index for CompanyName (scoped to IsActive)
            modelBuilder.Entity<Company>()
                .HasIndex(c => new { c.CompanyName, c.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");


            // Decimal precision configuration to avoid truncation warnings
            modelBuilder.Entity<Contract>().Property(c => c.PaymentAmount).HasPrecision(18, 2);

            modelBuilder.Entity<DriverExpense>().Property(d => d.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<DriverExpense>().Property(d => d.VatPercent).HasPrecision(5, 2);

            modelBuilder.Entity<Earning>().Property(e => e.GrossIncome).HasPrecision(18, 2);
            modelBuilder.Entity<Earning>().Property(e => e.BtwPercentage).HasPrecision(5, 2);

            modelBuilder.Entity<Settlement>().Property(s => s.GrossAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Settlement>().Property(s => s.NetPayout).HasPrecision(18, 2);
            modelBuilder.Entity<Settlement>().Property(s => s.RentDeduction).HasPrecision(18, 2);
            modelBuilder.Entity<Settlement>().Property(s => s.ExtraCosts).HasPrecision(18, 2);

            modelBuilder.Entity<Expense>().Property(ex => ex.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Expense>().Property(ex => ex.VatPercent).HasPrecision(5, 2);
        }

    }
}
