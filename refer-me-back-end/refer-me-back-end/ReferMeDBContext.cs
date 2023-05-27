using Microsoft.EntityFrameworkCore;
using refer_me_back_end.Models;
using System.Collections.Generic;

namespace refer_me_back_end
{
    public class ReferMeDBContext : DbContext
    {
        public ReferMeDBContext(DbContextOptions<ReferMeDBContext> options) : base(options)
        {

        }

        DbSet<User> Users { get; set; }
        DbSet<JobPost> JobPosts{ get; set; }
    }
}
