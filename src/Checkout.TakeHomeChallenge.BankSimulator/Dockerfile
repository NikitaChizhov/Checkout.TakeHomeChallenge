FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Checkout.TakeHomeChallenge.BankSimulator/Checkout.TakeHomeChallenge.BankSimulator.csproj", "Checkout.TakeHomeChallenge.BankSimulator/"]
COPY ["src/Checkout.TakeHomeChallenge.Contracts/Checkout.TakeHomeChallenge.Contracts.csproj", "Checkout.TakeHomeChallenge.Contracts/"]
RUN dotnet restore "src/Checkout.TakeHomeChallenge.BankSimulator/Checkout.TakeHomeChallenge.BankSimulator.csproj"
COPY . .
WORKDIR "/src/Checkout.TakeHomeChallenge.BankSimulator"
RUN dotnet build "Checkout.TakeHomeChallenge.BankSimulator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Checkout.TakeHomeChallenge.BankSimulator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Checkout.TakeHomeChallenge.BankSimulator.dll"]
