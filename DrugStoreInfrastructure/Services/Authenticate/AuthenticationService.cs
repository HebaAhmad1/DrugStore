using AutoMapper;
using DrugStore.Data;
using DrugStoreCore.Constant;
using DrugStoreCore.Dto;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace DrugStoreInfrastructure.Services.Authenticate
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly DrugStoreDbContext _db;
        private readonly UserManager<Pharmacy> _userManager;
        private readonly IMapper _Mapper;

        public AuthenticationService(DrugStoreDbContext db, IMapper Mapper,
                                     UserManager<Pharmacy> userManager)
        {
            _db = db;
            _userManager = userManager;
            _Mapper = Mapper;
        }

        // Return Pharmacy Information By UserName
        public PharmacyInfo PharmaceInfo(string userName)
        {
            var pharmacyInfo =  _db.Users.Where(phar => phar.UserName == userName)
                .Select(phar => new PharmacyInfo()
                {
                    PharmacyName = phar.PharmacyName,
                    PharmacyRole = phar.PharmacyRole,
                    PharmacyId = phar.Id,
                    Address = phar.Address,
                    AccountNumber = phar.AccountNumber.ToString()
                }).SingleOrDefault();
            if(pharmacyInfo is null)
            {
                return null;
            }
            return pharmacyInfo;
        }
        public async Task<ResponseViewModel<PharmacyInfoViewModel>> Login(PharmacyLoginDto dto)
        {
            if (dto.AccountNumber is null || dto.Password is null)
            {
                return new ResponseViewModel<PharmacyInfoViewModel>(false, "UserName Or Password Is Required", null);
            }
            var pharmacy = await _db.Users.SingleOrDefaultAsync(a => a.UserName == dto.AccountNumber);
            if (pharmacy is null)
            {
                return new ResponseViewModel<PharmacyInfoViewModel>(false, "Invalid UserName Or Password", null);
            }
            var CheckPass = await _userManager.CheckPasswordAsync(pharmacy, dto.Password);
            if (!CheckPass)
            {
                return new ResponseViewModel<PharmacyInfoViewModel>(false, "Invalid UserName Or Password", null);
            }

            var zzzz = await GenerateAccessToken(pharmacy);
            return new ResponseViewModel<PharmacyInfoViewModel>
            (
                true,
                "Welcom " + pharmacy.PharmacyName,
                new PharmacyInfoViewModel()
                {
                    PharmacyInformation = _Mapper.Map<PharmacyViewModel>(pharmacy),
                    TokenInformation = await GenerateAccessToken(pharmacy)
                }
            );
        }
        public async Task<AccessTokenViewModel> GenerateAccessToken(Pharmacy pharmacy)
        {
            // Create User Informations(Claims)
            var claims = new List<Claim>(){
            new Claim(JwtRegisteredClaimNames.Sub, pharmacy.UserName),
            new Claim(ClaimTypes.Role, pharmacy.PharmacyRole),//To Know Pharmacy Role In Authorize
            //new Claim(JwtRegisteredClaimNames.Email, pharmacy.Email),
            new Claim(PharmacyTokenInfo.PharmacyId, pharmacy.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("djksjkccyjkdvujkkjscui"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(10);
            var accessToken = new JwtSecurityToken("https://localhost:44303/",
                "https://localhost:44303/",
            claims,
            expires: expires,
            signingCredentials: credentials
            );
            string AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
            return new AccessTokenViewModel()
            {
                BearerToken = AccessToken,
                ExpiringDate = expires
            };
        }
    }
}
