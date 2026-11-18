using UnityEngine;
using System.Collections.Generic;

public interface IBonusUi
{
    int BonusQueueCount { get; }
    bool BonusIsAdding { get; }
    void ClearBonusButtons();
    IReadOnlyList<Transform> BonusButtons { get; }
}
