namespace OnlyVThemeCreator.Services
{
    using System;

    public interface IOptionsService
    {
        event EventHandler EpubPathChangedEvent;

        string AppWindowPlacement { get; set; }

        string EpubPath { get; set; }

        string Culture { get; set; }

        void Save();
    }
}
