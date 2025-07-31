using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieWorld.WebApi.Data;
using MovieWorld.WebApi.Entities;
using MovieWorld.WebApi.Models;
using System.Net.Http.Headers;

namespace MovieWorld.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
  private readonly MovieDbContext _context;
  private readonly IMapper _mapper;
  public MoviesController(MovieDbContext context, IMapper mapper)
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
      var movieCount = _context.Movies.Count();
      var movieList = _mapper.Map<List<MovieListViewModel>>(await _context.Movies.Include(m => m.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToListAsync());

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

      var movieData = _mapper.Map<MovieDetailsViewModel>(movie);

      response.Status = true;
      response.Message = "Success";
      response.Data = movieData;

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

        var postedModel = _mapper.Map<Movie>(model);
        postedModel.Actors = actors;

        await _context.Movies.AddAsync(postedModel);
        await _context.SaveChangesAsync();

        var responseData = _mapper.Map<MovieDetailsViewModel>(postedModel);

        response.Status = true;
        response.Message = "Created successfully.";
        response.Data = responseData;

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

        var responseData = new MovieDetailsViewModel
        {
          Id = movie.Id,
          Title = movie.Title,
          Description = movie.Description,
          Actors = movie.Actors.Select(a => new ActorViewModel
          {
            Id = a.Id,
            Name = a.Name,
            DateOfBirth = a.DateOfBirth
          }).ToList(),
          Language = movie.Language,
          ReleaseDate = movie.ReleaseDate,
          CoverImage = movie.CoverImage
        };

        response.Status = true;
        response.Message = "Updated successfully.";
        response.Data = responseData;

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

  [HttpDelete("delete")]
  public async Task<IActionResult> RemoveAsync(int id)
  {
    BaseResponseModel response = new BaseResponseModel();

    try
    {
      var movie = await _context.Movies.Where(m => m.Id == id).FirstOrDefaultAsync();

      if (movie == null)
      {
        response.Status = false;
        response.Message = "Invalid movie record.";

        return BadRequest(response);
      }

      _context.Movies.Remove(movie);
      await _context.SaveChangesAsync();

      response.Status = true;
      response.Message = "Deleted successfully.";

      return Ok(response);
    }
    catch (Exception ex)
    {
      response.Status = false;
      response.Message = "Something went wrong.";

      return BadRequest(response);
    }
  }

  [HttpPost("upload")]
  public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile)
  {
    try
    {
      var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
      string newPath = @"C:\Users\Yusuf\Desktop\Web\MovieWorld";

      if (!Directory.Exists(newPath))
      {
        Directory.CreateDirectory(newPath);
      }

      string[] allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png" };

      if (!allowedImageExtensions.Contains(Path.GetExtension(filename)))
      {
        return BadRequest(new BaseResponseModel()
        {
          Status = false,
          Message = "Only .jpg, .jpeg, .png type files are allowed."
        });
      }

      string newFileName = Guid.NewGuid() + Path.GetExtension(filename);
      string fullFilePath = Path.Combine(newPath, newFileName);

      using (var stream = new FileStream(fullFilePath, FileMode.Create))
      {
        await imageFile.CopyToAsync(stream);
      }

      return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}" });
    }
    catch (Exception ex)
    {
      return BadRequest(new BaseResponseModel()
      {
        Status = false,
        Message = "Error occured."
      });
    }
  }
}
