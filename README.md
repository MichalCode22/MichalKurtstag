# MichalKurtstag

## Architecture

The solution consists of an ASP.NET Core API and a React user interface built with Vite:

- `MichalKurtstag/Controllers` exposes the `GET /api/my-requests` endpoint and requires a JWT.
- `MichalKurtstag/Services` combines requests and the service catalog, maps statuses, and determines whether action is required or the SLA has been exceeded.
- `MichalKurtstag/Clients` currently provides sample data through mock implementations. The interfaces can be backed by real systems later.
- `MichalKurtstag/frontend/my-requests` is the React interface. Vite proxies `/api` requests to the local API.
- `MichalKurtstag.Tests` contains tests for the API logic.

## Prerequisites

- .NET SDK 10
- Node.js and npm

## Run Locally

Open two terminals at the repository root (`MichalKurtstag`).

In the first terminal, start the API:

```powershell
dotnet run --project MichalKurtstag/MichalKurtstag.csproj --launch-profile http
```

The API will be available at `http://localhost:5069`.

In the second terminal, install and start the frontend:

```powershell
cd MichalKurtstag/frontend/my-requests
npm install
npm start
```

Open the Vite URL shown in the terminal, usually `http://localhost:5173`. Start the API before using the interface. In Development, the interface automatically requests a local token from `/api/dev-auth/token`; no user or secret configuration is required.

## Test and Build

Run the API tests from the repository root:

```powershell
dotnet test MichalKurtstag.Tests/MichalKurtstag.Tests.csproj
```

Run the frontend tests and build from `MichalKurtstag/frontend/my-requests`:

```powershell
npm test
npm run build
```