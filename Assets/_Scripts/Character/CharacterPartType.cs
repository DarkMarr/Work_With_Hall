public enum CharacterPartType
{
    Mount,
    Head,
    Body,
    Eyebrow,
    Eye,
    ArmLeft,
    ArmRight,
    LegRight,
    LegLeft,
    Tail,
    HeadDecoration,
    ArmDecoration,
    BodyDecoration,
    BackDecoration,
    Mouth,

    // Appended for the fashion sets, which dress each arm separately and add a held prop.
    // Never reorder this enum: CharacterSpriteMixerCategory serializes PartType, so Unity
    // stores these as ordinals and inserting a value would silently re-point existing data.
    ArmDecorationLeft,
    ArmDecorationRight,
    Prop
}
