using MediatR;
using UserManagement.ApplicationFeatures.Users.Queries;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Repository.Interfaces;

namespace UserManagement.ApplicationFeatures.Users.QueryHandlers
{
    public class SearchUserHandler : IRequestHandler<SearchUserQuery, List<User>>
    {
        private readonly IRepository<User> _userRepository;

        public SearchUserHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> Handle(SearchUserQuery request, CancellationToken cancellationToken)
        {
            return await _userRepository.SearchUsersAsync(
                request.KeyWord,
                request.Page,
                request.PageSize
            );
        }


    }
}
