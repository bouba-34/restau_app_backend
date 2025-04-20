using AutoMapper;
using backend.Api.Controllers;
using backend.Api.Models.DTOs.Auth;
using backend.Api.Models.DTOs.Menu;
using backend.Api.Models.DTOs.Order;
using backend.Api.Models.DTOs.Reservation;
using backend.Api.Models.Entities;

namespace backend.Api.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, RegisterRequest>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();

            
            // Menu mappings
            CreateMap<MenuCategory, MenuCategoryDto>().ReverseMap();
            CreateMap<CreateMenuCategoryDto, MenuCategory>();
            CreateMap<UpdateMenuCategoryDto, MenuCategory>();
            
            CreateMap<MenuItem, MenuItemDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<CreateMenuItemDto, MenuItem>();
            CreateMap<UpdateMenuItemDto, MenuItem>();
            
            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Username))
                .ForMember(dest => dest.PreparedByName, opt => opt.MapFrom(src => src.PreparedBy != null ? src.PreparedBy.Username : null));
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItem.Name))
                .ForMember(dest => dest.MenuItemImageUrl, opt => opt.MapFrom(src => src.MenuItem.ImageUrl));
            CreateMap<CreateOrderDto, Order>();
            CreateMap<CreateOrderItemDto, OrderItem>();
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Username));
            
            // Reservation mappings
            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Username));
            CreateMap<CreateReservationDto, Reservation>();
            CreateMap<UpdateReservationDto, Reservation>();
            CreateMap<Reservation, ReservationSummaryDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Username));
            CreateMap<Table, AvailableTableDto>();
        }
    }
}