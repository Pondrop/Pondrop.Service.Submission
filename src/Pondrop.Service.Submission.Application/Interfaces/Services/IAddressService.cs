namespace Pondrop.Service.Submission.Application.Interfaces.Services;

public interface IAddressService
{
    bool IsValidAustralianPostcode(string postcode);
    bool IsValidAustralianState(string state);
    bool IsValidAustralianCountry(string country);
}