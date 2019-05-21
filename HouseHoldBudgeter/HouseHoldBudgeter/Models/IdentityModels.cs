using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using HouseHoldBudgeter.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace HouseHoldBudgeter.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        [InverseProperty(nameof(Household.CreatedBy))]
        public virtual List<Household> CreatedHouseholds { get; set; }

        [InverseProperty(nameof(Household.HouseholdJoinedMembers))]
        public virtual List<Household> JoinedHouseholds { get; set; }

        [InverseProperty(nameof(Household.HouseholdInvitedMembers))]
        public virtual List<Household> InvitedToHouseholds { get; set; }

        public ApplicationUser()
        {
            CreatedHouseholds = new List<Household>();
            JoinedHouseholds = new List<Household>();
            InvitedToHouseholds = new List<Household>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Household> Households { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Household>()
                .HasMany<Category>(g => g.HouseholdCategories)
                .WithRequired(s => s.Household)
                .WillCascadeOnDelete(false);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}