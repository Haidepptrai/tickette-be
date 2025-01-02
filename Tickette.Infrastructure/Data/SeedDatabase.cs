using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;
using static Tickette.Domain.Common.Constant;
using Constant = Tickette.Domain.Common.Constant;

namespace Tickette.Infrastructure.Data;

public static class SeedDatabase
{
    public static async Task SeedCategories(ApplicationDbContext dbContext)
    {
        if (!dbContext.Categories.Any())
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
                Category.CreateCategory("Family Events")
            };

            dbContext.Categories.AddRange(categories);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Seeded categories successfully.");
        }
        else
        {
            Console.WriteLine("Categories already exist.");
        }
    }

    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = Constant.APPLICATION_ROLES;

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var identityRole = new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    NormalizedName = role.ToUpper()
                };

                await roleManager.CreateAsync(identityRole);
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
            new CommitteeRole(COMMITTEE_MEMBER_ROLES.EventOwner)
                .WithPermissions(
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Marketing,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Orders,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.SeatMap,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Members,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckIn,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckOut,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Redeem
                ),

            new CommitteeRole(COMMITTEE_MEMBER_ROLES.Admin)
                .WithPermissions(
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Edit,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Summary,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Voucher,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Marketing,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Orders,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.SeatMap,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Members,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckIn,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckOut,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Redeem
                ),

            new CommitteeRole(COMMITTEE_MEMBER_ROLES.Manager)
                .WithPermissions(
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Edit,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Summary,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Voucher,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Marketing,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Orders,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.SeatMap,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Members,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckIn,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckOut,
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Redeem
                ),

            new CommitteeRole(COMMITTEE_MEMBER_ROLES.CheckInStaff)
                .WithPermissions(
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckIn
                ),

            new CommitteeRole(COMMITTEE_MEMBER_ROLES.CheckOutStaff)
                .WithPermissions(
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.CheckOut
                ),

            new CommitteeRole(COMMITTEE_MEMBER_ROLES.RedeemStaff)
                .WithPermissions(
                    COMMITTEE_MEMBER_ROLES_PERMISSIONS.Redeem
                )
        };


        foreach (var role in predefinedRoles)
        {
            // Check if the role already exists
            var existingRole = await dbContext.CommitteeRoles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == role.Name);

            if (existingRole == null)
            {
                // Add the new role and its permissions to the database
                dbContext.CommitteeRoles.Add(role);
            }
            else
            {
                // Update permissions if needed (optional logic)
                foreach (var permission in role.Permissions
                             .Where(permission => existingRole.Permissions.All(p => p.Name != permission.Name)))
                {
                    existingRole.Permissions.Add(new CommitteeRolePermission(permission.Name));
                }
            }
        }

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }
}