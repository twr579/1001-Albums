using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _1001Albums.Data;
using _1001Albums.Models;
using _1001Albums.Services.Abstract;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _1001Albums.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly AzureStorageConfig _storageConfig;

        public AlbumsController(ApplicationDbContext context, IImageService imageService, IOptions<AzureStorageConfig> storageConfig)
        {
            _context = context;
            _imageService = imageService;
            _storageConfig = storageConfig.Value;
        }

        // GET: Albums
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Album.Include(u => u.UserRatings);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Album
                .Include(u => u.UserRatings)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        [Authorize(Policy = "RequireAdministratorRole")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> Create(Album album)
        {
            IFormFile? file = album.File;

            if (!ModelState.IsValid || file == null || !_imageService.IsImage(file) || _storageConfig.AccountName == string.Empty || _storageConfig.AccountKey == string.Empty || _storageConfig.ImageContainer == string.Empty)
                return View(album);

            using (Stream stream = file.OpenReadStream())
            {
                album.ImagePath = await _imageService.UploadFileToStorage(stream, file.FileName, _storageConfig);
            }

            _context.Add(album);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Albums/Edit/5
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Album.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> Edit(int id, Album album)
        {
            if (id != album.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    album.ImagePath = _context.Album.AsNoTracking().Where(a => a.Id == id).FirstOrDefault()?.ImagePath;
                    IFormFile? file = album.File;
                    if (file != null && _imageService.IsImage(file) && file.FileName != album.ImagePath)
                    {
                        await _imageService.DeleteFileFromStorage(album.ImagePath, _storageConfig);
                        using (Stream stream = file.OpenReadStream())
                        {
                            album.ImagePath = await _imageService.UploadFileToStorage(stream, file.FileName, _storageConfig);
                        }

                    } 
                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(album);
        }

        // GET: Albums/Delete/5
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Album
                .FirstOrDefaultAsync(m => m.Id == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Album.FindAsync(id);
            if (album != null && album.ImagePath != null)
            {
                await _imageService.DeleteFileFromStorage(album.ImagePath, _storageConfig);
                _context.Album.Remove(album);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Albums/Rate/5
        [Authorize]
        public async Task<IActionResult> Rate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRating = await _context.UserRating.Include(u => u.Album).FirstOrDefaultAsync(u => u.UserId == userId && u.AlbumId == id);
            userRating ??= new UserRating { AlbumId = (int)id, UserId = userId, Rating = 0, Album = _context.Album.Find(id), };
            return View(userRating);
        }

        // POST: Albums/Rate/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Rate(int id, UserRating userRating)
        {
            if (id != userRating.AlbumId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(userRating.Rating != 0)
                    {
                        _context.Update(userRating);
                    } else
                    {
                        _context.UserRating.Remove(userRating);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserRatingExists(userRating.UserId, userRating.AlbumId))
                    {
                        if (userRating.Rating != 0)
                        {
                            _context.Add(userRating);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(userRating);
        }

        private bool AlbumExists(int id)
        {
            return _context.Album.Any(e => e.Id == id);
        }

        private bool UserRatingExists(string userId, int albumId)
        {
            return _context.UserRating.Any(e => e.UserId == userId && e.AlbumId == albumId);
        }
    }
}
