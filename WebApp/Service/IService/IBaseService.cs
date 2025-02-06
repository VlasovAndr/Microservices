using WebApp.Models;

namespace WebApp.Service.IService
{
    public interface IBaseService
    {
        Task<ResponseDto> SendAsunc(RequestDto requestDto);
    }
}
