using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieWorld.WebApi.Data;
using MovieWorld.WebApi.Entities;
using MovieWorld.WebApi.Models;

namespace MovieWorld.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonsController : ControllerBase
{
  private readonly MovieDbContext _context;
  private readonly IMapper _mapper;
  public PersonsController(MovieDbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  [HttpGet("getall")]
  public async Task<IActionResult> GetAllAsync(int pageIndex = 0, int pageSize = 10)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      var actorCount = _context.Persons.Count();
      var actorList = _mapper.Map<List<ActorViewModel>>(await _context.Persons.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync());

      response.Status = true;
      response.Message = "Success";
      response.Data = new { Persons = actorList, Count = actorCount };

      return Ok(response);
    }
    catch (Exception ex)
    {
      response.Status = false;
      response.Message = "Something went wrong.";

      return BadRequest(response);
    }
  }

  [HttpGet("getbyid/{id}")]
  public async Task<IActionResult> GetPersonById(int id)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      var person = await _context.Persons.Where(p => p.Id == id).FirstOrDefaultAsync();

      if (person == null)
      {
        response.Status = false;
        response.Message = "Movie does not exist.";

        return BadRequest(response);
      }

      var personData = new ActorDetailsViewModel()
      {
        Id = person.Id,
        Name = person.Name,
        DateOfBirth = person.DateOfBirth,
        Movies = await _context.Movies.Where(m => m.Actors.Contains(person)).Select(m => m.Title).ToArrayAsync()
      };

      response.Status = true;
      response.Message = "Success";
      response.Data = personData;

      return Ok(response);
    }
    catch
    {
      response.Status = false;
      response.Message = "Something went wrong.";

      return BadRequest(response);
    }
  }

  [HttpPost("add")]
  public async Task<IActionResult> AddAsync(ActorViewModel model)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      if (ModelState.IsValid)
      {
        var postedModel = new Person()
        {
          Name = model.Name,
          DateOfBirth = model.DateOfBirth
        };

        await _context.Persons.AddAsync(postedModel);
        await _context.SaveChangesAsync();

        model.Id = postedModel.Id;

        response.Status = true;
        response.Message = "Created successfully.";
        response.Data = model;

        return Ok(response);
      }
      else
      {
        response.Status = false;
        response.Message = "Validation failed.";
        response.Data = ModelState;

        return BadRequest(response);
      }
    }
    catch (Exception ex)
    {
      response.Status = false;
      response.Message = "Something went wrong.";

      return BadRequest(response);
    }
  }
}
