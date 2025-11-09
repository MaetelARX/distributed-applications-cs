namespace BookProject.Repositories.Interfaces
{
	public interface IDetailsRepository
	{
		Task AddDetails(Details details);
		Task UpdateDetails(Details details);
		Task<Details?> GetDetailsByBookId(int bookId);
	}
}
