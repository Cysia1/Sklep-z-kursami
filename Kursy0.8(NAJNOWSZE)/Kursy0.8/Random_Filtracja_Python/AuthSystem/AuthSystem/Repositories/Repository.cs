using Microsoft.EntityFrameworkCore;
using System.Linq;
using AuthSystem.Models;
using static AuthSystem.Entities.Database;
using AuthSystem.Data;

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
            // Brak dodatkowych metod specyficznych dla kursów w tym interfejsie
        }

        public interface IRepositoryService<EntityType, IdType> where EntityType : IEntity<IdType>
        {
            IQueryable<EntityType> GetAll();
            EntityType GetSingle(IdType id);
            void Add(EntityType entity);
            void Delete(EntityType entity);
            void Edit(EntityType entity);
        }

        public class SqlCourseRepositoryService : ICourseRepositoryService
        {
            private readonly KursyDbContext _context;
            private readonly DbSet<Courses> _set;

            public SqlCourseRepositoryService(KursyDbContext context)
            {
                _context = context;
                _set = context.Set<Courses>();
            }

            public void Add(Courses entity)
            {
                _set.Add(entity);
                _context.SaveChanges();
            }

            public void Delete(Courses entity)
            {
                _set.Remove(entity);
                _context.SaveChanges();
            }

            public void Edit(Courses entity)
            {
                _context.Entry(entity).State = EntityState.Modified;
                _context.SaveChanges();
            }

            public IQueryable<Courses> GetAll()
            {
                return _set;
            }

            public Courses GetSingle(int id)
            {
                return _set.Find(id);
            }
        }
    }
}