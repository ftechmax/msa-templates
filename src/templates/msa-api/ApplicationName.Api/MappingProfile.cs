﻿using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Contracts.Dtos;
using AutoMapper;

namespace ApplicationName.Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateExampleDto, CreateExampleCommand>();
        CreateMap<UpdateExampleDto, UpdateExampleCommand>();
        CreateMap<ExampleDocument, ExampleDetailsDto>();
        CreateMap<ExampleDocument, ExampleCollectionDto>();
    }
}