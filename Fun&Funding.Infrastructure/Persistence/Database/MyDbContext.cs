using Fun_Funding.Application.ExternalServices.SoftDeleteService;
using Fun_Funding.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace Fun_Funding.Infrastructure.Persistence.Database
{
    public class MyDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public MyDbContext()
        {

        }
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity to use Guid for keys
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto-generate GUID
            });

            // Configure the relationship between Order and User explicitly
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict deletion if needed

            // Configure the relationship between PackageBacker and User explicitly
            modelBuilder.Entity<PackageBacker>()
                .HasOne(pb => pb.User)
                .WithMany(u => u.PackageUsers)
                .HasForeignKey(pb => pb.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict deletion if needed

            #region FundingProject
            // Configure FundingProject and User relationship
            modelBuilder.Entity<FundingProject>()
                .HasOne(fp => fp.User)
                .WithMany(u => u.FundingProjects)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FundingProject categories relationship
            modelBuilder.Entity<FundingProject>()
                .HasMany(fp => fp.Categories)
                .WithMany(c => c.Projects);
            #endregion

            #region Milestone
            // Configure ProjectMilestone and Milestone relationship
            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(pm => pm.Milestone)
                .WithMany(m => m.ProjectMilestones)
                .HasForeignKey(pm => pm.MilestoneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProjectMilestone and FundingProject relationship
            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(pm => pm.FundingProject)
                .WithMany(fp => fp.ProjectMilestones)
                .HasForeignKey(pm => pm.FundingProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProjectMilestone and ProjectMilestoneRequirement relationship
            modelBuilder.Entity<ProjectMilestone>()
                .HasMany(pm => pm.ProjectMilestoneRequirements)
                .WithOne(pr => pr.ProjectMilestone)
                .HasForeignKey(pr => pr.ProjectMilestoneId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region ProjectMilestoneRequirement
            // Configure ProjectMilestoneRequirement and Requirement relationship
            modelBuilder.Entity<ProjectMilestoneRequirement>()
                .HasOne(pmr => pmr.Requirement)
                .WithMany(r => r.ProjectRequirements)
                .HasForeignKey(pmr => pmr.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProjectMilestoneRequirement and ProjectRequirementFile relationship
            modelBuilder.Entity<ProjectMilestoneRequirement>()
                .HasMany(pmr => pmr.RequirementFiles)
                .WithOne(prf => prf.ProjectRequirement)
                .HasForeignKey(prf => prf.ProjectMilestoneRequirementId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region ProjectMilestoneBacker
            // Configure ProjectMilestoneBacker and ProjectMilestone relationship
            modelBuilder.Entity<ProjectMilestoneBacker>()
                .HasOne(pmb => pmb.ProjectMilestone)
                .WithMany(pm => pm.ProjectMilestoneBackers)
                .HasForeignKey(pmb => pmb.ProjectMilestoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProjectMilestoneBacker and User relationship
            modelBuilder.Entity<ProjectMilestoneBacker>()
                .HasOne(pmb => pmb.Backer)
                .WithMany(u => u.ProjectMilestoneBackers)
                .HasForeignKey(pmb => pmb.BackerId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region MarketplaceProject
            // Configure cascade delete between MarketplaceProject and MarketplaceFile
            modelBuilder.Entity<MarketplaceProject>()
                .HasMany(mp => mp.MarketplaceFiles)
                .WithOne()
                .HasForeignKey(mf => mf.MarketplaceProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Set cascade delete

            // Configure cascade delete between MarketplaceProject and ProjectCoupon
            modelBuilder.Entity<MarketplaceProject>()
                .HasMany(mp => mp.ProjectCoupons)
                .WithOne(pc => pc.MarketplaceProject)
                .HasForeignKey(pc => pc.MarketplaceProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Set cascade delete
            #endregion

            //#region FilterIsDeleted
            //modelBuilder.Entity<FundingProject>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<FundingFile>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<MarketplaceProject>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<MarketplaceFile>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<Category>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<Milestone>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<Package>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<ProjectCoupon>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<Requirement>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<RewardItem>().HasQueryFilter(x => x.IsDeleted == false);
            //modelBuilder.Entity<WithdrawRequest>().HasQueryFilter(x => x.IsDeleted == false);
            //#endregion
        }




        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<CommissionFee> CommissionFee { get; set; }
        public DbSet<DigitalKey> DigitalKey { get; set; }
        public DbSet<FundingFile> FundingFile { get; set; }
        public DbSet<FundingProject> FundingProject { get; set; }
        public DbSet<MarketplaceFile> MarketplaceFile { get; set; }
        public DbSet<MarketplaceProject> MarketplaceProject { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Package> Package { get; set; }
        public DbSet<PackageBacker> PackageBacker { get; set; }
        public DbSet<RewardItem> RewardItem { get; set; }
        public DbSet<SystemWallet> SystemWallet { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<UserFile> UserFile { get; set; }
        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<WithdrawRequest> WithdrawRequest { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<ProjectMilestoneRequirement> ProjectMilestoneRequirements { get; set; }
        public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
        public DbSet<ProjectRequirementFile> ProjectRequirementFiles { get; set; }
        public DbSet<ProjectMilestoneBacker> ProjectMilestoneBacker { get; set; }
        public DbSet<ProjectCoupon> ProjectCoupon { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString())
                    .AddInterceptors(new SoftDeleteInterceptor());
            }
        }

        private string GetConnectionString()
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}
