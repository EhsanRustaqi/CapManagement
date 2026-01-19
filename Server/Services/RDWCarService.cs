using System.Net.Http;
using System.Text.Json;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.Models;

public class RdwCarService
{
    private readonly HttpClient _httpClient;

    public RdwCarService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }



    /// <summary>
    /// Retrieves vehicle information from the RDW (Dutch vehicle registry) using a license plate number.
    /// </summary>
    /// <param name="numberPlate">The license plate of the vehicle to look up.</param>
    /// <returns>
    /// A <see cref="CarDto"/> containing vehicle details retrieved from the RDW API.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the RDW API request fails or no vehicle is found for the provided license plate.
    /// </exception>
    /// <remarks>
    /// This method calls the RDW Open Data API and maps the returned data into a <see cref="CarDto"/>.
    /// The RDW JSON fields are lowercase, therefore case-insensitive deserialization is used.
    /// </remarks>
    public async Task<CarDto> GetCarInfoFromRdwAsync(string numberPlate)
    {
        string url = $"https://opendata.rdw.nl/resource/m9d7-ebf2.json?kenteken={numberPlate}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception("RDW API request failed.");

        var json = await response.Content.ReadAsStringAsync();

        var rdwCars = JsonSerializer.Deserialize<List<RdwCarDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Important for RDW JSON lowercase fields
        });

        var rdw = rdwCars?.FirstOrDefault();
        if (rdw == null)
            throw new Exception($"No car found for license plate: {numberPlate}");

        // ✅ Safe mapping
        var car = new CarDto
        {
            NumberPlate = numberPlate,
            Brand = rdw.merk ?? string.Empty,
            Model = rdw.handelsbenaming ?? string.Empty,
            Color = rdw.eerste_kleur ?? string.Empty,
            VehicleType = rdw.voertuigsoort ?? string.Empty,
            FuelType = rdw.brandstof_omschrijving ?? string.Empty,
            NumberOfDoors = rdw.aantal_deuren ?? string.Empty,
            NumberOfSeats = rdw.aantal_zitplaatsen ?? string.Empty,
            EngineCapacity = rdw.cilinderinhoud ?? string.Empty,
            EmptyWeight = rdw.massa_ledig_voertuig ?? string.Empty,
            ApkExpirationDate = rdw.vervaldatum_apk ?? string.Empty,
            Year = TryParseYear(rdw.datum_eerste_toelating)
        };

        return car;
    }

    //// 🔹 Helper methods (MUST be inside the class, below main methods)
    //private static int? TryParseNullableInt(string? value)
    //{
    //    if (int.TryParse(value, out var result))
    //        return result;
    //    return null;
    //}

    //private static DateTime? TryParseDate(string? value)
    //{
    //    if (DateTime.TryParse(value, out var date))
    //        return date;
    //    return null;
    //}

    private static int TryParseYear(string? dateValue)
    {
        if (!string.IsNullOrWhiteSpace(dateValue) && dateValue.Length >= 4 &&
            int.TryParse(dateValue.Substring(0, 4), out var year))
            return year;

        return 0;
    }
}
