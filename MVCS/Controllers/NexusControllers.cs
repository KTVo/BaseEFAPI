using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace BaseEFAPI.MVCS.Controllers;


[ApiController]
[AllowAnonymous]
[Route("api/v1/nexus")]
public sealed class NexusControllers : ControllerBase
{
    [HttpPost]
    [Route("process/administrator/raw/file")]
    public async Task<IActionResult> ReadAdministratorDataFile(IFormFile file)
    {

        string? filePath = StoreSmallFiles(file);

        if (filePath == null) { return BadRequest(); }
        return Ok();
    }

    [HttpPost]
    [Route("process/administrator/raw/file/path")]
    public async Task<IActionResult> ReadAdministratorDataPath(string filePath)
    {
        

        return Ok();
    }


    private async Task<string?> StoreSmallFiles(IFormFile file)
    {
        long fileSize = file.Length;

        if (fileSize <= 0) { return null; }

        // CREATES A TEMP FILE FOR OUT DOWNLOADED FILE
        string filePath = Path.GetTempFileName();

        using (FileStream stream = System.IO.File.Create(filePath))
        {
            // SAVES THE FILE TO LOCAL FILE SYSTEM
            await file.CopyToAsync(stream);
        }

        return filePath;
    }

    private async Task<string?> StoreLargeFiles(CancellationToken cancellationToken)
    {
        // Get the IFormFeature and read the form
        IFormFeature formFeature = Request.HttpContext.Features.GetRequiredFeature<IFormFeature>();
        await formFeature.ReadFormAsync(cancellationToken);

        // Access the uploaded file (example: first file)
        IFormFile? file = Request.Form.Files.FirstOrDefault();

        if (file == null) { return null; }

        // EXTRACTS FILE NAME TO PREVENT OVERWRITING API FILES (ATTACK)
        string fileName = Path.GetFileName(file.Name);

        return fileName;
    }
    
     
}
