﻿using System.ComponentModel.DataAnnotations;
using NetFusion.Core.Settings;

namespace NetFusion.Core.UnitTests.Settings.Mocks;

[ConfigurationSection("App:MainWindow")]
public class MockSettings : IAppSettings
{
    public int Height { get; set; } = 1000;
    public int Width { get; set; } = 2000;
}

public class MockDerivedSettings: MockSettings
{
    public Dialog Dialog { get; set; }
}

public class Dialog
{
    public Colors Colors { get; set;}
}

public class Colors {
    public string Frame { get; set; }
    public string Title { get; set; }
}

public class MockInvalidSettings : MockSettings
{
    [Range(5, 100, ErrorMessage = "Invalid Range")]
    public int ValidatedValue { get; set; }
}