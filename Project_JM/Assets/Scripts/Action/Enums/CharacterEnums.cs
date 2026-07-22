// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterEnums.cs
// Summary: Enum of character identities (party + each enemy type), used to key
//          per-character data (e.g. per-enemy defeat counts) at compile time
//          instead of by free-text name.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

namespace CharacterEnums
{
    public enum CharacterId
    {
        Unassigned = 0,
        Troop,
        SlimeKing,
        AntGladiator,
        MushroomBully,
        FoxThief,
        SnailWizard,
        DandelionToad,
        FlyingFish
    }
}
