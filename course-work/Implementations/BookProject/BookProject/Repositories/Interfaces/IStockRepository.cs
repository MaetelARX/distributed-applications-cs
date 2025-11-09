namespace BookProject.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock?> GetStockByBookId(int bookId);
        Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "");
        Task ManageStock(StockDTO stockToManage);
        Task<bool> BookExists(int bookId);
    }
}