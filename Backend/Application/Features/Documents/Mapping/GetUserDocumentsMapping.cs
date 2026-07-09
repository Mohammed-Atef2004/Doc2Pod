using Application.Features.Documents.Query.GetUserDocuments;
using Domain.Documents;

namespace Application.Features.Documents.Mapping
{
    public partial class DocumentProfile
    {
        public void GetUserDocumentsMapping()
        {
            CreateMap<Document, GetUserDocumentsResponse>()
                .ForCtorParam("DocumentId", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("DocumentName", opt => opt.MapFrom(src => src.FileName))
                .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt));

        }

    }
}
