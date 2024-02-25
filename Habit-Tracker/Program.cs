using System.Globalization;
using Microsoft.Data.Sqlite;

string connectionString = @"Data Source=Habit-Tracker.db";

Console.WriteLine("Would you like to add a new habit or use an existing one?");
Console.WriteLine("1. Create new habit");
Console.WriteLine("2. Use an existing one");
Console.WriteLine("3. Delete an existing one");
string? result = Console.ReadLine();

string[] habitInfo = new string[2];
List<string> habits = new List<string>();
List<DrinkingWater> tableData = new();
int count = 0;

switch (result)
{
    case "1":
        CreateHabit();
        break;
    case "2":
        GetUserInput();
        break;
    case "3":
        DeleteHabit();
        break;
    default:
        Console.WriteLine("Not an option.");
        break;

}

string GetInputHabit()
{

    string? inputHabit = "";
    Console.Clear();
    Console.WriteLine("Choose which habit you would like to view: ");
    do
    {
        count = 0;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";

            using (SqliteDataReader reader = tableCmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        count++;
                        // The "name" column contains the table name
                        string tableName = reader.GetString(0);
                        Console.WriteLine($"{count}. {tableName}");
                        habits.Add(tableName);
                    }
                }
            }
            inputHabit = Console.ReadLine();
            connection.Close();
        }
    } while (!int.TryParse(inputHabit, out _) || string.IsNullOrEmpty(inputHabit));
    return habits[Convert.ToInt32(inputHabit) - 1];

}

void GetUserInput()
{
    string habit = GetInputHabit();
    Console.Clear();
    bool closeApp = false;
    while (closeApp == false)
    {
        Console.Clear();
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
                Environment.Exit(0);
                break;
            case "1":
                GetAllRecords();
                break;
            case "2":
                Insert(habit);
                break;
            case "3":
                Delete(habit);
                break;
            case "4":
                Update(habit);
                break;
            default:
                Console.WriteLine("Invalid Command. Please type a number from 0 to 4.");
                break;
        }
    }
}

void GetAllRecords()
{
    string habit = GetInputHabit();
    string info = GetColumn(habit);
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText =
        $"SELECT * FROM {habit}";

        tableData.Clear();

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
        if (tableData.Capacity > 0)
        {
            Console.WriteLine("----------------------");

            foreach (var dw in tableData)
            {
                Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - {info}: {dw.Quantity}");
            }

            Console.WriteLine("----------------------");

        }
        Console.ReadLine();
    }
}

void Insert(string habit)
{
    string? info = GetColumn(habit);
    string date = GetDateInput();
    int quantity = GetNumberInput($"Please insert number of {info} or another measue of your choice (no decimals)");

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText =
        $"INSERT INTO {habit}(date, {info}) VALUES('{date}', {quantity})";

        tableCmd.ExecuteNonQuery();

        connection.Close();
    }
}

void Delete(string habit)
{
    GetAllRecords();
    if (tableData.Capacity > 0)
    {
        var recordID = GetNumberInput("Type the ID of the record you want to delete.");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"DELETE from {habit} WHERE Id = '{recordID}'";
            int rowCount = tableCmd.ExecuteNonQuery();

            if (rowCount == 0)
            {
                Console.WriteLine($"Record with ID {recordID} doesn't exist.");
                Delete(habit);
            }

            Console.WriteLine($"Record with ID {recordID} was deleted.");

            GetUserInput();
        }
    }
    else
        Console.WriteLine("No rows found.");

}

void Update(string habit)
{

    GetAllRecords();
    string info = GetColumn(habit);
    var recordID = GetNumberInput("Type the ID you want to update, Type 0 to go to Main Menu");

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM {habit} WHERE Id = {recordID})";
        int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (checkQuery == 0)
        {
            Console.WriteLine($"Record with Id {recordID} doesn't exist.");
            Console.ReadLine();
            connection.Close();
            Update(habit);
        }

        string date = GetDateInput();

        int quantity = GetNumberInput($"Insert the amount of {info} or other measure of your choice");


        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = @$"UPDATE {habit} SET date = '{date}', {info} = {quantity} WHERE Id = 
        {recordID}";

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

    while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
    {
        Console.WriteLine("Invalid Date. (Format: dd-mm-yy).");
        dateInput = Console.ReadLine();
    }

    return dateInput;
}

int GetNumberInput(string message)
{
    Console.WriteLine(message);

    string? numInput = Console.ReadLine();

    if (numInput == "0") GetUserInput();

    while (!Int32.TryParse(numInput, out _) || Convert.ToInt32(numInput) < 0)
    {
        Console.WriteLine("Invalid Number. Try again.");
        numInput = Console.ReadLine();
    }

    int finalInput = Convert.ToInt32(numInput);

    return finalInput;
}

void CreateHabit()
{
    string?[] habit = new string[2];
    Console.Clear();
    Console.WriteLine("Enter the habit you would like to track (Format: word, word_word): ");
    habit[0] = Console.ReadLine();

    Console.WriteLine("Enter the unit measurement you would like to track the habit with: ");
    habit[1] = Console.ReadLine();

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();

        tableCmd.CommandText =
            @$"CREATE TABLE IF NOT EXISTS {habit[0]} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            {habit[1]} INTEGER
        )";
        count++;
        habits.Add($"{habit[0]}");
        tableCmd.ExecuteNonQuery();

        connection.Close();
    }
    GetUserInput();
}

void DeleteHabit()
{
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        var tableCmd = connection.CreateCommand();

        tableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";
        Console.WriteLine("TYPE THE EXACT NAME OF THE TABLE YOU WANT TO DELETE");
        using (SqliteDataReader reader = tableCmd.ExecuteReader())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    count++;
                    // The "name" column contains the table name
                    string tableName = reader.GetString(0);
                    Console.WriteLine($"{count}. {tableName}");

                }
            }
        }
        string? input = Console.ReadLine();
        tableCmd.CommandText = $"DROP TABLE {input}";
    }
}

string GetColumn(string habit)
{
    string columnName;
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();
        string sql = $"SELECT * FROM {habit} LIMIT 1;";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (i == 2)
                    {
                        columnName = reader.GetName(i);
                        return columnName;
                    }
                }
            }
        }
        connection.Close();
        return "";
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}