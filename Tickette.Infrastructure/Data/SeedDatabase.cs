using Microsoft.AspNetCore.Identity;
using Tickette.Application.Common.Constants;
using Tickette.Domain.Entities;
using Constant = Tickette.Domain.Common.Constant;

namespace Tickette.Infrastructure.Data;

public static class SeedDatabase
{
    public static void SeedCategories(ApplicationDbContext dbContext)
    {
        var categories = new List<Category>
        {
            Category.CreateCategory(Guid.Parse("6aa24b95-68ee-47c7-979f-37bf686974c6") ,"Concerts"),
            Category.CreateCategory(Guid.Parse("eae9275f-f9fa-4928-ae72-f1c047ab41b7"), "Theater"),
            Category.CreateCategory(Guid.Parse("9a84ed7c-c3d7-4ee0-8669-62e1d1fa124a"), "Sports"),
            Category.CreateCategory(Guid.Parse("1b29b70a-5fad-4a48-a2b6-0b6d4449337f"), "Festivals"),
            Category.CreateCategory(Guid.Parse("8c14c2ca-b4d3-4514-8a5c-6f529edfaa65"), "Conferences"),
            Category.CreateCategory(Guid.Parse("4224c8c4-cda8-47b2-a18c-2190b25b53bd"), "Workshops"),
            Category.CreateCategory(Guid.Parse("07a50858-8655-474c-8ccf-04da85612675"),"Comedy Shows"),
            Category.CreateCategory(Guid.Parse("ad250634-f6ad-4568-9ec2-cd9b94db2a56"), "Family Events"),
            Category.CreateCategory(Guid.Parse("ec207bb3-adcb-463d-96e7-9bba2d85ed6f"), "Others"),
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

    public static void SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = Constant.APPLICATION_ROLES;

        foreach (var role in roles)
        {
            var exists = roleManager.RoleExistsAsync(role.Value).Result;

            if (!exists)
            {
                var identityRole = new IdentityRole<Guid>
                {
                    Id = role.Key,
                    Name = role.Value,
                    NormalizedName = role.Value.ToUpper()
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


    public static void SeedRolesAndPermissions(ApplicationDbContext dbContext)
    {
        // Define roles with their permissions
        var predefinedRoles = new List<CommitteeRole>
        {
            CommitteeRole.Create(Guid.Parse("7106ddaf-d5a2-41b2-9bbe-1be377f7ed74"), CommitteeMemberKeys.COMMITTEE_MEMBER_ROLES.EventOwner),
            CommitteeRole.Create(Guid.Parse("b7103fcf-c897-4b6e-9f9c-ade8ff92fb11"), CommitteeMemberKeys.COMMITTEE_MEMBER_ROLES.Admin),
            CommitteeRole.Create(Guid.Parse("0b28fec2-16f4-4f1b-8ebf-925c93c24dba"), CommitteeMemberKeys.COMMITTEE_MEMBER_ROLES.CheckInStaff),
            CommitteeRole.Create(Guid.Parse("9c4575bb-e66b-404e-bee6-3785f8362333"), CommitteeMemberKeys.COMMITTEE_MEMBER_ROLES.CheckOutStaff),
            CommitteeRole.Create(Guid.Parse("f957a634-98ea-4716-8c0e-c4c8172ef118"), CommitteeMemberKeys.COMMITTEE_MEMBER_ROLES.RedeemStaff)
        };

        foreach (var role in predefinedRoles)
        {
            // Check if the role already exists
            var existingRole = dbContext.CommitteeRoles
                .FirstOrDefault(r => r.Name == role.Name);

            if (existingRole == null)
            {
                // Add the new role to the database
                dbContext.CommitteeRoles.Add(role);
            }
        }

        // Save changes to the database
        dbContext.SaveChanges();
    }
}