using System.Text.Json.Serialization;

namespace Nop.Plugin.Accounting.Parasut.Models.Parasut;

/// <summary>
/// Paraşüt OAuth authentication request
/// </summary>
public record ParasutAuthRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; }
}

/// <summary>
/// Paraşüt OAuth authentication response
/// </summary>
public record ParasutAuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Paraşüt contact request (JSON:API format)
/// </summary>
public record ParasutContactRequest
{
    [JsonPropertyName("data")]
    public ContactData Data { get; set; }

    public record ContactData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "contacts";

        [JsonPropertyName("attributes")]
        public ContactAttributes Attributes { get; set; }
    }

    public record ContactAttributes
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        [JsonPropertyName("contact_type")]
        public string ContactType { get; set; } = "company";

        [JsonPropertyName("tax_office")]
        public string TaxOffice { get; set; }

        [JsonPropertyName("tax_number")]
        public string TaxNumber { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("district")]
        public string District { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }
    }
}

/// <summary>
/// Paraşüt product request (JSON:API format)
/// </summary>
public record ParasutProductRequest
{
    [JsonPropertyName("data")]
    public ProductData Data { get; set; }

    public record ProductData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "products";

        [JsonPropertyName("attributes")]
        public ProductAttributes Attributes { get; set; }
    }

    public record ProductAttributes
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("vat_rate")]
        public decimal VatRate { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "Adet";

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "TRL";

        [JsonPropertyName("list_price")]
        public decimal ListPrice { get; set; } = 0;

        [JsonPropertyName("archived")]
        public bool Archived { get; set; } = false;

        [JsonPropertyName("inventory_tracking")]
        public bool InventoryTracking { get; set; } = false;
    }
}

/// <summary>
/// Paraşüt generic response wrapper
/// </summary>
public record ParasutResponse<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonPropertyName("included")]
    public List<object> Included { get; set; }
}

/// <summary>
/// Paraşüt resource data (generic)
/// </summary>
public record ParasutResourceData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, object> Attributes { get; set; }

    [JsonPropertyName("relationships")]
    public Dictionary<string, object> Relationships { get; set; }
}

/// <summary>
/// Invoice line item
/// </summary>
public record InvoiceLine
{
    public string ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public string Description { get; set; }
}
