using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;

        private Cloudinary _cloudinary;
        public AdminController(DataContext context, UserManager<User> userManager, 
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _context = context;
            _userManager = userManager;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );
            
            _cloudinary = new Cloudinary(acc);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userWithRoles = await (from user in _context.Users
                                       orderby user.UserName
                                       select new
                                       {
                                           Id = user.Id,
                                           UserName = user.UserName,
                                           roles = (from userRole in user.UserRoles
                                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                                    select role.Name
                                           ).ToList()
                                       }).ToListAsync();
            return Ok(userWithRoles);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var userRoles = await _userManager.GetRolesAsync(user);

            var selectRoles = roleEditDto.RoleNames;

            selectRoles = selectRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectRoles.Except(userRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to remove the roles");

            return Ok(await _userManager.GetRolesAsync(user));

        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _context.Photos
                .Include(u => u.User)
                .IgnoreQueryFilters()
                .Where(p => p.isApproved == false)
                .Select(u => new {
                    Id = u.Id,
                    UserName = u.User.UserName,
                    Url = u.Url,
                    IsApproved = u.isApproved
                }).ToListAsync();
            return Ok(photos);
        }

         [Authorize(Policy = "ModeratePhotoRole")]
         [HttpPost("approvePhoto/{photoId}")]
         public async Task<IActionResult> ApprovePhoto(int photoId)
         {
             var photo = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);

            photo.isApproved = true;

            await _context.SaveChangesAsync();

            return Ok();
         }

         [Authorize(Policy = "ModeratePhotoRole")]
         [HttpPost("rejectPhoto/{photoId}")]
         public async Task<IActionResult> RejectPhoto(int photoId)
         {
             var photo = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo.IsMain) 
                return BadRequest("You cannot reject the main photo");
            
            if (photo.PublicId != null) {
                var deleteParams = new DeletionParams(photo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok") 
                {
                    _context.Photos.Remove(photo);
                }
            }
            if (photo.PublicId == null) {
                _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();

            return Ok();
         }



    }
}