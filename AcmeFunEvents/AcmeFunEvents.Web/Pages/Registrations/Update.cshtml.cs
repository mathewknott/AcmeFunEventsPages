using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcmeFunEvents.Web.DTO;
using AcmeFunEvents.Web.Interfaces;
using AcmeFunEvents.Web.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AcmeFunEvents.Web.Pages.Registrations
{
    public class UpdateModel : PageModel
    {
        [BindProperty]
        public Registration Registration { get; set; }

        [BindProperty]
        public IList<SelectListItem> Activities { get; set; }

        public UpdateModel(
            IMemoryCache cache,
            IOptions<AppOptions> optionsAccessor,
            IActivityService activityService,
            IRegistrationService registrationService,
            IUserService userService
        )
        {
            _cache = cache;
            _optionsAccessor = optionsAccessor;
            _activityService = activityService;
            _registrationService = registrationService;
            _userService = userService;
        }
        private readonly IOptions<AppOptions> _optionsAccessor;
        private readonly IMemoryCache _cache;
        private readonly IActivityService _activityService;
        private readonly IRegistrationService _registrationService;
        private readonly IUserService _userService;

        public void OnGet(Guid registrationId)
        {
            Registration = _registrationService.GetRegistrationsAsync(out _).Result.SingleOrDefault(x => x.Id.Equals(registrationId));

            if (Registration != null)
            {
                Activities = _activityService.GetActivitiesAsync(out _).Result.OrderBy(x => x.Name)
                    .Select(activity => new SelectListItem {Value = activity.Id.ToString(), Text = activity.Name})
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Activities = _activityService.GetActivitiesAsync(out _).Result.OrderBy(x => x.Name)
                .Select(activity => new SelectListItem { Value = activity.Id.ToString(), Text = activity.Name })
                .ToList();

            var registration = _registrationService.GetRegistrationsAsync(out _).Result.SingleOrDefault(x => x.Id.Equals(Registration.Id));

            if (registration == null)
            {
                ModelState.AddModelError("", "Registration Not Found");
                return Page();
            }

            var activities = await _activityService.GetActivitiesAsync(out int _);

            var findActivity = activities.FirstOrDefault(x => x.Id.Equals(Registration.Activity.Id));

            if (findActivity == null)
            {
                ModelState.AddModelError("", "Activity Not Found");
                return Page();
            }

            registration.Activity = findActivity;
            registration.ActivityId = findActivity.Id;

            var currentUser = _userService.GetUsersAsync(out _).Result.SingleOrDefault(x =>
                x.EmailAddress.Equals(Registration.User.EmailAddress, StringComparison.InvariantCultureIgnoreCase));

            if (currentUser == null)
            {
                //create a new user
                registration.User = new User
                {
                    FirstName = Registration.User.FirstName,
                    LastName = Registration.User.LastName,
                    EmailAddress = Registration.User.EmailAddress,
                    PhoneNumber = Registration.User.PhoneNumber
                };
            }
            else
            {
                //update properties except for email
                currentUser.FirstName = Registration.User.FirstName;
                currentUser.LastName = Registration.User.LastName;
                currentUser.PhoneNumber = Registration.User.PhoneNumber;
                registration.User = currentUser;
            }

            if (_optionsAccessor.Value.EfSettings.DataContext.SaveEntity)
            {
                _registrationService.EditRegistration(registration, out var result);

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

            return Page();
        }
    }
}