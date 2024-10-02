using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

// Створюємо builder для конфігурацій
var builder = WebApplication.CreateBuilder(args);

// Підключаємо JSON, XML та INI файли
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddXmlFile("companies.xml")
    .AddIniFile("companies.ini")
    .AddJsonFile("myinfo.json");

var app = builder.Build();

// Маршрут для відображення компанії з найбільшою кількістю співробітників
app.MapGet("/", (IConfiguration config) =>
{
    // Завантаження даних про компанії з JSON
    var companiesJson = config.GetSection("Companies").Get<IEnumerable<Company>>();

    // Завантаження даних з XML
    var companiesXml = XDocument.Load("companies.xml")
        .Descendants("Company")
        .Select(c => new Company
        {
            Name = c.Element("Name")?.Value,
            Employees = int.Parse(c.Element("Employees")?.Value ?? "0")
        });

    // Завантаження даних з INI
    var iniConfig = new ConfigurationBuilder()
        .AddIniFile("companies.ini")
        .Build();

    var companiesIni = new List<Company>
    {
        new Company { Name = "Microsoft", Employees = int.Parse(iniConfig["Microsoft:Employees"]) },
        new Company { Name = "Apple", Employees = int.Parse(iniConfig["Apple:Employees"]) },
        new Company { Name = "Google", Employees = int.Parse(iniConfig["Google:Employees"]) }
    };

    // Об'єднуємо всі дані
    var allCompanies = companiesJson.Concat(companiesXml).Concat(companiesIni);

    // Знаходимо компанію з найбільшою кількістю співробітників
    var largestCompany = allCompanies.OrderByDescending(c => c.Employees).First();

    return $"Компанія з найбільшою кількістю співробітників: {largestCompany.Name} ({largestCompany.Employees} співробітників)";
});

// Маршрут для відображення вашої інформації
app.MapGet("/myinfo", (IConfiguration config) =>
{
    var myInfo = config.GetSection("MyInfo").Get<MyInfo>();
    return $"Ім'я: {myInfo.Name}, Вік: {myInfo.Age}, Професія: {myInfo.Occupation}, Хобі: {myInfo.Hobby}";
});

app.Run();

// Класи для компаній та інформації про вас
public class Company
{
    public string Name { get; set; }
    public int Employees { get; set; }
}

public class MyInfo
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Occupation { get; set; }
    public string Hobby { get; set; }
}
