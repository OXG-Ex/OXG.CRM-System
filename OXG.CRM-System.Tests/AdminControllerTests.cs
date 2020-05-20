using Flurl.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using OXG.CRM_System.Controllers;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using OXG.CRM_System.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OXG.CRM_System.Tests
{
    public class AdminControllerTests
    {
        public AdminControllerTests()
        {
            optionsBuilder = new DbContextOptionsBuilder<CRMDbContext>();
            optionsBuilder.UseInMemoryDatabase("Tests DB");
            contextMemory = new CRMDbContext(optionsBuilder.Options);

            _appEnvironment = new Mock<IWebHostEnvironment>();
            _appEnvironment.Setup(host => host.ContentRootPath).Returns(@"C:\");
            _appEnvironment.Setup(host => host.WebRootPath).Returns(@"C:\");
        }

        private readonly DbContextOptionsBuilder<CRMDbContext> optionsBuilder;
        private readonly CRMDbContext contextMemory;
        private readonly Mock<IWebHostEnvironment> _appEnvironment;

        [Fact]
        public async Task IndexReturnsViewWithNotNullAdminIndexVM()
        {
            //Arrange
            var controller = new AdminController(contextMemory, _appEnvironment.Object);

            // Act
            IActionResult result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AdminIndexVM>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(30, model.Last30Days.Count());
        }

        [Fact]
        public async Task EmployeersReturnsViewWithNotNullModel()
        {
            //Arrange
            var controller = new AdminController(contextMemory, _appEnvironment.Object);

            // Act
            IActionResult result = await controller.Employeers();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AdminEmployeersVM>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task WorksReturnsViewWithWorksModel()
        {
            //Arrange
            await contextMemory.Works.AddAsync(new Work() { Id = 1, Name = "TestWork", Num = 1, Price = 1227 });
            await contextMemory.SaveChangesAsync();
            var controller = new AdminController(contextMemory, _appEnvironment.Object);

            // Act
            IActionResult result = controller.Works();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IQueryable<string>>(viewResult.Model);
            Assert.NotNull(model);
            Assert.True(model.Count() == 1);
            Assert.Equal("TestWork", model.First());
        }
    }
}
