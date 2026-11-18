public interface IManualService
{
    void RunManual();
    void RunBaseAttackManual();
    bool MoveByMouseManualState { get; }
    bool SkipManual { get; }
}
