using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using pricewhisper.Controllers;
using pricewhisper.Models;
using pricewhisper.Models.DTOs;
using pricewhisper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PriceWhisper.Tests
{
    public class EmpresaControllerTests : IDisposable
    {
        private readonly EmpresaController _controller;
        private readonly OracleDbContext _context;
        private readonly Mock<ICNPJaService> _mockService;

        public EmpresaControllerTests()
        {
            var options = new DbContextOptionsBuilder<OracleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Banco de dados único por teste
                .Options;
            _context = new OracleDbContext(options);
            _mockService = new Mock<ICNPJaService>();

            // Inicializa o controlador com o contexto e o serviço mockado
            _controller = new EmpresaController(_context, _mockService.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Get_ReturnsAllEmpresas()
        {
            // Arrange
            _context.Empresas.Add(new Empresa { EmpresaId = 1, CNPJ = "12345678901234", RazaoSocial = "Empresa A", NomeFantasia = "Fantasia A" });
            _context.Empresas.Add(new Empresa { EmpresaId = 2, CNPJ = "23456789012345", RazaoSocial = "Empresa B", NomeFantasia = "Fantasia B" });
            _context.SaveChanges();

            // Act
            var result = _controller.Get();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<EmpresaDto>>>(result);
            var returnValue = Assert.IsType<List<EmpresaDto>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenCNPJIsValid()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47960950000121",
                RazaoSocial = "Magazine Luiza S/A",
                NomeFantasia = "Magazine Luiza",
                Usuarios = new List<Usuario>()
            };

            var cnpjResponse = new CNPJaResponse
            {
                TaxId = "47960950000121",
                Active = true,
                Company = new CNPJaCompany
                {
                    Name = "Magazine Luiza S/A",
                    Status = new CNPJaStatus { Id = 1, Text = "Ativa" }
                }
            };

            _mockService.Setup(s => s.ConsultarCNPJ(It.Is<string>(c => c == empresa.CNPJ)))
                        .ReturnsAsync(cnpjResponse);

            // Act
            var result = await _controller.Create(empresa);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Empresa>(actionResult.Value);
            Assert.Equal("Magazine Luiza S/A", returnValue.RazaoSocial);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenCNPJIsInvalid()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47.960.950/0001-21", // CNPJ inválido (contém caracteres não numéricos)
                RazaoSocial = "Invalid CNPJ Empresa",
                NomeFantasia = "Invalid CNPJ",
                Usuarios = new List<Usuario>()
            };

            _mockService.Setup(s => s.ConsultarCNPJ(It.Is<string>(c => c == empresa.CNPJ)))
                        .ReturnsAsync((CNPJaResponse?)null); // Simula CNPJ inválido

            // Act
            var result = await _controller.Create(empresa);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("CNPJ inválido ou não encontrado na base da Receita Federal", actionResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsEmpresa_WhenEmpresaExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "34567890123456",
                RazaoSocial = "Empresa C",
                NomeFantasia = "Fantasia C",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            // Act
            var result = await _controller.GetById(empresa.EmpresaId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<EmpresaDto>>(result);
            var returnValue = Assert.IsType<EmpresaDto>(actionResult.Value);
            Assert.Equal("Empresa C", returnValue.RazaoSocial);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenEmpresaExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "45678901234567",
                RazaoSocial = "Empresa D",
                NomeFantasia = "Fantasia D",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            // Buscar a empresa existente para evitar conflito de rastreamento
            var empresaToUpdate = await _context.Empresas.FindAsync(empresa.EmpresaId);
            empresaToUpdate.NomeFantasia = "Magalu";

            // Act
            var result = await _controller.Update(empresa.EmpresaId, empresaToUpdate);

            // Assert
            Assert.IsType<OkResult>(result);
            var empresaInDb = await _context.Empresas.FindAsync(empresa.EmpresaId);
            Assert.Equal("Magalu", empresaInDb.NomeFantasia);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenEmpresaExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "56789012345678",
                RazaoSocial = "Empresa E",
                NomeFantasia = "Fantasia E",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            // Act
            var result = await _controller.Delete(empresa.EmpresaId);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(await _context.Empresas.FindAsync(empresa.EmpresaId));
        }
    }
}