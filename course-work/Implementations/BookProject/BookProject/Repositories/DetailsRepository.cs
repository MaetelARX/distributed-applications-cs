using BookProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Repositories
{
	public class DetailsRepository : IDetailsRepository
	{
		private readonly ApplicationDbContext _context;
		public DetailsRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task AddDetails(Details details)
		{
			_context.Details.Add(details);
			await _context.SaveChangesAsync();
		}

		public async Task<Details?> GetDetailsByBookId(int bookId)
		{
			return await _context.Details.FirstOrDefaultAsync(x => x.BookId == bookId);
		}

		public async Task UpdateDetails(Details details)
		{
			_context.Details.Update(details);
			await _context.SaveChangesAsync();
		}
	}
}
