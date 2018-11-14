using Microsoft.Data.Entity;

namespace ScheduledSMSUWP
{
    public class dbHelperConnection : DbContext
    {
        public DbSet<tbl_ScheduledTasks> tbl_ScheduledTasks { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=app-db.db");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<tbl_ScheduledTasks>()
                .Property(b => b.Id)
                .IsRequired();

        }



    }

    ////////////////////////////////////////////////////////////////////
    public class tbl_ScheduledTasks
    {
   
        public string  Id { get; set; }//Unique PK
       public string AppType { get; set; }
        public string DateInsert { get; set; }
        public string TargetDate { get; set; }
        public string TargetTime { get; set; }
        public string ToReceiver { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int IsSent { get; set; }
        public int IsScheduled { get; set; }
        public int HasAttachment { get; set; }
        public string AttachmentPath { get; set; }

        public string DisplayName { get; set; }

    }

}
