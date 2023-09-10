using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();
        public async Task<User> GetByIdAsync(Guid id) => await _userRepository.GetByIdAsync(id);
        public async Task<User> CreateAsync(User newUser) => await _userRepository.AddAsync(newUser);
        public async Task DeleteAsync(Guid id) => await _userRepository.DeleteAsync(id);
        public async Task UpdateAsync(User user) => await _userRepository.UpdateAsync(user);
        public async Task<bool> ValidateAsync(UserLoginDTO userLoginDto) => await _userRepository.ValidateAsync(userLoginDto);
    }
}