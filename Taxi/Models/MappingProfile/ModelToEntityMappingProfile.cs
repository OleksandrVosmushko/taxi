﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models.Customers;
using Taxi.Models.Drivers;

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

            CreateMap<Customer, CustomerDto>().ForMember(x => x.Id, map => map.MapFrom(vm => vm.Id));

            CreateMap<AppUser, CustomerDto>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Driver, DriverDto>().ForMember(x => x.Id, map => map.MapFrom(vm => vm.Id));

            CreateMap<AppUser, DriverDto>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<CustomerRegistrationDto, CustomerDto>();

            CreateMap<DriverRegistrationDto, DriverDto>();

            CreateMap<CustomerUpdateDto, AppUser>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<DriverUpdateDto, AppUser>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); 

            CreateMap<CustomerUpdateDto, Customer>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); 

            CreateMap<DriverUpdateDto, Driver>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Customer, Driver>().ForMember(x => x.Id, opt => opt.Ignore()); 

            CreateMap<CustomerDriverUpgradeDto, Driver>();


        }
    }
}
