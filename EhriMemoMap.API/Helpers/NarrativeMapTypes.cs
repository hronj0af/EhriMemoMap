namespace EhriMemoMap.API.Helpers;

/// <summary>Typy narativních map (narrative_maps.type → list_items).</summary>
public static class NarrativeMapTypes
{
    /// <summary>Příběhová mapa – kurátorovaný příběh s texty</summary>
    public const long StoryMap = 5;

    /// <summary>Osobní trajektorie – automaticky sestavená místa ze života osoby (Velké Meziříčí)</summary>
    public const long PersonalTrajectory = 6;
}
