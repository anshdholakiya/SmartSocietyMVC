# SmartSocietyMVC

A comprehensive web application for managing smart societies, built with ASP.NET Core MVC.

## Prerequisites

Before you begin, ensure you have the following installed on your local machine:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or the version specified in `SmartSocietyMVC.csproj`)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- [Node.js and npm](https://nodejs.org/) (for Tailwind CSS)
- [Git](https://git-scm.com/)

## Setup Instructions

Follow these exact steps to get the project running on your computer:

### 1. Clone the Repository
Copy the code from GitHub to your local machine:
```bash
git clone https://github.com/yourusername/SmartSocietyMVC.git
cd SmartSocietyMVC
```
*(Replace `yourusername` with the actual GitHub username)*

### 2. Restore Dependencies
Download all the required NuGet packages automatically:
```bash
dotnet restore
```

### 3. Install NPM Packages
The project uses Tailwind CSS. Install the necessary Node dependencies:
```bash
npm install
```

### 4. Build Tailwind CSS
Generate the CSS file from Tailwind:
```bash
npm run build:css
```

### 5. Build the Database
Because the database is configured to use `(localdb)\mssqllocaldb`, you just need to generate the tables using Entity Framework:
```bash
dotnet ef database update
```
**Tip for Visual Studio users:** Instead of the terminal, you can open the **Package Manager Console** (`Tools > NuGet Package Manager > Package Manager Console`) and type:
```powershell
Update-Database
```

### 6. Run the Application
Start the application from the terminal:
```bash
dotnet run
```
**Tip for Visual Studio users:** You can simply press the **Play** button or **F5**.

## Testing that it Works

Once the application starts, it should automatically launch in your default browser (usually at `http://localhost:5000` or `https://localhost:7000`).

You should be able to:
1. View the landing page.
2. Log in with your credentials.
3. Access society management features.

## Support
If you run into any issues, please reach out to the project maintainers.
