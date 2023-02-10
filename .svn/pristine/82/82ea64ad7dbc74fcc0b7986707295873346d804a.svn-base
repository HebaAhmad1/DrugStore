using AutoMapper;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using System;

namespace MAFInfrastructure.AutoMapper
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            //Order Service
            CreateMap<CreateOrderDto, OrderDetails>();
            CreateMap<OrderDetails, OrderViewModel>();
            CreateMap<UpdateOrderDto, OrderDetails>();
            //Drug Service
            CreateMap<Drug, DrugDetailsViewModel>();
            //Authentication Service
            CreateMap<Pharmacy, PharmacyViewModel>();
            //DispalyOrders Service
            CreateMap<OrderDetails, CurrentAndArchivedOrdersViewModel>();
            CreateMap<PharmacyOrders, PharmacyOrdersViewModel>();
            CreateMap<Pharmacy, PharmacyViewModel>();
        }

    }
}
