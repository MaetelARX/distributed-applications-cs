using BookProject.Controllers;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Controllers
{
    public class GenreControllerTests
    {
        private readonly Mock<IGenreRepository> _mockGenreRepo;
        private readonly GenreController _controller;

        public GenreControllerTests()
        {
            _mockGenreRepo = new Mock<IGenreRepository>();

            _controller = new GenreController(_mockGenreRepo.Object);

            var tempData = new Mock<ITempDataDictionary>();
            _controller.TempData = tempData.Object;
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithGenres()
        {
            var genres = new List<Genre>
            {
                new Genre { Id = 1, GenreName = "Fiction" },
                new Genre { Id = 2, GenreName = "Non-Fiction" }
            };

            _mockGenreRepo.Setup(repo => repo.GetGenres()).ReturnsAsync(genres);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(genres, viewResult.Model);
        }

        [Fact]
        public void AddGenre_Get_ShouldReturnView()
        {
            var result = _controller.AddGenre();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task AddGenre_Post_ShouldRedirectToAddGenreOnSuccess()
        {
            var genreDto = new GenreDTO { Id = 1, GenreName = "Fiction" };

            var result = await _controller.AddGenre(genreDto);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.AddGenre), redirectResult.ActionName);
        }

        [Fact]
        public async Task AddGenre_Post_ShouldReturnViewIfModelInvalid()
        {
            var genreDto = new GenreDTO { Id = 1, GenreName = "" };
            _controller.ModelState.AddModelError("GenreName", "Genre name is required");

            var result = await _controller.AddGenre(genreDto);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(genreDto, viewResult.Model);
        }

        [Fact]
        public async Task UpdateGenre_Get_ShouldReturnViewWithGenre()
        {
            var genre = new Genre { Id = 1, GenreName = "Fiction" };
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genre.Id)).ReturnsAsync(genre);

            var result = await _controller.UpdateGenre(genre.Id);
            var viewResult = Assert.IsType<ViewResult>(result);
            var genreDto = Assert.IsType<GenreDTO>(viewResult.Model);
            Assert.Equal(genre.Id, genreDto.Id);
            Assert.Equal(genre.GenreName, genreDto.GenreName);
        }

        [Fact]
        public async Task UpdateGenre_Get_ShouldThrowIfGenreNotFound()
        {
            int genreId = 99;
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync((Genre)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateGenre(genreId));
        }

        [Fact]
        public async Task UpdateGenre_Post_ShouldRedirectToIndexOnSuccess()
        {
            var genreDto = new GenreDTO { Id = 1, GenreName = "Fiction" };

            var result = await _controller.UpdateGenre(genreDto);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task UpdateGenre_Post_ShouldReturnViewIfModelInvalid()
        {
            var genreDto = new GenreDTO { Id = 1, GenreName = "" };
            _controller.ModelState.AddModelError("GenreName", "Genre name is required");

            var result = await _controller.UpdateGenre(genreDto);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(genreDto, viewResult.Model);
        }

        [Fact]
        public async Task DeleteGenre_ShouldRedirectToIndexOnSuccess()
        {
            var genre = new Genre { Id = 1, GenreName = "Fiction" };
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genre.Id)).ReturnsAsync(genre);
            var result = await _controller.DeleteGenre(genre.Id);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteGenre_ShouldThrowIfGenreNotFound()
        {
            int genreId = 99;
            _mockGenreRepo.Setup(repo => repo.GetGenreById(genreId)).ReturnsAsync((Genre)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteGenre(genreId));
        }
    }
}
