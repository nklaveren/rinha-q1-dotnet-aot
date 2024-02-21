FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-jammy-aot AS build
WORKDIR /source

COPY . .
RUN --mount=type=cache,target=/root/.nuget \
    --mount=type=cache,target=/source/bin \
    --mount=type=cache,target=/source/obj \
    dotnet publish -r linux-amd64 -o /app rinha/rinha.csproj --self-contained
RUN rm /app/*.dbg 

# final stage/image
FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:8.0-jammy-chiseled-aot
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./rinha"]