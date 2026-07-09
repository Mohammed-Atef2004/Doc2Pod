using AutoMapper;

namespace Application.Features.Documents.Mapping
{
    public partial class DocumentProfile : Profile
    {
        public DocumentProfile()
        {
            GetUserDocumentsMapping();
        }

    }
}
