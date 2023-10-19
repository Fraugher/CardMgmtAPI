using Microsoft.EntityFrameworkCore;
using CardManagementAPI.Models;

namespace CardManagementAPI.Data
{
    public class ApiDataContext : DbContext
    {
        public DbSet<Card> Cards { get; set; }  
        public DbSet<UFEFeeLogger> UFEFees { get; set; }  
      
        public ApiDataContext(DbContextOptions<ApiDataContext> options) 
            :base (options)
        {
        }
    }
}
