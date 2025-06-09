namespace Manage.Data.Reminder.Repository
{
    public interface ICommon<TMain>
    {
        Task<bool> Insert(TMain item);
        bool Update(TMain item);
        bool Delete(TMain item);
        Task<bool> Delete(int id);
        Task<bool> MockDelete(int id);
        Task<bool> UnMockDelete(int id);

        Task<TMain?> GetByID(int id);
        IQueryable<TMain> GetAll();
        IQueryable<TMain> GetAllNoTrack();
        Task Save();
    }
}
