using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Web.Controllers;
using UnitTest.Web.Helpers;
using UnitTest.Web.Models;
using UnitTest.Web.Repository;
using Xunit;

namespace UnitTest.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;

        private readonly ProductsApiController _controller;
        private readonly Helper _helper;
        private List<Product> products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
            _helper = new Helper();

            products = new List<Product>() {
                new Product() { Id=1, Name="Kalem",Price=100, Stock=50, Color="Kırımızı"},
            new Product() { Id=2, Name="Defter",Price=200, Stock=500, Color="Mavi"}
            };
        }

        [Theory]
        [InlineData(3, 7, 10)]
        public void Add_SampleValues_ReturnTotal(int a, int b, int total)
        {
            var expected = _helper.Add(a, b);

            Assert.Equal(expected, total);
        }


        [Fact]
        public async void GetProduct_ActionExecute_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            var result = await _controller.GetProduct();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal<int>(2, returnProduct.ToList().Count);
        }

        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdInValid_ReturnNotFoundResult(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetProduct_IdValid_ReturnOkResult(int productId)
        {
            Product product = products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(productId, returnProduct.Id);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_IdIsNotEqualProduct_ReturnBadRequestResult(int productId)
        {
            var product = products.First(x => x.Id == productId);

            var result = _controller.PutProduct(2, product);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_ActionExecute_ReturnNoContentResult(int productId)
        {
            var product = products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.Update(product));

            var result = _controller.PutProduct(1, product);

            _mockRepo.Verify(repo => repo.Update(product), Times.Once);

            var noContentResult = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecute_ReturnCreatedAtActionResult()
        {
            var product = products.First();

            _mockRepo.Setup(repo => repo.CreateAsync(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _mockRepo.Verify(x => x.CreateAsync(product), Times.Once);

            Assert.Equal("GetProduct", createdAtActionResult.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_IdInValid_ReturnNotFoundResult(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            var result = await _controller.DeleteProduct(productId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecute_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockRepo.Setup(repo => repo.Delete(product));

            var result = await _controller.DeleteProduct(productId);

            _mockRepo.Verify(x => x.Delete(product), Times.Once);
            _mockRepo.Verify(x => x.GetByIdAsync(productId), Times.Once);

            var noContentResult = Assert.IsType<NoContentResult>(result.Result);
        }



    }
}
