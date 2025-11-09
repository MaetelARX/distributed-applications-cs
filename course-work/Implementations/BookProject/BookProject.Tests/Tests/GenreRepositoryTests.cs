using BookProject.Controllers;
using BookProject.Data;
using BookProject.Models;
using BookProject.Models.DTOS;
using BookProject.Repositories;
using BookProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Tests
{
    public class GenreRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly GenreRepository _repository;

        public GenreRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GenreRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            List<Genre> genres = new List<Genre>
            {
                new Genre 
                { 
                    Id = 1,
                    GenreName = "Fiction" 
                },
                new Genre 
                { 
                    Id = 2,
                    GenreName = "Non-Fiction"
                },
                new Genre 
                {
                    Id = 3,
                    GenreName = "Fantasy"
                }
            };

            _context.Genres.AddRange(genres);
            _context.SaveChanges();
        }
        [Fact]
        public async Task AddGenre_ShouldAddGenreToDatabase()
        {
            Genre genre = new Genre
            {
                Id = 4,
                GenreName = "Science"
            };
            await _repository.AddGenre(genre);

            var genres = await _context.Genres.ToListAsync();
            Assert.Contains(genres, x => x.GenreName == "Science");
        }
        [Fact]
        public async Task GetGenres_ShouldReturnAllGenres()
        {
            var genres = await _repository.GetGenres();
            Assert.Equal(3, genres.Count());
        }
        [Fact]
        public async Task GetGenreById_ShouldReturnCorrectGenre()
        {
            var genre = await _repository.GetGenreById(1);

            Assert.NotNull(genre);
            Assert.Equal("Fiction", genre.GenreName);
        }
        [Fact]
        public async Task GetGenreById_ShouldReturnNullIfNotFound()
        {
            var genre = await _repository.GetGenreById(99);
            Assert.Null(genre);
        }
        [Fact]
        public async Task UpdateGenre_ShouldModifyGenreDetails()
        {
            var genre = await _repository.GetGenreById(1);
            genre.GenreName = "Updated Fiction";

            await _repository.UpdateGenre(genre);

            var updatedGenre = await _context.Genres.FindAsync(1);
            Assert.Equal("Updated Fiction", updatedGenre.GenreName);
        }
        [Fact]
        public async Task DeleteGenre_ShouldRemoveGenreFromDatabase()
        {
            var genre = await _repository.GetGenreById(1);
            await _repository.DeleteGenre(genre);

            var genres = await _context.Genres.ToListAsync();
            Assert.DoesNotContain(genres, x => x.Id == 1);
        }
    }
}
