namespace OnlyV.Mappings
{
    using AutoMapper;
    using Services.Monitors;
    using ViewModel;

    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SystemMonitor, MonitorItem>();
        }
    }
}
