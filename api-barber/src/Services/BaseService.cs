using api_barber.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
namespace api_barber.Services
{
    public class BaseService<T> : IBaseService<T> where T : ModelBase
    {
        protected readonly IBaseRepository<T> _repository;
        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }
        public virtual async Task<List<T>> GetAllAsync(string barbershopId)
        {
            return await _repository.GetAllAsync(barbershopId);
        }
        public virtual async Task<T> GetByIdAsync(string id, string barbershopId)
        {
            return await _repository.GetByIdAsync(id, barbershopId);
        }
        public virtual async Task CreateAsync(T entity)
        {
            await _repository.CreateAsync(entity);
        }
        public virtual async Task UpdateAsync(string id, T entity)
        {
            await _repository.UpdateAsync(id, entity);
        }
        public virtual async Task SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            await _repository.SoftDeleteAsync(id, barbershopId, deletedBy);
        }
    }
}

