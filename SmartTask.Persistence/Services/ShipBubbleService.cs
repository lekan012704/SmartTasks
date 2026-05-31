using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SmartTask.Application.Dto.Order;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using SmartTask.Persistence.Contexts;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class ShipBubbleService : IShipBubbleService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private string? _senderAddressCode;
    private string? _receiverAddressCode;
    private readonly ShipBubbleSettings _settings;

    public ShipBubbleService(HttpClient httpClient, IOptions<ShipBubbleSettings> options, ApplicationDbContext context, IOptions<ShipBubbleSettings> settings)
    {
        _httpClient = httpClient;
        var apiKey = settings.Value.ApiKey;

        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _context = context;
        _settings = settings.Value;
    }

    private async Task<string> ValidateAddressAsync(AddressDto address)
    {
        var json = JsonConvert.SerializeObject(address);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("shipping/address/validate", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Address validation failed: {responseBody}");

        dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody)
            ?? throw new Exception("Invalid response from ShipBubble");

        return jsonResponse.data.address_code.ToString();
    }

    //private async Task InitializeAddressCodesAsync(AddressDto sender, AddressDto receiver)
    //{
   
    //    var savedSender = await _context.AddressBook.FirstOrDefaultAsync(x => x.Email == receiver.email);
    //    var savedReceiver = await _context.AddressBook.FirstOrDefaultAsync(x => x.Email == receiver.email);

    //    if (savedSender != null)
    //    {
    //        _senderAddressCode = savedSender.AddressCode;
    //    }
    //    else
    //    {
    //        _senderAddressCode = await ValidateAddressAsync(sender);
    //        await _context.AddAsync(new AddressBook
    //        {
    //            Name = sender.name,
    //            Email = sender.email,
    //            Phone = sender.phone,
    //            Address = sender.address,
    //            AddressCode = _senderAddressCode
    //        });
    //    }

    //    if (savedReceiver != null)
    //    {
    //        _receiverAddressCode = savedReceiver.AddressCode;
    //    }
    //    else
    //    {
    //        _receiverAddressCode = await ValidateAddressAsync(receiver);
    //        await _context.AddAsync(new AddressBook
    //        {
    //            Name = receiver.name,
    //            Email = receiver.email,
    //            Phone = receiver.phone,
    //            Address = receiver.address,
    //            AddressCode = _receiverAddressCode
    //        });
           
    //    }
      
    //}


      //  public async Task<dynamic> FetchRatesAsync(FetchRatesDto fetchRates)
      //  {
      //      try
      //      {

      //          await InitializeAddressCodesAsync(fetchRates.Sender, fetchRates.Receiver);

      //          int senderCode = int.Parse(_senderAddressCode);
      //          int receiverCode = int.Parse(_receiverAddressCode);

      //          var categories = await GetCategoriesAsync();

      //      var matchedCategory = categories.FirstOrDefault(c =>
      //fetchRates.Items.Any(i => i.Description.Contains(c.Name, StringComparison.OrdinalIgnoreCase)));

      //      int categoryId = matchedCategory?.Id ?? categories.FirstOrDefault()?.Id ?? 1;


      //      // 🔹 Total weight
      //      double totalWeight = fetchRates.Items.Sum(i => i.Weight * i.Quantity);

      //          // 🔹 Package dimensions
      //          var dimension = fetchRates.PackageDimension ?? new PackageDimension
      //          {
      //              length = fetchRates.Items.Max(i => i.PackageLength),
      //              width = fetchRates.Items.Max(i => i.PackageWidth),
      //              height = fetchRates.Items.Max(i => i.PackageHeight)
      //          };
            
      //      // 🔹 Build payload for API
      //      var payload = new
      //          {
      //              sender_address_code = senderCode,
      //              reciever_address_code = receiverCode,
      //              pickup_date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
      //              category_id = categoryId,
      //              package_items = fetchRates.Items.Select(item => new
      //              {
      //                  name = item.ProductName,
      //                  description = item.Description,
      //                  unit_weight = item.Weight,
      //                  unit_amount = item.Price,
      //                  quantity = item.Quantity
      //              }).ToList(),
      //              package_dimension = new
      //              {
      //                  length = dimension.length,
      //                  width = dimension.width,
      //                  height = dimension.height
      //              },
      //              delivery_instructions = "Please provide additional instructions for your package",
      //              //weight = totalWeight
      //          };

      //          var json = JsonConvert.SerializeObject(payload);
      //          var content = new StringContent(json, Encoding.UTF8, "application/json");

      //          var response = await _httpClient.PostAsync("shipping/fetch_rates", content);
      //          var responseBody = await response.Content.ReadAsStringAsync();

      //          if (!response.IsSuccessStatusCode)
      //              throw new Exception($"Fetch rates failed: {responseBody}");
                    
      //          return JsonConvert.DeserializeObject<dynamic>(responseBody);
      //      }
      //      catch (Exception ex)
      //      {
      //          throw new Exception($"Error fetching rates: {ex.Message}");
      //      }
      //  }

        private async Task<List<CategoryDto>> GetCategoriesAsync()
        {
        var categoriesResponse = await _httpClient.GetAsync("shipping/labels/categories");
        var categoriesJson = await categoriesResponse.Content.ReadAsStringAsync();

        var categoriesWrapper = JsonConvert.DeserializeObject<CategoryApiResponse>(categoriesJson)
                                ?? new CategoryApiResponse();
        var categories = categoriesWrapper.Data;
        return categories;

    }





    private async Task<string> CreateShipmentAsync(string requestToken, string serviceCode, string courierId)
    {
        var shipmentData = new
        {
            request_token = requestToken,
            service_code = serviceCode,
            courier_id = courierId
        };

        var json = JsonConvert.SerializeObject(shipmentData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("shipments", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Shipment creation failed: {responseBody}");

        return responseBody;
    }


    //public async Task<ShipmentResultDto> CreateShipmentAutomaticallyAsync(FetchRatesDto fetchRates)
    //{
    //    var allRates = await FetchRatesAsync(fetchRates);

    //    string fastestServiceCode = (string)allRates.data.fastest_courier.service_code;

    //    var fastestCourierRates = await FetchRatesForServiceCodeAsync(fastestServiceCode, fetchRates);

    //    dynamic selectedCourier = fastestCourierRates.data.fastest_courier;

    //    // ✅ Force string casting for dynamic JSON values
    //    var shipmentResponseJson = await CreateShipmentAsync(
    //        (string)fastestCourierRates.data.request_token,
    //        (string)selectedCourier.service_code,
    //        (string)selectedCourier.courier_id
    //    );

    //    dynamic shipmentResult = JsonConvert.DeserializeObject<dynamic>(shipmentResponseJson);

    //    return new ShipmentResultDto
    //    {
    //        TrackingNumber = (string)shipmentResult.data.tracking_number,
    //        CourierName = (string)selectedCourier.courier_name,
    //        CourierId = (string)selectedCourier.courier_id,
    //        ServiceCode = (string)selectedCourier.service_code,
    //        LabelUrl = (string)shipmentResult.data.label_url,
    //        DeliveryETA = (string)selectedCourier.delivery_eta,
    //        TotalAmount = (decimal)selectedCourier.total
    //    };
    //}


    // 🔹 Private method to fetch rates for a specific service code
    //private async Task<dynamic> FetchRatesForServiceCodeAsync(string serviceCode, FetchRatesDto fetchRates)
    //{
    //    // Ensure sender & receiver address codes
    //    await InitializeAddressCodesAsync(fetchRates.Sender, fetchRates.Receiver);

    //    int senderCode = int.Parse(_senderAddressCode);
    //    int receiverCode = int.Parse(_receiverAddressCode);

    //    // Compute category
    //    var categories = await GetCategoriesAsync();
    //    var matchedCategory = categories.FirstOrDefault(c =>
    //        fetchRates.Items.Any(i => i.Description.Contains(c.Name, StringComparison.OrdinalIgnoreCase)));
    //    int categoryId = matchedCategory?.Id ?? categories.FirstOrDefault()?.Id ?? 1;

    //    // Package dimension
    //    var dimension = fetchRates.PackageDimension ?? new PackageDimension
    //    {
    //        length = fetchRates.Items.Max(i => i.PackageLength),
    //        width = fetchRates.Items.Max(i => i.PackageWidth),
    //        height = fetchRates.Items.Max(i => i.PackageHeight)
    //    };

    //    var payload = new
    //    {
    //        sender_address_code = senderCode,
    //        reciever_address_code = receiverCode,
    //        pickup_date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    //        category_id = categoryId,
    //        package_items = fetchRates.Items.Select(item => new
    //        {
    //            name = item.ProductName,
    //            description = item.Description,
    //            unit_weight = item.Weight,
    //            unit_amount = item.Price,
    //            quantity = item.Quantity
    //        }).ToList(),
    //        package_dimension = new
    //        {
    //            length = dimension.length,
    //            width = dimension.width,
    //            height = dimension.height
    //        },
    //        delivery_instructions = "Please provide additional instructions for your package"
    //    };

    //    var json = JsonConvert.SerializeObject(payload);
    //    var content = new StringContent(json, Encoding.UTF8, "application/json");

    //    // Call the endpoint with the service code in the URL
    //    var response = await _httpClient.PostAsync($"shipping/fetch_rates/{serviceCode}", content);
    //    var responseBody = await response.Content.ReadAsStringAsync();

    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Fetch rates failed for service code {serviceCode}: {responseBody}");

    //    return JsonConvert.DeserializeObject<dynamic>(responseBody);
    //}

}
