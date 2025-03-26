using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.DigitalKeyDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface IDigitalKeyService
    {
        string GenerateGameKey();
        Task<ResultDTO<string>> VerifyDigitalKey(string key, string projectName);
        Task<ResultDTO<DigitalKeyInfoResponse>> GenerateTestKey(Guid marketplaceProjectId);
        Task<ResultDTO<DigitalKeyInfoResponse>> GetDigitalKeyById(Guid id);
        Task<ResultDTO<PaginatedResponse<DigitalKeyInfoResponse>>> GetAllDigitalKey(ListRequest request);
    }
}
