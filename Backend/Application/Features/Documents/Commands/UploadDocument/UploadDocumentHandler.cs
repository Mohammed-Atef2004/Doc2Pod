using Application.Interfaces;
using Domain.Documents;
using Domain.Documents.Errors;
using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Domain.Users.Errors;
using MediatR;

namespace Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentHandler
        : IRequestHandler<UploadDocumentCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileHashService _fileHashService;

        public UploadDocumentHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IUserContext userContext, IFileHashService fileHashService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _userContext = userContext;
            _fileHashService = fileHashService;
        }

        public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {

            var userid = _userContext.UserId;
            if (userid == null)
            {
                return Result<Guid>.Failure(UserErrors.InvalidUserId);

            }

            var fileHash = await _fileHashService.GenerateHashAsync(request.File);

            var alreadyExists = await _unitOfWork.Document.IsHashExistsAsync(
              userid.Value, fileHash);

            if (alreadyExists)
            {
                return Result<Guid>.Failure(
              UploadDocumentError.DocumentAlreadyExists);
            }

            var filePath = await _fileStorageService.SaveFileAsync(request.File);

            var pageCount = await _fileHashService.GetPageCountAsync(request.File);

            var document = new Document(
                userid.Value,
                request.File.FileName,
                filePath,
                fileHash,
                pageCount
            );

            await _unitOfWork.Document.AddAsync(document);
            await _unitOfWork.CompleteAsync();
            return Result<Guid>.Success(document.Id);
        }
    }
}
