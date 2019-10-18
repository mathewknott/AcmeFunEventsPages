using System.Collections.Generic;
using System.Threading.Tasks;
using AcmeFunEvents.Web.DTO;
using AcmeFunEvents.Web.Interfaces;
using AcmeFunEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AcmeFunEvents.Web.Controllers
{
    public class ActivityController : Controller
    {
        private readonly IActivityService _activityService;
        private readonly IRegistrationService _registrationService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityService"></param>
        /// <param name="registrationService"></param>
        public ActivityController(
            IActivityService activityService,
            IRegistrationService registrationService
            )
        {
            _activityService = activityService;
            _registrationService = registrationService;
        }
        
        #region JsonResults 

        /// <summary>
        /// Returns a list created.
        /// </summary>
        /// <param name="search">Text to search</param>
        /// <param name="sort">Parameter to sort </param>
        /// <param name="order">ASC or DESC</param>
        /// <param name="limit">Number of rows to return</param>
        /// <param name="offset">Starting position/offset in table</param>
        /// <returns></returns>
        /// 
        [HttpGet("/Activity/GetActivities", Name = "Activity_List")]
        public async Task<JsonPagedResult<IEnumerable<Activity>>> GetActivities(string search, string sort, string order, int limit = 200, int offset = 0)
        {
            var activities = await _activityService.GetActivitiesAsync(sort, out int total, order, limit, offset, search);

            return new JsonPagedResult<IEnumerable<Activity>>
            {
                Total = total,
                Rows = activities
            };
        }

        /// <summary>
        /// Returns a list created.
        /// </summary>
        /// <param name="search">Text to search</param>
        /// <param name="sort">Parameter to sort </param>
        /// <param name="order">ASC or DESC</param>
        /// <param name="limit">Number of rows to return</param>
        /// <param name="offset">Starting position/offset in table</param>
        /// <returns></returns>
        /// 
        [HttpGet("/Activity/GetRegistrations", Name = "Registrations_List")]
        public async Task<JsonPagedResult<IEnumerable<Registration>>> GetRegistrations(string search, string sort, string order, int limit = 200, int offset = 0)
        {
            var registrations = await _registrationService.GetRegistrationsAsync(sort, out int total, order, limit, offset, search);

            return new JsonPagedResult < IEnumerable < Registration >>
            {
                Total = total,
                Rows = registrations
            };
        }

        #endregion
    }
}