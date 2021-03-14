using Microsoft.AspNetCore.Identity;
using System;

namespace AS.Service.Identity
{
    public class IdentityInitializer
    {
        private readonly IdentityAppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityInitializer(
            IdentityAppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
             if (_context.Database.EnsureCreated())
            {
                if (!_roleManager.RoleExistsAsync(Roles.ROLE_GERAL).Result)
                {
                    var resultado = _roleManager.CreateAsync(
                        new IdentityRole(Roles.ROLE_GERAL)).Result;
                    if (!resultado.Succeeded)
                    {
                        throw new Exception(
                            $"Erro durante a criação da role {Roles.ROLE_GERAL}.");
                    }
                }

                CreateUser(
                    new ApplicationUser()
                    {
                        UserName = "admin_api",
                        Email = "admin-api@teste.com.br",
                        EmailConfirmed = true
                    }, "Admin@12345", Roles.ROLE_GERAL);

                CreateUser(
                    new ApplicationUser()
                    {
                        UserName = "usuarioinvalido",
                        Email = "usuarioinvalido@teste.com.br",
                        EmailConfirmed = true
                    }, "Invalido@12345");
            }
        }
        private void CreateUser(
            ApplicationUser user,
            string password,
            string initialRole = null)
        {
            if (_userManager.FindByNameAsync(user.UserName).Result == null)
            {
                var resultado = _userManager
                    .CreateAsync(user, password).Result;

                if (resultado.Succeeded &&
                    !String.IsNullOrWhiteSpace(initialRole))
                {
                    _userManager.AddToRoleAsync(user, initialRole).Wait();
                }
            }
        }
    }
}
