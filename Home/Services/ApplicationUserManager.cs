using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using HomeApp.Models;

namespace HomeApp.Services
{
    public class ApplicationUserManager
    {
        private static readonly ConcurrentDictionary<string, string> _registeredUsers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin@demo.com"] = "Admin123!"
        };

        public ApplicationUser? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        public event Action? OnAuthStateChanged;

        public async Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError 
                { 
                    Description = "El correo y la contraseña son requeridos." 
                }));
            }

            if (_registeredUsers.ContainsKey(model.Email))
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError 
                { 
                    Description = "Ya existe una cuenta registrada con este correo electrónico." 
                }));
            }

            _registeredUsers[model.Email] = model.Password;
            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> LoginAsync(LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError 
                { 
                    Description = "El correo y la contraseña son requeridos." 
                }));
            }

            if (_registeredUsers.TryGetValue(model.Email, out var storedPassword))
            {
                if (storedPassword == model.Password)
                {
                    CurrentUser = new ApplicationUser
                    {
                        Email = model.Email,
                        UserName = model.Email
                    };
                    NotifyAuthStateChanged();
                    return await Task.FromResult(IdentityResult.Success);
                }
                else
                {
                    return await Task.FromResult(IdentityResult.Failed(new IdentityError 
                    { 
                        Description = "Contraseña incorrecta." 
                    }));
                }
            }

            return await Task.FromResult(IdentityResult.Failed(new IdentityError 
            { 
                Description = "No se encontró ningún usuario con ese correo electrónico." 
            }));
        }

        public async Task LogoutAsync()
        {
            CurrentUser = null;
            NotifyAuthStateChanged();
            await Task.CompletedTask;
        }

        public void Logout()
        {
            CurrentUser = null;
            NotifyAuthStateChanged();
        }

        private void NotifyAuthStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            if (!string.IsNullOrEmpty(user.Email))
            {
                _registeredUsers[user.Email] = password;
            }
            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            if (_registeredUsers.ContainsKey(email))
            {
                return await Task.FromResult(new ApplicationUser { Email = email, UserName = email });
            }
            return await Task.FromResult<ApplicationUser?>(null);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            if (user.Email != null && _registeredUsers.TryGetValue(user.Email, out var storedPassword))
            {
                return await Task.FromResult(storedPassword == password);
            }
            return await Task.FromResult(false);
        }
    }
}
