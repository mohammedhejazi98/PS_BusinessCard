using PS_BusinessCard.Models;

namespace PS_BusinessCard.Repositories
{
    public interface IBusinessCardRepository
    {
        #region Public Methods

        Task Add(BusinessCard businessCard);
        Task Delete(int id);
        Task<IEnumerable<BusinessCard>> GetAll();
        Task<BusinessCard?> GetById(int id);
        Task Update(BusinessCard businessCard);

        #endregion Public Methods
    }
}
