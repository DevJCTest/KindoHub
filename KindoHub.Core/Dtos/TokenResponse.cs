using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Dtos
{
    public class TokenResponse
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }  // Renombrar Token a AccessToken para claridad
        public string RefreshToken { get; set; }
        public string[] Roles { get; set; }
        public DateTime AccessTokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
