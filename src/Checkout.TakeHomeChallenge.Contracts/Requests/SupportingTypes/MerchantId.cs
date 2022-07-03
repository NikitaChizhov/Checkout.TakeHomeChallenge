using StronglyTypedIds;

namespace Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

[StronglyTypedId(StronglyTypedIdBackingType.Guid, 
    StronglyTypedIdConverter.SystemTextJson | StronglyTypedIdConverter.TypeConverter)]
public partial struct MerchantId {}