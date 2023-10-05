﻿using System.Globalization;
using Microsoft.Data.Sqlite;

namespace HabitTracker.wkktoria;

internal class Program
{
    private static readonly string Path = System.IO.Path.GetDirectoryName(Directory
        .GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent
        .FullName);

    private static readonly string DbPath = System.IO.Path.Combine(Path, "data/habit-tracker.db");

    private static readonly string ConnectionString = @"Data Source = " + DbPath;

    private static void Main(string[] args)
    {
        var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";

        Thread.CurrentThread.CurrentCulture = customCulture;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = """
                               CREATE TABLE IF NOT EXISTS drinking_water (
                                   Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                   Date DATETIME,
                                   Quantity NUMERIC
                               )
                               """;
        tableCmd.ExecuteNonQuery();

        connection.Close();

        ShowMenu();
        GetUserInput();
    }

    private static void ShowMenu()
    {
        Console.WriteLine("\nMain Menu");
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("\t- Type v to view all records");
        Console.WriteLine("\t- Type i to insert record");
        Console.WriteLine("\t- Type u to update record");
        Console.WriteLine("\t- Type d to delete record");
        Console.WriteLine("\t- Type m to show main menu");
        Console.WriteLine("\t- Type q to quit application");
    }

    private static void GetUserInput()
    {
        var quitApp = false;

        while (!quitApp)
        {
            Console.Write("> ");
            var commandInput = Console.ReadLine();

            switch (commandInput.Trim().ToLower())
            {
                case "v":
                    GetAllRecords();
                    break;
                case "i":
                    Insert();
                    break;
                case "u":
                    Update();
                    break;
                case "d":
                    Delete();
                    break;
                case "m":
                    ShowMenu();
                    break;
                case "q":
                    quitApp = true;
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
    }

    private static string GetDateInput()
    {
        Console.Write("Insert the date (format: dd-mm-yy): ");

        var dateInput = Console.ReadLine();

        return dateInput;
    }

    private static double GetNumberInput(string message)
    {
        Console.Write(message);

        var numberInput = Console.ReadLine();

        if (numberInput.Contains(',')) numberInput = numberInput.Replace(",", ".");

        return Convert.ToDouble(numberInput);
    }

    private static void Insert()
    {
        var date = GetDateInput();
        var quantity = GetNumberInput("Please insert the quantity: ");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";
        tableCmd.ExecuteNonQuery();

        Console.WriteLine("Record has been inserted.");

        connection.Close();
    }

    private static void GetAllRecords()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "SELECT * FROM drinking_water";

        List<DrinkingWater> tableData = new();
        var reader = tableCmd.ExecuteReader();

        if (reader.HasRows)
            while (reader.Read())
                tableData.Add(new DrinkingWater
                    { Id = reader.GetInt32(0), Date = reader.GetDateTime(1), Quantity = reader.GetDouble(2) });


        connection.Close();

        Console.WriteLine("All records:");
        if (tableData.Any())
            foreach (var dw in tableData)
                Console.WriteLine($"id: {dw.Id} - added: {dw.Date:dd-MM-yyyy} - quantity: {dw.Quantity:#.##}");
        else
            Console.WriteLine("No records found.");
    }

    private static void Update()
    {
        GetAllRecords();

        var recordId = (int)GetNumberInput("Please insert id of the record you want to update: ");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id={recordId})";

        var checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (checkQuery == 0)
        {
            Console.WriteLine($"Record with id {recordId} doesn't exist.");
            connection.Close();
            Update();
        }
        else
        {
            var date = GetDateInput();
            var quantity = GetNumberInput("Please insert the quantity: ");

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"UPDATE drinking_water SET date='{date}', quantity={quantity} WHERE Id={recordId}";
            tableCmd.ExecuteNonQuery();

            Console.WriteLine("Record has been updated.");

            connection.Close();
        }
    }

    private static void Delete()
    {
        GetAllRecords();

        var recordId = (int)GetNumberInput("Please insert id of the record you want to delete: ");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = $"DELETE FROM drinking_water WHERE Id='{recordId}'";

        var rowCount = tableCmd.ExecuteNonQuery();

        switch (rowCount)
        {
            case 0:
                Console.WriteLine($"Record with id {recordId} doesn't exist.");
                Delete();
                break;
            default:
                Console.WriteLine($"Record with id {recordId} has been deleted.");
                break;
        }

        connection.Close();
    }
}