﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Models.MappingProfile
{
    public class ModelToEntityMappingProfile : Profile
    {
        public ModelToEntityMappingProfile()
        {
            CreateMap<CustomerRegistrationDto, AppUser>().ForMember(au => au.UserName, map => map.MapFrom(vm => vm.Email));

            CreateMap<DriverRegistrationDto, AppUser>().ForMember(au => au.UserName, map => map.MapFrom(vm => vm.Email));

            CreateMap<DriverRegistrationDto, Driver>();

            CreateMap<CustomerRegistrationDto, Customer>();

            CreateMap<Customer, CustomerDto>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<AppUser, CustomerDto>().ForMember(x => x.Id, map => map.MapFrom(vm => vm.Id));

            CreateMap<Driver, DriverDto>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<AppUser, DriverDto>().ForMember(x => x.Id, map => map.MapFrom(vm => vm.Id));

            CreateMap<CustomerRegistrationDto, CustomerDto>();

            CreateMap<DriverRegistrationDto, DriverDto>();

        }
    }
}
