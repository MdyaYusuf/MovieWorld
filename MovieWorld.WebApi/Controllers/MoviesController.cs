using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieWorld.WebApi.Data;
using MovieWorld.WebApi.Entities;
using MovieWorld.WebApi.Models;

namespace MovieWorld.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
  private readonly MovieDbContext _context;
  public MoviesController(MovieDbContext context)
  {
    _context = context;    
  }

  [HttpGet("getall")]
  public async Task<IActionResult> GetAllAsync(int pageIndex = 0, int pageSize = 10)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      var movieCount = _context.Movies.Count();
      var movieList = await _context.Movies.Include(m => m.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

      response.Status = true;
      response.Message = "Success";
      response.Data = new { Movies = movieList, Count = movieCount };

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
  public async Task<IActionResult> GetMovieById(int id)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      var movie = await _context.Movies.Include(m => m.Actors).Where(m => m.Id == id).FirstOrDefaultAsync();

      if (movie == null)
      {
        response.Status = false;
        response.Message = "Movie does not exist.";

        return BadRequest(response);
      }

      response.Status = true;
      response.Message = "Success";
      response.Data = movie;

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
  public async Task<IActionResult> AddAsync(CreateMovieViewModel model)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      if (ModelState.IsValid)
      {
        var actors = await _context.Persons.Where(p => model.Actors.Contains(p.Id)).ToListAsync();

        if (actors.Count != model.Actors.Count)
        {
          response.Status = false;
          response.Message = "Invalid actors assigned.";

          return BadRequest(response);
        }

        var postedModel = new Movie()
        {
          Title = model.Title,
          Description = model.Description,
          Language = model.Language,
          ReleaseDate = model.ReleaseDate,
          CoverImage = model.CoverImage,
          Actors = actors
        };

        await _context.Movies.AddAsync(postedModel);
        await _context.SaveChangesAsync();

        response.Status = true;
        response.Message = "Created successfully.";
        response.Data = postedModel;

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

  [HttpPut("update")]
  public async Task<IActionResult> UpdateAsync(UpdateMovieViewModel model)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      if (ModelState.IsValid)
      {
        if (model.Id <= 0)
        {
          response.Status = false;
          response.Message = "Invalid movie record.";

          return BadRequest(response);
        }
        var actors = await _context.Persons.Where(p => model.Actors.Contains(p.Id)).ToListAsync();

        if (actors.Count != model.Actors.Count)
        {
          response.Status = false;
          response.Message = "Invalid actors assigned.";

          return BadRequest(response);
        }

        var movie = await _context.Movies.Include(m => m.Actors).Where(m => m.Id == model.Id).FirstOrDefaultAsync();

        if (movie == null)
        {
          response.Status = false;
          response.Message = "Invalid movie record.";

          return BadRequest(response);
        }

        movie.Title = model.Title;
        movie.Description = movie.Description;
        movie.Language = movie.Language;
        movie.ReleaseDate = movie.ReleaseDate;
        movie.CoverImage = model.CoverImage;

        var removedActors = movie.Actors.Where(p => !model.Actors.Contains(p.Id)).ToList();

        foreach (var actor in removedActors)
        {
          movie.Actors.Remove(actor);
        }

        var addedActors = actors.Except(movie.Actors).ToList();

        foreach (var actor in removedActors)
        {
          movie.Actors.Add(actor);
        }

        await _context.SaveChangesAsync();

        response.Status = true;
        response.Message = "Updated successfully.";
        response.Data = movie;

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
