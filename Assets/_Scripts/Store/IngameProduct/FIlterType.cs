using System;

[Flags]
public enum FilterType
{
    None = 0 << 0,
    New = 1 << 0,
    Season = 1 << 1,
    Coin = 1 << 2,
    Gem = 1 << 3,

    Any = New | Season | Coin | Gem,  // All flags combined
}