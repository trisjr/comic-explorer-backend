namespace PublicApi.Responses;

public class BaseResponseWithData<T> : BaseResponse
{
    public T Data { get; set; } = default!;
}
