using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

// ��������� builder ��� ������������
var builder = WebApplication.CreateBuilder(args);

// ϳ�������� JSON, XML �� INI �����
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddXmlFile("companies.xml")
    .AddIniFile("companies.ini")
    .AddJsonFile("myinfo.json");

var app = builder.Build();

// ������� ��� ����������� ������ � ��������� ������� �����������
app.MapGet("/", (IConfiguration config) =>
{
    // ������������ ����� ��� ������ � JSON
    var companiesJson = config.GetSection("Companies").Get<IEnumerable<Company>>();

    // ������������ ����� � XML
    var companiesXml = XDocument.Load("companies.xml")
        .Descendants("Company")
        .Select(c => new Company
        {
            Name = c.Element("Name")?.Value,
            Employees = int.Parse(c.Element("Employees")?.Value ?? "0")
        });

    // ������������ ����� � INI
    var iniConfig = new ConfigurationBuilder()
        .AddIniFile("companies.ini")
        .Build();

    var companiesIni = new List<Company>
    {
        new Company { Name = "Microsoft", Employees = int.Parse(iniConfig["Microsoft:Employees"]) },
        new Company { Name = "Apple", Employees = int.Parse(iniConfig["Apple:Employees"]) },
        new Company { Name = "Google", Employees = int.Parse(iniConfig["Google:Employees"]) }
    };

    // ��'������ �� ���
    var allCompanies = companiesJson.Concat(companiesXml).Concat(companiesIni);

    // ��������� ������� � ��������� ������� �����������
    var largestCompany = allCompanies.OrderByDescending(c => c.Employees).First();

    return $"������� � ��������� ������� �����������: {largestCompany.Name} ({largestCompany.Employees} �����������)";
});

// ������� ��� ����������� ���� ����������
app.MapGet("/myinfo", (IConfiguration config) =>
{
    var myInfo = config.GetSection("MyInfo").Get<MyInfo>();
    return $"��'�: {myInfo.Name}, ³�: {myInfo.Age}, �������: {myInfo.Occupation}, ���: {myInfo.Hobby}";
});

app.Run();

// ����� ��� ������� �� ���������� ��� ���
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
