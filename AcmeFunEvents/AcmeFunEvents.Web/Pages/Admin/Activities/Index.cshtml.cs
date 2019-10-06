using System;
using System.Linq;
using System.Threading.Tasks;
using AcmeFunEvents.Web.Controllers;
using AcmeFunEvents.Web.Data;
using AcmeFunEvents.Web.Interfaces;
using AcmeFunEvents.Web.Models;
using AcmeFunEvents.Web.Models.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AcmeFunEvents.Web.Pages.Admin.Activities
{
    public class IndexModel : PageModel
    {
        private readonly IOptions<AppOptions> _optionsAccessor;
        private readonly IHostingEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;
        private readonly IRegistrationService _registrationService;

        public IndexModel(
            IHostingEnvironment env,
            IMemoryCache cache,
            IEmailSender emailSender,
            ApplicationDbContext activitiesContext,
            IOptions<AppOptions> optionsAccessor,
            ILogger<IndexModel> logger,
            IActivityService activityService,
            IRegistrationService registrationService
            )
        {
            _cache = cache;
            _env = env;
            _emailSender = emailSender;
            _db = activitiesContext;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
            _activityService = activityService;
            _registrationService = registrationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {                      
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            if (ModelState.IsValid)
            {
                var activities = await _activityService.GetActivitiesAsync(out _);

                var m = activities.SingleOrDefault(x => x.Id.Equals(id));

                if (m != null && _optionsAccessor.Value.EfSettings.DataContext.SaveEntity)
                {
                    try
                    {
                        _db.Activity.Remove(m);
                        _db.SaveChanges();
                        _logger.Log(LogLevel.Information, new EventId(2), "", null, (s, exception) => "Activity Deleted");
                        _cache.Remove(Url.Action("GetActivities", "Activity"));
                        return Page();
                    }
                    catch (Exception dbEx)
                    {
                        ModelState.AddModelError("", "Cannot be deleted. Activity in use.");
                        _logger.LogError(new EventId(2), dbEx, $"An error occured deleting the Activity: {id}");
                    }
                }
            }

            return Page();
        }
    }
}