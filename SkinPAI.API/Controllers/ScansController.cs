using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScansController : ControllerBase
{
    private readonly IScanService _scanService;
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ScansController> _logger;

    public ScansController(IScanService scanService, IUserService userService, IFileStorageService fileStorageService, ILogger<ScansController> logger)
    {
        _scanService = scanService;
        _userService = userService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get daily scan usage for current user
    /// </summary>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(DailyScanUsageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DailyScanUsageDto>> GetDailyUsage()
    {
        var userId = GetUserId();
        _logger.LogDebug("📊 SCAN USAGE: Fetching daily usage | UserId: {UserId}", userId);
        
        var usage = await _scanService.GetDailyScanUsageAsync(userId);
        _logger.LogInformation("📊 SCAN USAGE: Retrieved | UserId: {UserId} | ScansUsed: {ScansUsed}/{ScansLimit}", 
            userId, usage.ScanCount, usage.MaxScans);
        return Ok(usage);
    }

    /// <summary>
    /// Check if user can perform a scan
    /// </summary>
    [HttpGet("can-scan")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> CanPerformScan()
    {
        var userId = GetUserId();
        var canScan = await _scanService.CanPerformScanAsync(userId);
        _logger.LogDebug("🔍 CAN SCAN CHECK: UserId: {UserId} | CanScan: {CanScan}", userId, canScan);
        return Ok(new { canScan });
    }

    /// <summary>
    /// Perform a new skin scan
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SkinScanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SkinScanDto>> CreateScan([FromBody] CreateScanRequest request)
    {
        var userId = GetUserId();
        _logger.LogInformation("📸 SCAN CREATE: New scan requested | UserId: {UserId} | ImageDataSize: {ImageSize} bytes", 
            userId, request.ImageBase64?.Length ?? 0);
        
        try
        {
            // Check if questionnaire is completed
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null && !user.QuestionnaireCompleted)
            {
                _logger.LogWarning("⚠️ SCAN BLOCKED: Questionnaire not completed | UserId: {UserId}", userId);
                return StatusCode(403, new { 
                    message = "Please complete the skin questionnaire first",
                    requiresQuestionnaire = true
                });
            }
            
            // Check scan limits
            var canScan = await _scanService.CanPerformScanAsync(userId);
            if (!canScan)
            {
                _logger.LogWarning("⚠️ SCAN LIMIT REACHED: Daily limit exceeded | UserId: {UserId}", userId);
                return StatusCode(403, new { 
                    message = "You have reached your scan limit. Upgrade to get more scans!",
                    requiresUpgrade = true
                });
            }
            
            var scan = await _scanService.CreateScanAsync(userId, request, _fileStorageService);
            _logger.LogInformation("✅ SCAN CREATED: Scan completed | UserId: {UserId} | ScanId: {ScanId} | OverallScore: {Score}", 
                userId, scan.ScanId, scan.OverallScore);
            return Ok(scan);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "❌ SCAN FAILED: Error creating scan | UserId: {UserId} | Error: {Error}", userId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get user's scan history
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SkinScanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SkinScanDto>>> GetScanHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        _logger.LogDebug("📜 SCAN HISTORY: Fetching | UserId: {UserId} | Page: {Page} | PageSize: {PageSize}", userId, page, pageSize);
        
        var scans = await _scanService.GetUserScansAsync(userId, page, pageSize);
        _logger.LogInformation("📜 SCAN HISTORY: Retrieved {Count} scans | UserId: {UserId}", scans.Count, userId);
        return Ok(scans);
    }

    /// <summary>
    /// Get specific scan details
    /// </summary>
    [HttpGet("{scanId}")]
    [ProducesResponseType(typeof(SkinScanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkinScanDto>> GetScan(Guid scanId)
    {
        var userId = GetUserId();
        _logger.LogDebug("🔍 SCAN GET: Fetching scan details | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
        
        var scan = await _scanService.GetScanByIdAsync(scanId, userId);
        if (scan == null)
        {
            _logger.LogWarning("⚠️ SCAN NOT FOUND: Scan does not exist | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
            return NotFound();
        }
        return Ok(scan);
    }

    /// <summary>
    /// Get detailed analysis for a scan
    /// </summary>
    [HttpGet("{scanId}/analysis")]
    [ProducesResponseType(typeof(SkinAnalysisResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkinAnalysisResultDto>> GetAnalysis(Guid scanId)
    {
        var userId = GetUserId();
        _logger.LogDebug("📊 ANALYSIS GET: Fetching analysis | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
        
        var analysis = await _scanService.GetAnalysisByScanIdAsync(scanId, userId);
        if (analysis == null)
        {
            _logger.LogWarning("⚠️ ANALYSIS NOT FOUND: Analysis does not exist | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
            return NotFound();
        }
        _logger.LogInformation("📊 ANALYSIS GET: Retrieved analysis | UserId: {UserId} | ScanId: {ScanId} | SkinType: {SkinType}", 
            userId, scanId, analysis.SkinType);
        return Ok(analysis);
    }

    /// <summary>
    /// Get skin progress over time
    /// </summary>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(SkinProgressDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SkinProgressDto>> GetProgress([FromQuery] int days = 30)
    {
        var userId = GetUserId();
        _logger.LogDebug("📈 PROGRESS GET: Fetching progress | UserId: {UserId} | Days: {Days}", userId, days);
        
        var progress = await _scanService.GetSkinProgressAsync(userId, days);
        _logger.LogInformation("📈 PROGRESS GET: Retrieved progress | UserId: {UserId} | TotalScans: {TotalScans} | AverageScore: {AverageScore}", 
            userId, progress.TotalScans, progress.AverageScore);
        return Ok(progress);
    }

    /// <summary>
    /// Delete a scan
    /// </summary>
    [HttpDelete("{scanId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteScan(Guid scanId)
    {
        var userId = GetUserId();
        _logger.LogInformation("🗑️ SCAN DELETE: Delete requested | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
        
        var deleted = await _scanService.DeleteScanAsync(scanId, userId);
        if (!deleted)
        {
            _logger.LogWarning("⚠️ SCAN DELETE FAILED: Scan not found | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
            return NotFound();
        }
        
        _logger.LogInformation("✅ SCAN DELETED: Scan removed | UserId: {UserId} | ScanId: {ScanId}", userId, scanId);
        return Ok(new { message = "Scan deleted" });
    }

    /// <summary>
    /// Compare two scans
    /// </summary>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> CompareScans([FromQuery] Guid scan1Id, [FromQuery] Guid scan2Id)
    {
        var userId = GetUserId();
        _logger.LogDebug("🔄 SCAN COMPARE: Comparing scans | UserId: {UserId} | Scan1: {Scan1Id} | Scan2: {Scan2Id}", 
            userId, scan1Id, scan2Id);
        
        var scan1 = await _scanService.GetScanByIdAsync(scan1Id, userId);
        var scan2 = await _scanService.GetScanByIdAsync(scan2Id, userId);

        if (scan1 == null || scan2 == null)
        {
            _logger.LogWarning("⚠️ SCAN COMPARE FAILED: One or both scans not found | UserId: {UserId}", userId);
            return NotFound();
        }

        var comparison = new
        {
            scan1,
            scan2,
            comparison = new
            {
                overallScoreChange = (scan2.OverallScore ?? 0) - (scan1.OverallScore ?? 0),
                dateDifference = (scan2.ScanDate - scan1.ScanDate).Days
            }
        };
        
        _logger.LogInformation("✅ SCAN COMPARE: Comparison completed | UserId: {UserId} | ScoreChange: {ScoreChange}", 
            userId, comparison.comparison.overallScoreChange);
        return Ok(comparison);
    }
}
