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
            return View(await _context.Album.ToListAsync());
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

        private bool AlbumExists(int id)
        {
            return _context.Album.Any(e => e.Id == id);
        }
    }
}
