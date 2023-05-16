namespace IFPACompanionSlack.Settings
{
    public record SlackSettings
    {
        public string ApiToken { get; init; } = string.Empty;
        public string AppLevelToken { get; init; } = string.Empty;
        public string SigningSecret { get; init; } = string.Empty;
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
    }
}
