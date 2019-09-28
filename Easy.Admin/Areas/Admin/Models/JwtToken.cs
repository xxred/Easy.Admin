namespace Easy.Admin.Areas.Admin.Models
{
    public class JwtToken
    {
        public JwtToken()
        {

        }

        public JwtToken(string token)
        {
            Token = token;
        }

        public string Token { get; set; }
    }
}
