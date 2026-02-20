using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nop.Plugin.Payments.Iyzico.Models.Api;

#region Request Models

public class ThreeDSInitializeRequest
{
    [JsonPropertyName("locale")]
    public string Locale { get; set; } = "tr";

    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; }

    [JsonPropertyName("paidPrice")]
    public string PaidPrice { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "TRY";

    [JsonPropertyName("installment")]
    public int Installment { get; set; } = 1;

    [JsonPropertyName("paymentChannel")]
    public string PaymentChannel { get; set; } = "WEB";

    [JsonPropertyName("basketId")]
    public string BasketId { get; set; }

    [JsonPropertyName("paymentGroup")]
    public string PaymentGroup { get; set; } = "PRODUCT";

    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; set; }

    [JsonPropertyName("paymentCard")]
    public PaymentCard PaymentCard { get; set; }

    [JsonPropertyName("buyer")]
    public Buyer Buyer { get; set; }

    [JsonPropertyName("shippingAddress")]
    public Address ShippingAddress { get; set; }

    [JsonPropertyName("billingAddress")]
    public Address BillingAddress { get; set; }

    [JsonPropertyName("basketItems")]
    public List<BasketItem> BasketItems { get; set; } = new();
}

public class PaymentCard
{
    [JsonPropertyName("cardHolderName")]
    public string CardHolderName { get; set; }

    [JsonPropertyName("cardNumber")]
    public string CardNumber { get; set; }

    [JsonPropertyName("expireYear")]
    public string ExpireYear { get; set; }

    [JsonPropertyName("expireMonth")]
    public string ExpireMonth { get; set; }

    [JsonPropertyName("cvc")]
    public string Cvc { get; set; }

    [JsonPropertyName("registerCard")]
    public int RegisterCard { get; set; } = 0;
}

public class Buyer
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("surname")]
    public string Surname { get; set; }

    [JsonPropertyName("identityNumber")]
    public string IdentityNumber { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("gsmNumber")]
    public string GsmNumber { get; set; }

    [JsonPropertyName("registrationAddress")]
    public string RegistrationAddress { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("zipCode")]
    public string ZipCode { get; set; }

    [JsonPropertyName("ip")]
    public string Ip { get; set; }
}

public class Address
{
    [JsonPropertyName("address")]
    public string AddressLine { get; set; }

    [JsonPropertyName("zipCode")]
    public string ZipCode { get; set; }

    [JsonPropertyName("contactName")]
    public string ContactName { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }
}

public class BasketItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category1")]
    public string Category1 { get; set; }

    [JsonPropertyName("category2")]
    public string Category2 { get; set; }

    [JsonPropertyName("itemType")]
    public string ItemType { get; set; } = "PHYSICAL";
}

public class CreateSubMerchantRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("gsmNumber")]
    public string GsmNumber { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("iban")]
    public string Iban { get; set; }

    [JsonPropertyName("identityNumber")]
    public string IdentityNumber { get; set; }

    [JsonPropertyName("subMerchantType")]
    public string SubMerchantType { get; set; } = "PERSONAL";
}

public class ApprovalRequest
{
    [JsonPropertyName("paymentTransactionId")]
    public string PaymentTransactionId { get; set; }

    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }
}

#endregion

#region Response Models

public class IyzicoResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }
}

public class ThreeDSInitializeResponse : IyzicoResponse
{
    [JsonPropertyName("threeDSHtmlContent")]
    public string HtmlContent { get; set; }

    [JsonPropertyName("paymentId")]
    public string PaymentId { get; set; }
}

public class PaymentResponse : IyzicoResponse
{
    [JsonPropertyName("paymentId")]
    public string PaymentId { get; set; }

    [JsonPropertyName("basketId")]
    public string BasketId { get; set; }

    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }

    [JsonPropertyName("itemTransactions")]
    public List<ItemTransaction> ItemTransactions { get; set; }
}

public class ItemTransaction
{
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; }

    [JsonPropertyName("paymentTransactionId")]
    public string PaymentTransactionId { get; set; }

    [JsonPropertyName("subMerchantKey")]
    public string SubMerchantKey { get; set; }

    [JsonPropertyName("subMerchantPrice")]
    public decimal SubMerchantPrice { get; set; }
}

public class SubMerchantResponse : IyzicoResponse
{
    [JsonPropertyName("subMerchantKey")]
    public string SubMerchantKey { get; set; }
}

public class ApprovalResponse : IyzicoResponse
{
    [JsonPropertyName("paymentTransactionId")]
    public string PaymentTransactionId { get; set; }
}

#endregion
