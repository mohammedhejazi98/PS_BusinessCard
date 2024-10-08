using Microsoft.EntityFrameworkCore;

using PS_BusinessCard.Data;
using PS_BusinessCard.Models;

namespace PS_BusinessCard.Repositories
{
    public class BusinessCardRepository(DataContext context) : IBusinessCardRepository
    {
        #region Public Methods

        public async Task Add(BusinessCard businessCard)
        {
            await context.BusinessCards.AddAsync(businessCard);
            await context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var businessCard = await context.BusinessCards.FindAsync(id);
            if (businessCard != null)
            {
                context.BusinessCards.Remove(businessCard);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BusinessCard>> GetAll()
        {
            return await context.BusinessCards.ToListAsync();
        }

        public async Task<BusinessCard?> GetById(int id)
        {
            var businessCard = await context.BusinessCards.FirstOrDefaultAsync(b=>b.Id==id);
            return businessCard ;
        }
        public async Task Update(BusinessCard businessCard)
        {
            context.BusinessCards.Update(businessCard);
            await context.SaveChangesAsync();
        }

        #endregion Public Methods
    }
}
