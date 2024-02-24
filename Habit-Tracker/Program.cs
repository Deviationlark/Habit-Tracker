using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using Microsoft.Data.Sqlite;

string connectionString = @"Data Source=Habit-Tracker.db";

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    var tableCmd = connection.CreateCommand();

    tableCmd.CommandText =
        @"CREATE TABLE IF NOT EXISTS drinking_water (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            Quantity INTEGER
        )";

    tableCmd.ExecuteNonQuery();

    connection.Close();
}

GetUserInput();

void GetUserInput()
{
    Console.Clear();
    bool closeApp = false;
    while (closeApp == false)
    {
        Console.WriteLine("Main Menu");
        Console.WriteLine("--------------------");
        Console.WriteLine("0. Close Application");
        Console.WriteLine("1. View Records");
        Console.WriteLine("2. Insert Record");
        Console.WriteLine("3. Delete Record");
        Console.WriteLine("4. Update Record");
        string? input = Console.ReadLine();

        switch (input)
        {
            case "0":
                Console.WriteLine("Goodbye!");
                Console.ReadLine();
                closeApp = true;
                break;
            case "1":
                GetAllRecords();
                break;
            case "2":
                Insert();
                break;
                /*case "3":
                    Delete();
                    break;
                case "4":
                    Update();
                    break;*/
        }
    }
}

void Insert()
{
    string date = GetDateInput();

    int quantity = GetNumberInput("Please insert number of glasses or another measue of your choice (no decimals)");

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText =
        $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";

        tableCmd.ExecuteNonQuery();

        connection.Close();
    }
}

string GetDateInput()
{
    Console.WriteLine("Please insert the date: (Format: dd-mm-yy).");
    Console.WriteLine("Enter 0 to go back to Main Menu.");

    string? dateInput = Console.ReadLine();
    if (dateInput == "0") GetUserInput();
    return dateInput;
}

int GetNumberInput(string message)
{
    Console.WriteLine(message);

    string numInput = Console.ReadLine();

    if (numInput == "0") GetUserInput();

    int finalInput = Convert.ToInt32(numInput);

    return finalInput;
}

void GetAllRecords()
{
    Console.Clear();
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText =
        $"SELECT * FROM drinking_water";

        List<DrinkingWater> tableData = new();

        SqliteDataReader reader = tableCmd.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                tableData.Add(
                    new DrinkingWater
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                        Quantity = reader.GetInt32(2)
                    }
                );
            }
        }
        else
            Console.WriteLine("No rows found");

        connection.Close();

        Console.WriteLine("----------------------");

        foreach(var dw in tableData)
        {
            Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - Quantity: {dw.Quantity}");
        }

        Console.WriteLine("----------------------");
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}