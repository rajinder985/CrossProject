using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CrossSolar.Controllers;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CrossSolar.Tests.Controller
{
	public class PanelControllerTests
    {

		public PanelControllerTests()
        {
            _panelController = new PanelController(_panelRepositoryMock.Object);
			_analyticsController = new AnalyticsController(_analyticsRepositoryMock.Object,_panelRepositoryMock.Object);

		}

        private readonly PanelController _panelController;
		private readonly AnalyticsController _analyticsController;
		private const string BASE_ADDRESS = "http://localhost:51063/";


		private readonly Mock<IPanelRepository> _panelRepositoryMock = new Mock<IPanelRepository>();
		private readonly Mock<IAnalyticsRepository> _analyticsRepositoryMock = new Mock<IAnalyticsRepository>();

		[Fact]
        public async Task Register_ShouldInsertPanel()
        {
            var panel = new PanelModel
            {
                Brand = "Areva",
                Latitude = 12.345678,
                Longitude = 98.7655432,
                Serial = "AAAA1111BBBB2222"
            };

            // Arrange

            // Act
            var result = await _panelController.Register(panel);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
		}

		[Fact]
		public async Task Register_ActionInsertSuccess()
		{
			var panel = new PanelModel
			{
				Id = 23,
				Brand = "Areva",
				Latitude = 12.345678,
				Serial ="FiveBC",
				Longitude = 98.7655432,
			};

			string output = JsonConvert.SerializeObject(panel);
			
			 using (var httpclient = new HttpClient())
				{
					var content = new StringContent(output.ToString(), Encoding.UTF8, "application/json");				
					httpclient.BaseAddress = new Uri(BASE_ADDRESS);
					HttpResponseMessage response = httpclient.PostAsync("Panel", content).Result;
					string message = response.Content.ReadAsStringAsync().Result;
					Assert.Equal(HttpStatusCode.Created, response.StatusCode);
					Assert.True(response.IsSuccessStatusCode);

				}
		}


		/// <summary>
		/// In this test case, we are not passing Serial property to API
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task Register_ActionInsertFailRequired()
		{
			var panel = new PanelModel
			{
				Id = 23,
				Brand = "Areva",
				Latitude = 12.345678,
				//Serial = "FiveBC",
				Longitude = 98.7655432,
			};

			string output = JsonConvert.SerializeObject(panel);

			using (var httpclient = new HttpClient())
			{
				var content = new StringContent(output.ToString(), Encoding.UTF8, "application/json");
				httpclient.BaseAddress = new Uri(BASE_ADDRESS);
				HttpResponseMessage response = httpclient.PostAsync("Panel", content).Result;
				string message = response.Content.ReadAsStringAsync().Result;
				Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
				Assert.False(response.IsSuccessStatusCode);

			}
		}


		[Fact]
		public async Task PostAnalytics_Success()
		{
			int panelID = 2;
			var analytics = new OneHourElectricityModel
			{
				Id = 15,
				KiloWatt = 124,
				DateTime = Convert.ToDateTime("2018-06-10 00:51:14.9630")
			};
						
			var result = await _analyticsController.Post(panelID, analytics);

	     	Assert.NotNull(result);
			var createdResult = result as CreatedResult;
			Assert.NotNull(createdResult);
			Assert.Equal(201, createdResult.StatusCode);
		}

		[Fact]
		public async Task PostAnalytics_ActualInsert()
		{
			int panelID = 2;
			var analytics = new OneHourElectricityModel
			{
				Id = 20,
				KiloWatt = 5464,
				DateTime = Convert.ToDateTime("2018-06-10 00:51:14.9630")
			};

			string output = JsonConvert.SerializeObject(analytics);

			using (var httpclient = new HttpClient())
			{
				var content = new StringContent(output.ToString(), Encoding.UTF8, "application/json");
				httpclient.BaseAddress = new Uri(BASE_ADDRESS);
				HttpResponseMessage response = httpclient.PostAsync("panel/" + panelID + "/analytics", content).Result;
				string message = response.Content.ReadAsStringAsync().Result;
				Assert.Equal(HttpStatusCode.Created, response.StatusCode);
				Assert.True(response.IsSuccessStatusCode);

			}
		}

		/// <summary>
		/// Failing Insert of the record in case if we pass panelID as string
		/// </summary>
		/// <returns></returns>
		[Fact]
		public async Task PostAnalytic_InsertFailure()
		{
			string panelID = "abc";
			var analytics = new OneHourElectricityModel
			{
				Id = 20,
				//KiloWatt = 5464,
				DateTime = Convert.ToDateTime("2018-06-10 00:51:14.9630")
			};

			string output = JsonConvert.SerializeObject(analytics);

			using (var httpclient = new HttpClient())
			{
				var content = new StringContent(output.ToString(), Encoding.UTF8, "application/json");
				httpclient.BaseAddress = new Uri(BASE_ADDRESS);
				HttpResponseMessage response = httpclient.PostAsync("panel/" + panelID + "/analytics", content).Result;
				string message = response.Content.ReadAsStringAsync().Result;
				Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
				Assert.False(response.IsSuccessStatusCode);

			}
		}


		[Fact]
		public void PostGetHistoricalData_Success()
		{
			using (var httpclient = new HttpClient())
			{
				httpclient.BaseAddress = new Uri(BASE_ADDRESS);
				HttpResponseMessage response = httpclient.GetAsync("panel/GetHistoricalData").Result;
				string message = response.Content.ReadAsStringAsync().Result;
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
				Assert.True(response.IsSuccessStatusCode);
			
			}
		}
	}
}