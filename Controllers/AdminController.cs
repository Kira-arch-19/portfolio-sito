
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Portfolio.Data;
using Portfolio.Models;
using Portfolio.Services.Interfaces;
using Portfolio.DTOs.Projects;

namespace Portfolio.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IProjectService _projectService;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IProjectService projectService,
        AppDbContext context,
        IWebHostEnvironment environment,
        ILogger<AdminController> logger)
    {
        _projectService = projectService;
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    // ==========================================
    // DASHBOARD
    // GET /admin
    // ==========================================

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<Project> projects =
            await _projectService.GetAllAsync();

        return View(projects);
    }

    // ==========================================
    // CREATE
    // GET /admin/projects/create
    // ==========================================

    [HttpGet("projects/create")]
    public IActionResult Create()
    {
        return View(new CreateProjectDto());
    }

    // POST /admin/projects/create

    [HttpPost("projects/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromForm] CreateProjectDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        Project project = new Project
        {
            Name = model.Name,
            ShortDescription = model.ShortDescription,
            Description = model.Description,
            Technologies = model.Technologies,
            GithubLink = model.GithubLink,
            SkillImproved = model.SkillImproved
        };

        try
        {
            await _projectService.CreateAsync(project);

            TempData["SuccessMessage"] =
                "Progetto creato correttamente!";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                nameof(model.Name), ex.Message);

            return View(model);
        }
    }

    // ==========================================
    // UPDATE
    // GET /admin/projects/2/edit
    // ==========================================

    [HttpGet("projects/{id:int}/edit")]
    public async Task<IActionResult> Update(int id)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

        Project? project =
            await _projectService.GetByIdAsync(id);

        if (project == null)
            return NotFound();

        UpdateProjectDto model = new UpdateProjectDto
        {
            Name = project.Name,
            ShortDescription = project.ShortDescription,
            Description = project.Description,
            Technologies = project.Technologies,
            GithubLink = project.GithubLink,
            SkillImproved = project.SkillImproved
        };

        ViewBag.ProjectId = id;

        return View(model);
    }

    // POST /admin/projects/2/edit

    [HttpPost("projects/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] UpdateProjectDto model)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

        ViewBag.ProjectId = id;

        if (!ModelState.IsValid)
            return View(model);

        Project project = new Project
        {
            Name = model.Name,
            ShortDescription = model.ShortDescription,
            Description = model.Description,
            Technologies = model.Technologies,
            GithubLink = model.GithubLink,
            SkillImproved = model.SkillImproved
        };

        Project? updated =
            await _projectService.UpdateAsync(id, project);

        if (updated == null)
            return NotFound();

        TempData["SuccessMessage"] =
            "Progetto aggiornato correttamente!";

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // DELETE
    // GET /admin/projects/2/delete
    // ==========================================

    [HttpGet("projects/{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

        Project? project =
            await _projectService.GetByIdAsync(id);

        if (project == null)
            return NotFound();

        return View(project);
    }

    // POST /admin/projects/2/delete

    [HttpPost("projects/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

        Project? project =
            await _projectService.GetByIdAsync(id);

        if (project == null)
            return NotFound();

        // Conserva i percorsi prima della cancellazione
        List<string> imageUrls = project.Images
            .Select(i => i.ImageUrl)
            .ToList();

        bool deleted =
            await _projectService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        // Elimina i file fisici dopo il database
        foreach (string url in imageUrls)
        {
            DeleteStoredFile(id, url);
        }

        TempData["SuccessMessage"] =
            "Progetto eliminato correttamente!";

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // GESTIONE IMMAGINI
    // GET /admin/projects/2/images
    // ==========================================

    [HttpGet("projects/{id:int}/images")]
    public async Task<IActionResult> Images(int id)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

        Project? project =
            await _projectService.GetByIdAsync(id);

        if (project == null)
            return NotFound();

        return View(project);
    }

    // ==========================================
    // UPLOAD IMMAGINI
    // POST /admin/projects/2/images
    // ==========================================

    [HttpPost("projects/{id:int}/images")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 60_000_000)]
    public async Task<IActionResult> Images(
        int id,
        [FromForm] IFormFile? cover,
        [FromForm] List<IFormFile>? gallery)
    {
        if (id <= 0)
            return BadRequest("ID non valido");

        Project? project = await _context.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        // Rimuove file vuoti dalla selezione
        gallery = gallery?
            .Where(f => f.Length > 0)
            .ToList() ?? new List<IFormFile>();

        bool hasCover = cover != null && cover.Length > 0;

        if (!hasCover && gallery.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Seleziona almeno un'immagine.");

            return View(project);
        }

        // Limiti galleria
        int existingGallery = project.Images
            .Count(i => !i.IsCover);

        if (gallery.Count > 10 ||
            existingGallery + gallery.Count > 30)
        {
            ModelState.AddModelError(
                "",
                "Puoi caricare massimo 10 immagini alla volta " +
                "e 30 immagini complessive nella galleria.");

            return View(project);
        }

        // ======================================
        // VALIDAZIONE FILE
        // ======================================

        List<IFormFile> files = new();

        if (hasCover && cover != null)
            files.Add(cover);

        files.AddRange(gallery);

        foreach (IFormFile file in files)
        {
            if (!await IsValidImageAsync(file))
            {
                ModelState.AddModelError(
                    "",
                    $"Il file {file.FileName} non è valido. " +
                    "Sono accettati JPG, PNG e WebP " +
                    "fino a 5 MB.");

                return View(project);
            }
        }

        // ======================================
        // SALVATAGGIO
        // ======================================

        List<string> savedUrls = new();
        string? oldCoverUrl = null;
        bool completed = false;

        try
        {
            // COPERTINA
            if (hasCover && cover != null)
            {
                string url = await SaveImageAsync(cover, id);

                savedUrls.Add(url);

                ProjectImage? oldCover = project.Images
                    .FirstOrDefault(i => i.IsCover);

                if (oldCover != null)
                {
                    oldCoverUrl = oldCover.ImageUrl;

                    _context.ProjectImages.Remove(oldCover);
                }

                ProjectImage coverImage = new ProjectImage
                {
                    ProjectId = id,
                    ImageUrl = url,
                    IsCover = true,
                    Position = 0
                };

                _context.ProjectImages.Add(coverImage);
            }

            // GALLERIA
            int nextPosition = project.Images
                .Where(i => !i.IsCover)
                .Select(i => i.Position)
                .DefaultIfEmpty(-1)
                .Max() + 1;

            foreach (IFormFile file in gallery)
            {
                string url = await SaveImageAsync(file, id);

                savedUrls.Add(url);

                ProjectImage image = new ProjectImage
                {
                    ProjectId = id,
                    ImageUrl = url,
                    IsCover = false,
                    Position = nextPosition++
                };

                _context.ProjectImages.Add(image);
            }

            // Salva i riferimenti nel database
            await _context.SaveChangesAsync();

            completed = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Errore durante l'upload delle immagini del progetto {ProjectId}",
                id);

            ModelState.AddModelError(
                "",
                "Errore durante il caricamento delle immagini.");

            return View(project);
        }
        finally
        {
            // Se il database non è stato aggiornato,
            // elimina i nuovi file già scritti
            if (!completed)
            {
                foreach (string url in savedUrls)
                {
                    DeleteStoredFile(id, url);
                }
            }
        }

        // Rimuove la vecchia copertina solo dopo
        // il salvataggio della nuova
        if (oldCoverUrl != null)
        {
            DeleteStoredFile(id, oldCoverUrl);
        }

        TempData["SuccessMessage"] =
            "Immagini caricate correttamente!";

        return RedirectToAction(
            nameof(Images),
            new { id });
    }

    // ==========================================
    // ELIMINA SINGOLA IMMAGINE
    // POST /admin/projects/2/images/5/delete
    // ==========================================

    [HttpPost("projects/{id:int}/images/{imageId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(
        int id,
        int imageId)
    {
        if (id <= 0 || imageId <= 0)
            return BadRequest("ID non valido");

        ProjectImage? image =
            await _context.ProjectImages
                .FirstOrDefaultAsync(i =>
                    i.Id == imageId &&
                    i.ProjectId == id);

        if (image == null)
            return NotFound();

        string imageUrl = image.ImageUrl;

        _context.ProjectImages.Remove(image);

        await _context.SaveChangesAsync();

        // Eliminazione fisica del file
        DeleteStoredFile(id, imageUrl);

        TempData["SuccessMessage"] =
            "Immagine eliminata correttamente!";

        return RedirectToAction(
            nameof(Images),
            new { id });
    }

    // ==========================================
    // HELPER - VALIDAZIONE IMMAGINE
    // ==========================================

    private static async Task<bool> IsValidImageAsync(
        IFormFile file)
    {
        const long maxSize = 5 * 1024 * 1024;

        if (file.Length < 12 || file.Length > maxSize)
            return false;

        string extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension is not (
            ".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            return false;
        }

        byte[] header = new byte[12];

        try
        {
            await using Stream stream =
                file.OpenReadStream();

            await stream.ReadExactlyAsync(header);
        }
        catch (IOException)
        {
            return false;
        }

        return extension switch
        {
            ".jpg" or ".jpeg" =>
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[]
                    {
                        137, 80, 78, 71, 13, 10, 26, 10
                    }),

            ".webp" =>
                header.AsSpan(0, 4)
                    .SequenceEqual("RIFF"u8) &&
                header.AsSpan(8, 4)
                    .SequenceEqual("WEBP"u8),

            _ => false
        };
    }

    // ==========================================
    // HELPER - SALVATAGGIO FISICO IMMAGINE
    // ==========================================

    private async Task<string> SaveImageAsync(
        IFormFile file,
        int projectId)
    {
        string webRoot = _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        string folder = Path.Combine(
            webRoot,
            "uploads",
            "projects",
            projectId.ToString());

        Directory.CreateDirectory(folder);

        // Nome casuale per evitare collisioni
        string extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        string fileName =
            $"{Guid.NewGuid():N}{extension}";

        string physicalPath =
            Path.Combine(folder, fileName);

        string imageUrl =
            $"/uploads/projects/{projectId}/{fileName}";

        try
        {
            await using FileStream stream = new(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write);

            await file.CopyToAsync(stream);
        }
        catch
        {
            // Pulizia di eventuali file parziali
            DeleteStoredFile(projectId, imageUrl);
            throw;
        }

        return imageUrl;
    }

    // ==========================================
    // HELPER - ELIMINAZIONE FILE FISICO
    // ==========================================

    private void DeleteStoredFile(
        int projectId,
        string imageUrl)
    {
        string expectedPrefix =
            $"/uploads/projects/{projectId}/";

        // Consente la cancellazione solo
        // all'interno della cartella del progetto
        if (!imageUrl.StartsWith(
            expectedPrefix,
            StringComparison.Ordinal))
        {
            return;
        }

        string webRoot = _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        string physicalPath = Path.Combine(
            webRoot,
            "uploads",
            "projects",
            projectId.ToString(),
            Path.GetFileName(imageUrl));

        try
        {
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(
                ex,
                "Impossibile eliminare il file {FilePath}",
                physicalPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                ex,
                "Accesso negato durante l'eliminazione del file {FilePath}",
                physicalPath);
        }
    }
}
