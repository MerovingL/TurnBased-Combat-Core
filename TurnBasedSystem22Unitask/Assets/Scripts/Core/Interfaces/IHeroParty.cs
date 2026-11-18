using System.Collections.Generic;
using UnityEngine;

public interface IHeroParty
{
    IReadOnlyList<GameObject> Heroes { get; }
    IReadOnlyList<HP> HeroHps { get; }
    IReadOnlyList<Heroes> HeroClass { get; }
}
