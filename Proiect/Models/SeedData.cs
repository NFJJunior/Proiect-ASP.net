using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
namespace Proiect.Models
{
    //pasul 4 din user si roluri
    public static class SeedData
    {
        public static void Initialize(IServiceProvider
       serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul
                // De aceea facem return pentru a nu insera rolurile inca o data
                // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }
                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(
                new IdentityRole { Id = "867cd91f-95d4-4c82-82c1-cf9e536d8f38", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "e9059137-5103-4868-80e1-e09f863db4a0", Name = "User", NormalizedName = "User".ToUpper() }
                );
                // o noua instanta pe care o vom utiliza pentrucrearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();
                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "26fa51ea-9839-41bb-9506-552c4db32053",
                    // primary key
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,
               "Admin1!")
                },

                new ApplicationUser
                {
                    Id = "2dfcf83a-e67a-4983-bad4-866f3ca3d036",
                    // primary key
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,
                "User1!")
                }
);
                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "867cd91f-95d4-4c82-82c1-cf9e536d8f38",
                    UserId = "26fa51ea-9839-41bb-9506-552c4db32053"
                },
               new IdentityUserRole<string>
               {
                   RoleId = "e9059137-5103-4868-80e1-e09f863db4a0",
                   UserId = "2dfcf83a-e67a-4983-bad4-866f3ca3d036"
               }
                );
                context.SaveChanges();
            }
        } 
    }
}

