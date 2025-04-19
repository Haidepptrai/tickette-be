using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tickette.Domain.Entities;
using static Tickette.Domain.Common.Constant;
using Constant = Tickette.Domain.Common.Constant;

namespace Tickette.Infrastructure.Data;

public static class SeedDatabase
{
    public static void SeedCategories(ApplicationDbContext dbContext)
    {
        var categories = new List<Category>
        {
            Category.CreateCategory("Concerts"),
            Category.CreateCategory("Theater"),
            Category.CreateCategory("Sports"),
            Category.CreateCategory("Festivals"),
            Category.CreateCategory("Conferences"),
            Category.CreateCategory("Workshops"),
            Category.CreateCategory("Comedy Shows"),
            Category.CreateCategory("Family Events"),
            Category.CreateCategory("Others"),
        };

        foreach (var cat in categories)
        {
            var exists = dbContext.Categories.Any(c => c.Name.Contains(cat.Name));
            if (!exists)
            {
                dbContext.Categories.Add(cat);
            }
        }

        dbContext.SaveChanges();
    }


    public static void SeedRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = Constant.APPLICATION_ROLES;

        foreach (var role in roles)
        {
            var exists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
            if (!exists)
            {
                var identityRole = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    NormalizedName = role.ToUpper()
                };

                roleManager.CreateAsync(identityRole).GetAwaiter().GetResult();
                Console.WriteLine($"Created new role: {role}");
            }
            else
            {
                Console.WriteLine($"Role already exists: {role}");
            }
        }
    }


    public static async Task SeedRolesAndPermissions(ApplicationDbContext dbContext)
    {
        // Define roles with their permissions
        var predefinedRoles = new List<CommitteeRole>
        {
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.EventOwner),
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.Admin),
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.Manager),
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.CheckInStaff),
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.CheckOutStaff),
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.RedeemStaff)
        };


        foreach (var role in predefinedRoles)
        {
            // Check if the role already exists
            var existingRole = await dbContext.CommitteeRoles
                .FirstOrDefaultAsync(r => r.Name == role.Name);

            if (existingRole == null)
            {
                // Add the new role to the database
                dbContext.CommitteeRoles.Add(role);
            }
        }

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }
}