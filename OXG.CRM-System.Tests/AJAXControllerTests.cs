using Flurl.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using OXG.CRM_System.Controllers;
using OXG.CRM_System.Models;
using OXG.CRM_System.Models.Employeers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OXG.CRM_System.Tests
{
    public class AJAXControllerTests
    {
        public AJAXControllerTests()
        {
            optionsBuilder = new DbContextOptionsBuilder<CRMDbContext>();
            optionsBuilder.UseInMemoryDatabase("Tests DB");
            contextMemory = new CRMDbContext(optionsBuilder.Options);
        }

        private readonly DbContextOptionsBuilder<CRMDbContext> optionsBuilder;
        private readonly CRMDbContext contextMemory;

        [Fact]
        public void IndexReturnsOk()
        {
            //Arrange
            var controller = new AJAXController(contextMemory);

            // Act
            IActionResult result = controller.Index();

            // Assert
            Assert.IsType<ContentResult>(result);
            Assert.NotNull(result);
            Assert.Equal("Ok", (result as ContentResult).Content);
        }

        [Fact]
        public async Task GetClientReturnsNotNullJsonResult()
        {
            //Arrange
            await contextMemory.Clients.AddAsync(new Client() { Id = 5, Name = "TestClient" });
            await contextMemory.SaveChangesAsync();
            var controller = new AJAXController(contextMemory);

            // Act
            JsonResult result = await controller.GetClient(5) as JsonResult;

            // Assert
            Assert.IsType<JsonResult>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserPhotoReturnsStringPath()
        {
            //Arrange
            await contextMemory.Managers.AddAsync(new Manager() { Id = "abcde-14257", Name = "TestManager", Email = "TestManager@manager.com", Photo = "c://Photo.png" });
            await contextMemory.SaveChangesAsync();
            var controller = new AJAXController(contextMemory);

            // Act
            IActionResult result = await controller.GetUserPhoto("TestManager@manager.com");
            var str = (result as ContentResult).Content;
            // Assert
            Assert.IsType<ContentResult>(result);
            Assert.Equal("c://Photo.png", str);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetNoticesNumReturnsNotNullString()
        {
            //Arrange
            await contextMemory.Employeers.AddAsync(new Manager() { Id = "abcde", Name = "TestManager2", Email = "TestManager2@manager.com" });
            await contextMemory.Notices.AddAsync(new Notice() { Id = 1, EmployeerId = "abcde-142578", EmployeerName = "TestManager", Text = "TestNotice", IsViewed = false });
            await contextMemory.SaveChangesAsync();
            var controller = new AJAXController(contextMemory);

            // Act
            ContentResult result = await controller.GetNoticesNum("TestManager2@manager.com");
            var str = result.Content;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ContentResult>(result);
            Assert.Equal("0", str);
        }
    }
}
