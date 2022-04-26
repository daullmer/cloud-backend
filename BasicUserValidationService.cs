using AspNetCore.Authentication.Basic;

namespace CloudComputingBackend;

public class BasicUserValidationService : IBasicUserValidationService
{
    private readonly ILogger<BasicUserValidationService> _logger;

    public BasicUserValidationService(ILogger<BasicUserValidationService> logger)
    {
        _logger = logger;
    }


    public Task<bool> IsValidAsync(string username, string password)
    {
        var vaild = username == "dhbw-demo" && password == "dhbw-demo";
        return Task.FromResult(vaild);
    }
}