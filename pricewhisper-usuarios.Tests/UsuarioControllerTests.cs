using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pricewhisper.Controllers;
using pricewhisper.Models;
using pricewhisper.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PriceWhisper.Tests
{
    public class UsuarioControllerTests : IDisposable
    {
        private readonly UsuarioController _controller;
        private readonly OracleDbContext _context;

        public UsuarioControllerTests()
        {
            var options = new DbContextOptionsBuilder<OracleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Banco de dados único por teste
                .Options;
            _context = new OracleDbContext(options);
            _controller = new UsuarioController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Get_ReturnsAllUsuarios()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47960950000121",
                RazaoSocial = "Magazine Luiza S/A",
                NomeFantasia = "Magazine Luiza",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            _context.Usuarios.Add(new Usuario { Nome = "João de Souza", NomeUsuario = "joaosouza", Senha = "senha1234", EmpresaId = empresa.EmpresaId });
            _context.Usuarios.Add(new Usuario { Nome = "Maria Oliveira", NomeUsuario = "mariaoliveira", Senha = "senha5678", EmpresaId = empresa.EmpresaId });
            _context.SaveChanges();

            // Act
            var result = _controller.Get();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<UsuarioDto>>>(result);
            var returnValue = Assert.IsType<List<UsuarioDto>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenEmpresaExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47960950000121",
                RazaoSocial = "Magazine Luiza S/A",
                NomeFantasia = "Magazine Luiza",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            var usuario = new Usuario
            {
                Nome = "João de Souza",
                NomeUsuario = "joaosouza",
                Senha = "senha1234",
                EmpresaId = empresa.EmpresaId
            };

            // Act
            var result = await _controller.Create(usuario);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Usuario>(actionResult.Value);
            Assert.Equal("João de Souza", returnValue.Nome);
        }

        [Fact]
        public async Task GetById_ReturnsUsuario_WhenUsuarioExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47960950000121",
                RazaoSocial = "Magazine Luiza S/A",
                NomeFantasia = "Magazine Luiza",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            var usuario = new Usuario
            {
                Nome = "Maria Souza",
                NomeUsuario = "mariasouza",
                Senha = "senha5678",
                EmpresaId = empresa.EmpresaId
            };
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var result = await _controller.GetById(usuario.UsuarioId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UsuarioDto>>(result);
            var returnValue = Assert.IsType<UsuarioDto>(actionResult.Value);
            Assert.Equal("Maria Souza", returnValue.Nome);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenUsuarioExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47960950000121",
                RazaoSocial = "Magazine Luiza S/A",
                NomeFantasia = "Magazine Luiza",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            var usuario = new Usuario
            {
                Nome = "Carlos Pereira",
                NomeUsuario = "carlospereira",
                Senha = "senha91011",
                EmpresaId = empresa.EmpresaId
            };
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Buscar o usuário existente para evitar conflito de rastreamento
            var usuarioToUpdate = await _context.Usuarios.FindAsync(usuario.UsuarioId);
            usuarioToUpdate.Nome = "Carlos Almeida de Souza";
            usuarioToUpdate.NomeUsuario = "carlosalmeida";
            usuarioToUpdate.Senha = "novasenha123";

            // Act
            var result = await _controller.Update(usuario.UsuarioId, usuarioToUpdate);

            // Assert
            Assert.IsType<OkResult>(result);
            var usuarioInDb = await _context.Usuarios.FindAsync(usuario.UsuarioId);
            Assert.Equal("Carlos Almeida de Souza", usuarioInDb.Nome);
            Assert.Equal("carlosalmeida", usuarioInDb.NomeUsuario);
            Assert.Equal("novasenha123", usuarioInDb.Senha);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenUsuarioExists()
        {
            // Arrange
            var empresa = new Empresa
            {
                CNPJ = "47960950000121",
                RazaoSocial = "Magazine Luiza S/A",
                NomeFantasia = "Magazine Luiza",
                Usuarios = new List<Usuario>()
            };
            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            var usuario = new Usuario
            {
                Nome = "Ana Maria",
                NomeUsuario = "anamaria",
                Senha = "senha121314",
                EmpresaId = empresa.EmpresaId
            };
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var result = await _controller.Delete(usuario.UsuarioId);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(await _context.Usuarios.FindAsync(usuario.UsuarioId));
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUsuarioDoesNotExist()
        {
            // Arrange
            var updatedUsuario = new Usuario { UsuarioId = 99, Nome = "User99", NomeUsuario = "user99", Senha = "pwd99", EmpresaId = 1 };

            // Act
            var result = await _controller.Update(99, updatedUsuario);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUsuarioDoesNotExist()
        {
            // Act
            var result = await _controller.GetById(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenEmpresaDoesNotExist()
        {
            // Arrange
            var usuario = new Usuario { Nome = "User1", NomeUsuario = "user1", Senha = "pwd1", EmpresaId = 99 };

            // Act
            var result = await _controller.Create(usuario);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Empresa não encontrada", actionResult.Value);
        }
    }
}