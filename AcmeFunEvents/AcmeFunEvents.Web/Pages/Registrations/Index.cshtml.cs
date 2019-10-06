using System;
using System.Linq;
using System.Threading.Tasks;
using AcmeFunEvents.Web.Data;
using AcmeFunEvents.Web.Interfaces;
using AcmeFunEvents.Web.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AcmeFunEvents.Web.Pages.Registrations
{
    public class IndexModel : PageModel
    {
        private readonly IOptions<AppOptions> _optionsAccessor;
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _db;
        private readonly ILogger _logger;
        private readonly IRegistrationService _registrationService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="registrationsContext"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="logger"></param>
        /// <param name="registrationService"></param>
        public IndexModel(
            IMemoryCache cache,
            ApplicationDbContext registrationsContext,
            IOptions<AppOptions> optionsAccessor,
            ILogger<IndexModel> logger,
            IRegistrationService registrationService
        )
        {
            _cache = cache;
            _db = registrationsContext;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
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
                var registrations = await _registrationService.GetRegistrationsAsync(out _);

                var m = registrations.SingleOrDefault(x => x.Id.Equals(id));

                if (m != null && _optionsAccessor.Value.EfSettings.DataContext.SaveEntity)
                {
                    try
                    {
                        _db.Registration.Remove(m);
                        _db.SaveChanges();
                        _logger.Log(LogLevel.Information, new EventId(2), "", null, (s, exception) => "Activity Deleted");
                        _cache.Remove(Url.Action("GetRegistrations", "Registration"));
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