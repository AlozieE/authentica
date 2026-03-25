using Authentica_app.Data;
        using Authentica_app.Models;
        using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Identity;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.EntityFrameworkCore;
        
        namespace Authentica_app.Controllers
        {
            [Authorize]
            public class VaultController : Controller
            {
                private readonly ApplicationDbContext _context;
                private readonly UserManager<IdentityUser> _userManager;
                
                public VaultController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
                {
                    _context = context;
                    _userManager = userManager;
                }
                
                public async Task<IActionResult> Index()
                {
                    var userId = _userManager.GetUserId(User);
                    var vaults = await _context.Vaults
                        .Where(v => v.UserId == userId)
                        .OrderByDescending(v => v.CreatedAt)
                        .ToListAsync();
                    return View(vaults);
                }
                
                public IActionResult Create()
                {
                    return View();
                }
                
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> Create(Vault vault)
                {
                    ModelState.Remove("UserId");
                    ModelState.Remove("User");
                
                    if (ModelState.IsValid)
                    {
                        vault.UserId = _userManager.GetUserId(User)!;
                        vault.CreatedAt = DateTime.UtcNow;
                        _context.Vaults.Add(vault);
                        await _context.SaveChangesAsync();
                        
                        return RedirectToAction(nameof(Index));
                    }
                    return View(vault);
                }
                
                public async Task<IActionResult> Edit(int id)
                {
                    var userId = _userManager.GetUserId(User);
                    
                    var vault = await _context.Vaults
                        .FirstOrDefaultAsync(v => v.VaultId == id && v.UserId == userId);
                        
                    if (vault == null)
                        return NotFound();
                    return View(vault);
                }
                
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> Edit(int id, Vault vault)
                {
                    ModelState.Remove("UserId");
                    ModelState.Remove("User");
                    
                    var userId = _userManager.GetUserId(User);
                    var existingVault = await _context.Vaults
                        .FirstOrDefaultAsync(v => v.VaultId == id && v.UserId == userId);
                        
                    if (existingVault == null)
                        return NotFound();
                        
                    if (ModelState.IsValid)
                    {
                        existingVault.Name = vault.Name;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    return View(vault);
                }
                
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> Delete(int id)
                {
                    var userId = _userManager.GetUserId(User);
                    var vault = await _context.Vaults
                        .FirstOrDefaultAsync(v => v.VaultId == id && v.UserId == userId);
                    
                    if (vault == null)
                        return NotFound();
                        
                    _context.Vaults.Remove(vault);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
        }