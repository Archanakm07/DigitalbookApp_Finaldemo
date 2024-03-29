﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Reader.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReaderLoginController : ControllerBase
    {

        BookAppContext db = new BookAppContext();
        private IConfiguration _config;
        public bool IsRegister { get; private set; }
        public ReaderLoginController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        [Route("login-reader")]
        public IActionResult Login(TblLogin login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login, false);
            if (user != null)
            {
                var tokenString = GenerateToken(user);
                response = Ok(new { token = tokenString });
            }
            return response;
        }
        private TblLogin AuthenticateUser(TblLogin login, bool IsRegister)
        {
            if (IsRegister)
            {
                db.TblLogins.Add(login);
                db.SaveChanges();
                return login;
            }
            else
            {
                if (db.TblLogins.Any(x => x.UserName == login.UserName && x.Password == login.Password))
                {
                    return db.TblLogins.Where(x => x.UserName == login.UserName && x.Password == login.Password).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }
        private string GenerateToken(TblLogin login)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, login.Id.ToString()),
                    new Claim(ClaimTypes.Role, login.UserName)
                }),
                Expires = DateTime.Now.AddMinutes(120),
                SigningCredentials = credentials
            };
            var TokenHandler = new JwtSecurityTokenHandler();
            var tokenGenerated = TokenHandler.CreateToken(token);
            return TokenHandler.WriteToken(tokenGenerated).ToString();
        }
        [HttpPost]
        [Route("register-reader")]
        public IActionResult Register(TblLogin login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login, true);
            if (user != null)
            {
                var tokenString = GenerateToken(user);
                response = Ok(new { token = tokenString });
            }
            return response;
        }
    }
}

