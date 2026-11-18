public interface IDeveloperSettings
{
    bool SkipManual { get; }
    bool SkipBattlePresentation { get; }
    bool DoNotDetectEnemies { get; }
    bool IsAiDisabled { get; }
}
