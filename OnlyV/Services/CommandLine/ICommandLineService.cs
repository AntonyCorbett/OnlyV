﻿namespace OnlyV.Services.CommandLine
{
    internal interface ICommandLineService
    {
        bool NoGpu { get; set; }

        string OptionsIdentifier { get; set; }

        bool NoSettings { get; set; }
    }
}
