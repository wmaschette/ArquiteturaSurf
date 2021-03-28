﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace AS.Service.Identity
{
    public class AccessManager
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private SigningConfigurations _signingConfigurations;
        private TokenConfigurations _tokenConfigurations;
        private IDistributedCache _cache;

        public AccessManager(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            SigningConfigurations signingConfigurations,
            TokenConfigurations tokenConfigurations,
            IDistributedCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _signingConfigurations = signingConfigurations;
            _tokenConfigurations = tokenConfigurations;
            _cache = cache;
        }

        public bool ValidateCredentials(AccessCredentials credenciais)
        {
            bool credenciaisValidas = false;
            if (credenciais != null && !String.IsNullOrWhiteSpace(credenciais.UserID))
            {
                if (credenciais.GrantType == "password")
                {
                    var userIdentity = _userManager
                        .FindByNameAsync(credenciais.UserID).Result;
                    if (userIdentity != null)
                    {
                        var resultadoLogin = _signInManager
                            .CheckPasswordSignInAsync(userIdentity, credenciais.Password, false)
                            .Result;
                        if (resultadoLogin.Succeeded)
                        {
                            credenciaisValidas = _userManager.IsInRoleAsync(
                                userIdentity, Roles.ROLE_GERAL).Result;
                        }
                    }
                }
                else if (credenciais.GrantType == "refresh_token")
                {
                    if (!String.IsNullOrWhiteSpace(credenciais.RefreshToken))
                    {
                        RefreshTokenData refreshTokenBase = null;

                        string strTokenArmazenado =
                            _cache.GetString(credenciais.RefreshToken);
                        if (!String.IsNullOrWhiteSpace(strTokenArmazenado))
                        {
                            refreshTokenBase = JsonConvert
                                .DeserializeObject<RefreshTokenData>(strTokenArmazenado);
                        }

                        credenciaisValidas = (refreshTokenBase != null &&
                            credenciais.UserID == refreshTokenBase.UserID &&
                            credenciais.RefreshToken == refreshTokenBase.RefreshToken);

                        // Elimina o token de refresh já que um novo será gerado
                        if (credenciaisValidas)
                            _cache.Remove(credenciais.RefreshToken);
                    }
                }
            }

            return credenciaisValidas;
        }

        public Token GenerateToken(AccessCredentials credenciais)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                new GenericIdentity(credenciais.UserID, "Login"),
                new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, credenciais.UserID)
                }
            );

            DateTime dataCriacao = DateTime.Now;
            DateTime dataExpiracao = dataCriacao +
                TimeSpan.FromSeconds(_tokenConfigurations.Seconds);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _tokenConfigurations.Issuer,
                Audience = _tokenConfigurations.Audience,
                SigningCredentials = _signingConfigurations.SigningCredentials,
                Subject = identity,
                NotBefore = dataCriacao,
                Expires = dataExpiracao
            });
            var token = handler.WriteToken(securityToken);

            var resultado = new Token()
            {
                Authenticated = true,
                Created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                Expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                AccessToken = token,
                RefreshToken = Guid.NewGuid().ToString().Replace("-", String.Empty),
                Message = "OK"
            };

            var refreshTokenData = new RefreshTokenData();
            refreshTokenData.RefreshToken = resultado.RefreshToken;
            refreshTokenData.UserID = credenciais.UserID;

            TimeSpan finalExpiration =
                TimeSpan.FromSeconds(_tokenConfigurations.FinalExpiration);

            DistributedCacheEntryOptions opcoesCache =
                new DistributedCacheEntryOptions();
            opcoesCache.SetAbsoluteExpiration(finalExpiration);
                _cache.SetString(resultado.RefreshToken,
                JsonConvert.SerializeObject(refreshTokenData),
                opcoesCache);

            return resultado;
        }
    }
}
