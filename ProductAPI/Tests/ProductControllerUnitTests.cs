using AutoMapper;
using Moq;
using ProductAPI.Controllers;
using ProductAPI.Models.Dto;
using ProductAPI.Models;
using ProductAPI.Repository;
using Xunit;
using FluentAssertions;
using AutoFixture.Xunit2;

namespace ProductAPI.Tests;

public class ProductControllerUnitTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly ProductAPIController _controller;
    private readonly IMapper _mapper;

    public ProductControllerUnitTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapper = MappingConfig.RegisterMaps().CreateMapper();
        _controller = new ProductAPIController(_productRepositoryMock.Object, _mapper);
    }

    [Theory, AutoData]
    public void GetShouldReturnListOfProducts(List<Product> products)
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.GetAllProducts()).Returns(products);
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products).ToList();

        // Act
        var response = _controller.Get();

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.Result.Should().BeEquivalentTo(productDtos);
        _productRepositoryMock.Verify(repo => repo.GetAllProducts(), Times.Once);
    }

    [Fact]
    public void GetShouldReturnEmptyListForEmptyDb()
    {
        // Arrange
        var products = new List<Product>();
        _productRepositoryMock.Setup(repo => repo.GetAllProducts()).Returns(products);
        var productDtos = new List<ProductDto>();

        // Act
        var response = _controller.Get();

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.Result.Should().BeEquivalentTo(productDtos);
        _productRepositoryMock.Verify(repo => repo.GetAllProducts(), Times.Once);
    }

    [Theory, AutoData]
    public void GetWithValidIdShouldReturnProduct(Product product)
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.GetProductById(1)).Returns(product);
        var productDto = _mapper.Map<ProductDto>(product);

        // Act
        var response = _controller.Get(1);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.Result.Should().BeEquivalentTo(productDto);
        _productRepositoryMock.Verify(repo => repo.GetProductById(1), Times.Once);
    }

    [Fact]
    public void GetShouldHandleException()
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.GetProductById(It.IsAny<int>())).Throws(new Exception("Database error"));

        // Act
        var response = _controller.Get(99);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Database error");
        _productRepositoryMock.Verify(repo => repo.GetProductById(99), Times.Once);
    }

    [Theory, AutoData]
    public void PostShouldAddProductSuccessfully(Product product)
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.AddProduct(It.IsAny<Product>())).Verifiable();
        var productDto = _mapper.Map<ProductDto>(product);

        // Act
        var response = _controller.Post(productDto);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.Result.Should().BeEquivalentTo(productDto);
        _productRepositoryMock.Verify(repo => repo.AddProduct(It.IsAny<Product>()), Times.Once);
    }

    [Theory, AutoData]
    public void PostShouldHandleException(ProductDto productDto)
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.AddProduct(It.IsAny<Product>())).Throws(new Exception("Database error"));

        // Act
        var response = _controller.Post(productDto);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Database error");
        _productRepositoryMock.Verify(repo => repo.AddProduct(It.IsAny<Product>()), Times.Once);
    }

    [Theory, AutoData]
    public void PutShouldUpdateProductSuccessfully(Product product)
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.UpdateProduct(It.IsAny<Product>())).Verifiable();
        var productDto = _mapper.Map<ProductDto>(product);

        // Act
        var response = _controller.Put(productDto);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.Result.Should().BeEquivalentTo(productDto);
        _productRepositoryMock.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Once);
    }

    [Theory, AutoData]
    public void PutShouldHandleException(ProductDto productDto)
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.UpdateProduct(It.IsAny<Product>())).Throws(new Exception("Database error"));

        // Act
        var response = _controller.Put(productDto);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Database error");
        _productRepositoryMock.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public void DeleteShouldDeleteProductSuccessfully()
    {
        // Arrange
        var productId = 1;
        _productRepositoryMock.Setup(repo => repo.DeleteProduct(It.IsAny<int>())).Verifiable();

        // Act
        var response = _controller.Delete(productId);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeEmpty();
        _productRepositoryMock.Verify(repo => repo.DeleteProduct(productId), Times.Once);
    }

    [Fact]
    public void DeleteShouldHandleException()
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.DeleteProduct(It.IsAny<int>())).Throws(new Exception("Database error"));

        // Act
        var response = _controller.Delete(1);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Database error");
        _productRepositoryMock.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Once);
    }

}
