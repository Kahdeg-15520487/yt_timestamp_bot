namespace discordbot.Services.Interfaces
{
    public interface ITokenAuthentication
    {
        string GetToken();
        bool ValidateToken(string token);
    }
}
