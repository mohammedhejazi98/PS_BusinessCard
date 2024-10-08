using Microsoft.EntityFrameworkCore;

using PS_BusinessCard.Models;

namespace PS_BusinessCard.Data
{

    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        #region Public Properties

        public DbSet<BusinessCard> BusinessCards { get; set; }

        #endregion Public Properties
    }
}
