using AutoMapper;
using MovieWorld.WebApi.Entities;
using MovieWorld.WebApi.Models;

namespace MovieWorld.WebApi;

public class MappingProfiles : Profile
{
  public MappingProfiles()
  {
    CreateMap<Movie, MovieListViewModel>();
    CreateMap<Movie, MovieDetailsViewModel>();
    CreateMap<MovieListViewModel, Movie>();
    CreateMap<CreateMovieViewModel, Movie>().ForMember(m => m.Actors, opt => opt.Ignore());

    CreateMap<Person, ActorViewModel>();
    CreateMap<Person, ActorDetailsViewModel>();
    CreateMap<ActorViewModel, Person>();
  }
}
