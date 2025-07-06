namespace BetaUni.Other
{
    //Classe creata che contiene proprietà di jwt
    public class JWTSettings
    {
        public string? SecretKey { get; set; }
        public string? Issuer { get; set; }
        public string? AudienceP { get; set; }
        public string? AudienceS { get; set; }
        public int ExpirationMinutes { get; set; }
    }
}
