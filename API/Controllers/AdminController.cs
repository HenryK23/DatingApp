using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
    
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles(){

            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration(){

            var unmoderatedPhotos = await _userManager.Users
                .Where(User => User.Photos.FirstOrDefault() != null)
                .IgnoreQueryFilters()
                .Select(p => new {
                    Username = p.UserName,
                    unmoderatedPhotos = p.Photos.Where(x => x.IsAvailable == false).ToList(),
                })
                .ToListAsync();
            
            
            return Ok(unmoderatedPhotos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpDelete("disapprove-photo/{userId}")]
        public async Task<ActionResult> DisapprovePhoto(int userId, [FromQuery] int photoId){

            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);

            if(user == null) return NotFound("Could not find this user");

            var photoToDelete = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photoToDelete == null) return NotFound("Could not find photo to delete");

            if (photoToDelete.PublicId != null){
                var result = await _photoService.DeletePhotoAsync(photoToDelete.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photoToDelete);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete photo");
         }
        
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPut("approve-photo/{userId}")]
        public async Task<ActionResult> ApprovePhoto(int userId, [FromQuery] int photoId){

            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);

            if(user == null) return NotFound("Could not find this user");

            var photoToApprove = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photoToApprove == null) return NotFound("Could not find photo to approve");

            photoToApprove.IsAvailable = true;

            if(user.Photos.Where(x => x.IsMain == true).Count() == 0) photoToApprove.IsMain = true;
            
            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to approve photo");
        }


        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if(user == null) return NotFound("Could not find user");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }
    }
}