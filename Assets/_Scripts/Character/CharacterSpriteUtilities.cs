using System.Collections.Generic;

public static class CharacterSpriteUtilities
{
    /// <summary>
    /// Canonical category names used when a sprite library follows the project naming convention.
    /// </summary>
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
        { CharacterPartType.BackDecoration, "Back_Decor" },
        { CharacterPartType.Mouth, "Mouth" }
    };

    /// <summary>
    /// Reverse lookup: canonical category name -> CharacterPartType.
    /// </summary>
    private static Dictionary<string, CharacterPartType> categoryToPartType;

    private static void EnsureReverseLookup()
    {
        if (categoryToPartType != null) return;
        categoryToPartType = new Dictionary<string, CharacterPartType>();
        foreach (var kvp in partTypeByCategoryName)
        {
            categoryToPartType[kvp.Value] = kvp.Key;
        }
    }

    /// <summary>
    /// Normalized aliases per part type. Different character sprite libraries in the project use
    /// different category spellings ("Arm L", "Arm_Left", "Arm_Dec", "Head dec", ...), so every
    /// category name is normalized (lowercase, no spaces/underscores) before matching.
    /// </summary>
    private static Dictionary<string, CharacterPartType> partTypeByNormalizedCategoryName = new Dictionary<string, CharacterPartType>
    {
        { "mount", CharacterPartType.Mount },
        { "head", CharacterPartType.Head },
        { "body", CharacterPartType.Body },
        { "eyebrow", CharacterPartType.Eyebrow },
        { "eye", CharacterPartType.Eye },
        { "arml", CharacterPartType.ArmLeft },
        { "armleft", CharacterPartType.ArmLeft },
        { "armr", CharacterPartType.ArmRight },
        { "armright", CharacterPartType.ArmRight },
        { "legr", CharacterPartType.LegRight },
        { "legright", CharacterPartType.LegRight },
        { "legl", CharacterPartType.LegLeft },
        { "legleft", CharacterPartType.LegLeft },
        { "tail", CharacterPartType.Tail },
        { "headdec", CharacterPartType.HeadDecoration },
        { "headdecor", CharacterPartType.HeadDecoration },
        { "headdecoration", CharacterPartType.HeadDecoration },
        { "armdec", CharacterPartType.ArmDecoration },
        { "armdecor", CharacterPartType.ArmDecoration },
        { "armdecoration", CharacterPartType.ArmDecoration },
        { "bodydec", CharacterPartType.BodyDecoration },
        { "bodydecor", CharacterPartType.BodyDecoration },
        { "bodydecoration", CharacterPartType.BodyDecoration },
        { "backdec", CharacterPartType.BackDecoration },
        { "backdecor", CharacterPartType.BackDecoration },
        { "backdecoration", CharacterPartType.BackDecoration },
        { "mouth", CharacterPartType.Mouth }
    };

    public static string GetCategoryNameByPartType(CharacterPartType partType)
    {
        return partTypeByCategoryName.TryGetValue(partType, out var categoryName) ? categoryName : null;
    }

    public static CharacterPartType GetPartTypeByCategoryName(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            return default;
        }

        EnsureReverseLookup();

        if (categoryToPartType.TryGetValue(categoryName, out var directMatch))
        {
            return directMatch;
        }

        var normalized = NormalizeCategoryName(categoryName);
        if (partTypeByNormalizedCategoryName.TryGetValue(normalized, out var normalizedMatch))
        {
            return normalizedMatch;
        }

        return default;
    }

    /// <summary>
    /// Lowercases the name and strips spaces/underscores so "Arm L", "arm_l" and "ArmLeft" unify.
    /// </summary>
    public static string NormalizeCategoryName(string categoryName)
    {
        var stringBuilder = new System.Text.StringBuilder(categoryName.Length);
        foreach (var character in categoryName)
        {
            if (character == ' ' || character == '_')
            {
                continue;
            }
            stringBuilder.Append(char.ToLower(character));
        }
        return stringBuilder.ToString();
    }
}
