using Application.DTOs.Product.Requests;
using Application.DTOs.Product.Responses;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<Product, GetAllProductsResponse>().ReverseMap();
            CreateMap<CreateProductRequest, Product>();
            CreateMap<Product, ProductResponse>();
            CreateMap<GetAllProductsRequest, GetAllProductsParameter>();
        }
    }
}
