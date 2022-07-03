FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Checkout.TakeHomeChallenge.PaymentGateway/Checkout.TakeHomeChallenge.PaymentGateway.csproj", "Checkout.TakeHomeChallenge.PaymentGateway/"]
RUN dotnet restore "src/Checkout.TakeHomeChallenge.PaymentGateway/Checkout.TakeHomeChallenge.PaymentGateway.csproj"
COPY . .
WORKDIR "/src/Checkout.TakeHomeChallenge.PaymentGateway"
RUN dotnet build "Checkout.TakeHomeChallenge.PaymentGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Checkout.TakeHomeChallenge.PaymentGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Checkout.TakeHomeChallenge.PaymentGateway.dll"]
