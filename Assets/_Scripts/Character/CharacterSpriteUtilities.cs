using System.Collections.Generic;

public static class CharacterSpriteUtilities
{
    private static Dictionary<CharacterPartType, string> partTypeByCategoryName = new Dictionary<CharacterPartType, string>
    {
        { CharacterPartType.Mount, "Mount" },
        { CharacterPartType.Head, "Head" },
        { CharacterPartType.Body, "Body" },
        { CharacterPartType.Eyebrow, "Eyebrow" },
        { CharacterPartType.Eye, "Eye" },
        { CharacterPartType.ArmLeft, "Arm_Left" },
        { CharacterPartType.ArmRight, "Arm_Right" },
        { CharacterPartType.LegRight, "Leg_Right" },
        { CharacterPartType.LegLeft, "Leg_Left" },
        { CharacterPartType.Tail, "Tail" },
        { CharacterPartType.HeadDecoration, "Head_Decor" },
        { CharacterPartType.ArmDecoration, "Arm_Decor" },
        { CharacterPartType.BodyDecoration, "Body_Decor" },
        { CharacterPartType.BackDecoration, "Back_Decor" }
    };

    public static string GetCategoryNameByPartType(CharacterPartType partType)
    {
        return partTypeByCategoryName.TryGetValue(partType, out var categoryName) ? categoryName : null;
    }

    public static CharacterPartType GetPartTypeByCategoryName(string categoryName)
    {
        foreach (var kvp in partTypeByCategoryName)
        {
            if (kvp.Value == categoryName)
            {
                return kvp.Key;
            }
        }
        return default;
    }
}
