using MediatR;
using UserManagement.Domain.Entities;

namespace UserManagement.ApplicationFeatures.Users.Queries
{
    public class SearchUserQuery : IRequest<List<User>>
    {
        public string KeyWord { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }

        public SearchUserQuery(string keyWord, int pageSize, int page)
        {
            KeyWord = keyWord;
            PageSize = pageSize;
            Page = page;
        }
    }
}
