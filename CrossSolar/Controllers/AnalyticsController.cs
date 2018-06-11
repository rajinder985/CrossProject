using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Cli.Utils.CommandParsing;
using Microsoft.EntityFrameworkCore;
using Nancy.Json;

namespace CrossSolar.Controllers
{
    [Route("panel")]
    public class AnalyticsController : Controller
    {
		private readonly IAnalyticsRepository _analyticsRepository;

        private readonly IPanelRepository _panelRepository;

        public AnalyticsController(IAnalyticsRepository analyticsRepository, IPanelRepository panelRepository)
        {
            _analyticsRepository = analyticsRepository;
            _panelRepository = panelRepository;
        }

        // GET panel/XXXX1111YYYY2222/analytics
        [HttpGet("{panelId}/[controller]")]
        public async Task<IActionResult> Get([FromRoute] string panelId)

		{
            var panel = await _panelRepository.Query()
                .FirstOrDefaultAsync(x => x.Serial.Equals(panelId, StringComparison.CurrentCultureIgnoreCase));

            if (panel == null) return NotFound();

			int panelTemp = panel.Id;
			var analytics = await _analyticsRepository.Query()
                .Where(x => x.PanelId.Equals(panelTemp)).ToListAsync();

            var result = new OneHourElectricityListModel
            {
                OneHourElectricitys = analytics.Select(c => new OneHourElectricityModel
                {
                    Id = c.Id,
                    KiloWatt = c.KiloWatt,
                    DateTime = c.DateTime
                })
            };
			return Ok(result);
        }

		[HttpGet("GetHistoricalData")]
		public async Task<IActionResult> Get()
		{
			var analytics = from p in _analyticsRepository.GetAll() select p;
			var panel	  = from p in _panelRepository.GetAll() select p;


			var result = from a in analytics
						 join p in panel on a.PanelId 
						 equals p.Id where a.DateTime < DateTime.Today
						 group a by new { a.PanelId,a.DateTime } into analPanel 
							  select new {
											  PanelID = analPanel.Key.PanelId,
											  Date = analPanel.Key.DateTime,
											  Sum = analPanel.Sum(x => x.KiloWatt),
											  Avg = analPanel.Average(x => x.KiloWatt),
											  Min = analPanel.Min(x => x.KiloWatt),
											  Max = analPanel.Max(x => x.KiloWatt)
										};
				
			return Ok(result);
		}
					   
		// GET panel/XXXX1111YYYY2222/analytics/day
		[HttpGet("{panelId}/[controller]/day")]
        public async Task<IActionResult> DayResults([FromRoute] string panelId)
        {
            var result = new List<OneDayElectricityModel>();

            return Ok(result);
        }

        // POST panel/XXXX1111YYYY2222/analytics
        [HttpPost("{panelId}/[controller]")]
        public async Task<IActionResult> Post([FromRoute] int panelId, [FromBody] OneHourElectricityModel value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var oneHourElectricityContent = new OneHourElectricity
            {
                PanelId = panelId,
                KiloWatt = value.KiloWatt,
                DateTime = DateTime.UtcNow
            };

            await _analyticsRepository.InsertAsync(oneHourElectricityContent);

            var result = new OneHourElectricityModel
            {
                Id = oneHourElectricityContent.Id,
                KiloWatt = oneHourElectricityContent.KiloWatt,
                DateTime = oneHourElectricityContent.DateTime
            };

            return Created($"panel/{panelId}/analytics/{result.Id}", result);
        }
    }
}