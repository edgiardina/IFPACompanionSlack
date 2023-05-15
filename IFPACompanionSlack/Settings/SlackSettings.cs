namespace IFPACompanionSlack.Settings
{
    record SlackSettings
    {
        public string ApiToken { get; init; } = string.Empty;
        public string AppLevelToken { get; init; } = string.Empty;
        public string SigningSecret { get; init; } = string.Empty;
    }
}
