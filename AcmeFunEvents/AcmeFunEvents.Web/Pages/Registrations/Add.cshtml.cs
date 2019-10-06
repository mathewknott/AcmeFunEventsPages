using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcmeFunEvents.Web.Data;
using AcmeFunEvents.Web.DTO;
using AcmeFunEvents.Web.Interfaces;
using AcmeFunEvents.Web.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AcmeFunEvents.Web.Pages.Registrations
{
    public class AddModel : PageModel
    {
        [BindProperty]
        public Registration Registration { get; set; }

        [BindProperty]
        public IList<SelectListItem> Activities { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="registrationsContext"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="registrationService"></param>
        /// <param name="logger"></param>
        /// <param name="activityService"></param>
        /// <param name="userService"></param>
        public AddModel(
            IMemoryCache cache,
            ApplicationDbContext registrationsContext,
            IOptions<AppOptions> optionsAccessor,
            ILogger<AddModel> logger,
            IActivityService activityService,
            IRegistrationService registrationService,
            IUserService userService
        )
        {
            _cache = cache;
            _db = registrationsContext;
            _optionsAccessor = optionsAccessor;
            _activityService = activityService;
            _registrationService = registrationService;
            _userService = userService;
        }

        private readonly IOptions<AppOptions> _optionsAccessor;
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _db;
        private readonly IActivityService _activityService;
        private readonly IRegistrationService _registrationService;
        private readonly IUserService _userService;

        public void OnGet(Guid registrationId)
        {
            Activities = _activityService.GetActivitiesAsync(out _).Result.OrderBy(x => x.Name).Select(activity =>
                new SelectListItem { Value = activity.Id.ToString(), Text = activity.Name }).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Activities = _activityService.GetActivitiesAsync(out _).Result.OrderBy(x => x.Name).Select(activity =>
                new SelectListItem { Value = activity.Id.ToString(), Text = activity.Name }).ToList();

            var currentUser = _userService.GetUsersAsync(out _).Result.SingleOrDefault(x =>
                     x.EmailAddress.Equals(Registration.User.EmailAddress, StringComparison.InvariantCultureIgnoreCase));

            var registrations = _registrationService.GetRegistrationsAsync(out _).Result.OrderBy(x => x.RegistrationNumber).ToList();

            var registeredUser = registrations.SingleOrDefault(x =>
                x.User.Equals(currentUser) && x.Activity.Id.Equals(Registration.ActivityId));

            if (registeredUser != null)
            {
                ModelState.AddModelError("", "User already registered for this activity");
                return Page();
            }

            var activities = await _activityService.GetActivitiesAsync(out _);

            var findActivity = activities.FirstOrDefault(x => x.Id.Equals(Registration.ActivityId));

            if (findActivity == null)
            {
                ModelState.AddModelError("", "Activity Not Found");
                return Page();
            }

            Registration.Comments = Registration.Comments;
            //Registration.ActivityId = findActivity.Id;
            Registration.Activity = findActivity;

            if (currentUser == null)
            {
                Registration.User = new User
                {
                    FirstName = Registration.User.FirstName,
                    LastName = Registration.User.LastName,
                    EmailAddress = Registration.User.EmailAddress,
                    PhoneNumber = Registration.User.PhoneNumber,
                };

                _db.User.Add(Registration.User);
                _db.SaveChanges();

                //Registration.UserId = Registration.User.Id;
            }
            else
            {
                //update properties except for email
                currentUser.FirstName = Registration.User.FirstName;
                currentUser.LastName = Registration.User.LastName;
                currentUser.PhoneNumber = Registration.User.PhoneNumber;
                Registration.User = currentUser;
                //Registration.UserId = currentUser.Id;
            }

            if (registrations.Any())
            {
                Registration.RegistrationNumber = registrations.Last().RegistrationNumber + 1;
            }
            else
            {
                Registration.RegistrationNumber = 1;
            }
            
            if (_optionsAccessor.Value.EfSettings.DataContext.SaveEntity)
            {
                _registrationService.AddRegistration(Registration, out var result);

                if (result > 0)
                {
                    if (_optionsAccessor.Value.EfSettings.DataContext.SendEmail)
                    {
                        //await SendCreatedConfirmEmail(Registration);
                    }

                    _cache.Remove(Url.Action("GetRegistrations", "Registration"));

                    return Redirect("/Registrations/Index");
                }

                return Page();
            }

            return Redirect("/Registrations/Index");
        }
    }
}