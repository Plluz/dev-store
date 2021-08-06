using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using DevStore.Core.Communication;
using DevStore.WebAPI.Core.Usuario;
using DevStore.WebApp.MVC.Extensions;
using DevStore.WebApp.MVC.Models;

namespace DevStore.WebApp.MVC.Services
{
    public interface IAutenticacaoService
    {
        Task<UsuarioRespostaLogin> Login(UsuarioLogin usuarioLogin);

        Task<UsuarioRespostaLogin> Registro(UsuarioRegistro usuarioRegistro);

        Task RealizarLogin(UsuarioRespostaLogin resposta);
        Task Logout();

        bool TokenExpirado();

        Task<bool> RefreshTokenValido();
    }

    public class AutenticacaoService : Service, IAutenticacaoService
    {
        private readonly HttpClient _httpClient;

        private readonly IAspNetUser _user;
        private readonly IAuthenticationService _authenticatioDevStorervice;

        public AutenticacaoService(HttpClient httpClient, 
                                   IOptions<AppSettings> settings, 
                                   IAspNetUser user, 
                                   IAuthenticationService authenticatioDevStorervice)
        {
            httpClient.BaseAddress = new Uri(settings.Value.AutenticacaoUrl);

            _httpClient = httpClient;
            _user = user;
            _authenticatioDevStorervice = authenticatioDevStorervice;
        }

        public async Task<UsuarioRespostaLogin> Login(UsuarioLogin usuarioLogin)
        {
            var loginContent = ObterConteudo(usuarioLogin);

            var Response = await _httpClient.PostAsync("/api/identidade/autenticar", loginContent);

            if (!ManageResponseErrors(Response))
            {
                return new UsuarioRespostaLogin
                {
                    ResponseResult = await DeserializeResponse<ResponseResult>(Response)
                };
            }

            return await DeserializeResponse<UsuarioRespostaLogin>(Response);
        }

        public async Task<UsuarioRespostaLogin> Registro(UsuarioRegistro usuarioRegistro)
        {
            var registroContent = ObterConteudo(usuarioRegistro);

            var Response = await _httpClient.PostAsync("/api/identidade/nova-conta", registroContent);

            if (!ManageResponseErrors(Response))
            {
                return new UsuarioRespostaLogin
                {
                    ResponseResult = await DeserializeResponse<ResponseResult>(Response)
                };
            }

            return await DeserializeResponse<UsuarioRespostaLogin>(Response);
        }

        public async Task<UsuarioRespostaLogin> UtilizarRefreshToken(string refreshToken)
        {
            var refreshTokenContent = ObterConteudo(refreshToken);

            var Response = await _httpClient.PostAsync("/api/identidade/refresh-token", refreshTokenContent);

            if (!ManageResponseErrors(Response))
            {
                return new UsuarioRespostaLogin
                {
                    ResponseResult = await DeserializeResponse<ResponseResult>(Response)
                };
            }

            return await DeserializeResponse<UsuarioRespostaLogin>(Response);
        }

        public async Task RealizarLogin(UsuarioRespostaLogin resposta)
        {
            var token = ObterTokenFormatado(resposta.AccessToken);

            var claims = new List<Claim>();
            claims.Add(new Claim("JWT", resposta.AccessToken));
            claims.Add(new Claim("RefreshToken", resposta.RefreshToken));
            claims.AddRange(token.Claims);

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                IsPersistent = true
            };

            await _authenticatioDevStorervice.SignInAsync(
                _user.ObterHttpContext(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task Logout()
        {
            await _authenticatioDevStorervice.SignOutAsync(
                _user.ObterHttpContext(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                null);
        }

        public static JwtSecurityToken ObterTokenFormatado(string jwtToken)
        {
            return new JwtSecurityTokenHandler().ReadToken(jwtToken) as JwtSecurityToken;
        }

        public bool TokenExpirado()
        {
            var jwt = _user.ObterUserToken();
            if (jwt is null) return false;

            var token = ObterTokenFormatado(jwt);
            return token.ValidTo.ToLocalTime() < DateTime.Now;
        }

        public async Task<bool> RefreshTokenValido()
        {
            var resposta = await UtilizarRefreshToken(_user.ObterUserRefreshToken());

            if (resposta.AccessToken != null && resposta.ResponseResult == null)
            {
                await RealizarLogin(resposta);
                return true;
            }

            return false;
        }
    }
}
