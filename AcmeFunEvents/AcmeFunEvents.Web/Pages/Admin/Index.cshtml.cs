using System;
using System.Threading.Tasks;
using AcmeFunEvents.Web.Data;
using AcmeFunEvents.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace AcmeFunEvents.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IMemoryCache _cache;

        private readonly IActivityService _activityService;

        private readonly IRegistrationService _registrationService;

        private readonly IUserService _userService;

        private readonly ApplicationDbContext _db;

        [BindProperty]
        public string Message { get; set; }

        /// <param name="memoryCache"></param>
        /// <param name="activityService"></param>
        /// <param name="registrationService"></param>
        /// <param name="userService"></param>
        /// <param name="dataContext"></param>
        public IndexModel(IMemoryCache memoryCache, IActivityService activityService, IRegistrationService registrationService, IUserService userService, ApplicationDbContext dataContext)
        {
            _cache = memoryCache;
            _activityService = activityService;
            _registrationService = registrationService;
            _userService = userService;
            _db = dataContext;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                foreach (var o in _registrationService.GetRegistrationsAsync(out int _).Result)
                {
                    try
                    {
                        _db.Registration.Remove(o);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                foreach (var m in _activityService.GetActivitiesAsync(out int _).Result)
                {
                    if (m != null)
                    {
                        try
                        {
                            _db.Activity.Remove(m);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                foreach (var m in _userService.GetUsersAsync(out int _).Result)
                {
                    if (m != null)
                    {
                        try
                        {
                            _db.User.Remove(m);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                Message = "Success!";
                _db.SaveChanges();

                _cache.Remove(Url.Action("GetActivities", "Activity"));
                _cache.Remove(Url.Action("GetRegistrations", "Activity"));
                _cache.Remove(Url.Action("GetUsers", "Registration"));
              
            }
            return Page();
        }
    }
}