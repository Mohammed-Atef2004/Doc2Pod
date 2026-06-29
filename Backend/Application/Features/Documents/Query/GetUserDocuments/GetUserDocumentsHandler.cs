using Application.Common.Extensions;
using Application.Common.Wrappers;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Domain.Users.Errors;
using MediatR;


namespace Application.Features.Documents.Query.GetUserDocuments
{
    public class GetUserDocumentsHandler : IRequestHandler<GetUserDocumentsQuery, Result<PaginatedResult<GetUserDocumentsResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;

        public GetUserDocumentsHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserContext userContext)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<PaginatedResult<GetUserDocumentsResponse>>> Handle(GetUserDocumentsQuery request, CancellationToken cancellationToken)
        {
            var userid = _userContext.UserId;
            if (userid == null)
            {
                return Result<PaginatedResult<GetUserDocumentsResponse>>.Failure(UserErrors.InvalidUserId);

            }

            var query = _unitOfWork.Document.GetQueryableDocumentsByUserId(userid.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x =>
                    x.FileName.Contains(request.SearchTerm));
            }

            if (!string.IsNullOrEmpty(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "date":
                        query = request.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(v => v.CreatedAt)
                            : query.OrderBy(v => v.CreatedAt);
                        break;

                    default:
                        query = query.OrderBy(v => v.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(v => v.Id);
            }


            var result = await query
            .ProjectTo<GetUserDocumentsResponse>(_mapper.ConfigurationProvider)
              .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Result<PaginatedResult<GetUserDocumentsResponse>>.Success(result);
        }
    }
}