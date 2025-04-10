using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AuthSystem.Data;
using static AuthSystem.Entities.Database;

namespace AuthSystem.Repositories
{
    public class CourseRepository 
    {
        public interface IEntity<T>
        {
            T Id { get; set; }
        }

        public interface ICourseRepositoryService : IRepositoryService<Courses, int>
        {
           
        }

        public interface IRepositoryService<EntityType, IdType> where EntityType : IEntity<IdType>
        {
            IQueryable<EntityType> GetAll();
            EntityType GetSingle(IdType id);
            void Add(EntityType entity);
            void Delete(EntityType entity);
            void Edit(EntityType entity);
        }

        public class InMemoryCourseRepositoryService : ICourseRepositoryService 
        {
            protected InMemoryDbContext _context;
            protected DbSet<Courses> _set;

            public InMemoryCourseRepositoryService(InMemoryDbContext context)
            {
                _context = context;
                _set = context.Set<Courses>();
            }

            public virtual void Add(Courses entity)
            {
                _set.Add(entity);
                _context.SaveChanges();
            }

            public virtual void Delete(Courses entity)
            {
                _set.Remove(entity);
                _context.SaveChanges();
            }

            public virtual void Edit(Courses entity)
            {
                (_context as DbContext).Entry(entity).State = EntityState.Modified;
                _context.SaveChanges();
            }

            public virtual IQueryable<Courses> GetAll()
            {
                return _set;
            }

            public virtual Courses GetSingle(int id)
            {
                return _set.Find(id);
            }
        }
    }
}