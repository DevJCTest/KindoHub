using KindoHub.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Core.Interfaces
{
    public interface ITokenService
    {
        TokenDto GenerarToken(string username, string[] roles, string[] permissions);
        Task<TokenDto> RefrescarToken();
        void SetRefreshTokenCookie(TokenDto tokenResponse);
        void ClearRefreshTokenCookie();
    }
}
