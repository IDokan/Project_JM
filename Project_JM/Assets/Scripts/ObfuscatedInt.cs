// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 23/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ObfuscatedInt.cs
// Summary: XOR-obfuscated integer. The real value is never stored in plain form,
//          so memory scanners cannot locate it by searching for the expected number.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

public struct ObfuscatedInt
{
    private static readonly System.Random Rng = new System.Random();

    private int _key;
    private int _data; // always stored as (value ^ _key)

    public ObfuscatedInt(int value)
    {
        _key = Rng.Next();
        _data = value ^ _key;
    }

    public int Value
    {
        get => _data ^ _key;
        set => _data = value ^ _key;
    }
}
