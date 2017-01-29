using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace Inventory.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public IdentityResult SetUserStock(ApplicationUser user, int stockId) 
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.UserStocks.Add(new UserStock() { StockId = stockId, UserId = user.Id });
            db.SaveChanges();

            return IdentityResult.Success; 
        }

        public void SetUserRole(string roleName, ApplicationUser user)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());

            if (!roleManager.RoleExists(roleName))
            {
                var roleResult = roleManager.Create(new IdentityRole(roleName));
            }

            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var rolesForUser = userManager.GetRoles(user.Id);

            if (!rolesForUser.Contains(roleName))
            {
                var result = userManager.AddToRole(user.Id, roleName);
            }
        }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public DbSet<UserStock> UserStocks { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
        {
            protected override void Seed(ApplicationDbContext context)
            {
                
                InitializeIdentityForEF(context);
                //base.Seed(context);
            }

            private static void InitializeIdentityForEF(ApplicationDbContext db)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());               
                const string roleName = "Admin";

                if (!roleManager.RoleExists(roleName))
                {
                    var roleResult = roleManager.Create(new IdentityRole(roleName));
                }

                var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                const string name = "gadowski.t";
                const string pswd = "0000";

                var user = userManager.FindByName(name);
                if (user == null)
                {
                    user = new ApplicationUser() { UserName = name };
                    var userResult = userManager.Create(user, pswd);
                }

                var rolesForUser = userManager.GetRoles(user.Id);
                if (!rolesForUser.Contains(roleName))
                {
                    var result = userManager.AddToRole(user.Id, roleName);
                }

            }
        }




    }




}