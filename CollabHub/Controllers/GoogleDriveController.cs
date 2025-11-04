using CollabHub.Dtos;
using CollabHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabHub.Controllers;

[Authorize]
public class GoogleDriveController : Controller
{
    private readonly IGoogleDriveService _drive;

    public GoogleDriveController(IGoogleDriveService drive)
    {
        _drive = drive;
    }

    public async Task<IActionResult> Index()
    {
        var files = await _drive.GetFilesFromPublicFolderAsync();

        ViewData["Title"] = "Google Drive файли";
        ViewData["MetaDescription"] = "Файли з публічної теки Google Drive, пов'язаної з CollabHub.";

        return View(files);
    }
}
