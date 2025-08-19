using System.Linq;
using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-GetPlayfieldSize() / 2f, GetPlayfieldSize() / 2f), Random.Range(-GetPlayfieldSize() / 2f, GetPlayfieldSize() / 2f), Random.Range(-GetPlayfieldSize() / 2f, GetPlayfieldSize() / 2f)) * 0.9f;
    }
    public static float GetPlayfieldSize()
    {
        return 100;
    }
    public static string GetRandomName()
    {
        string[] playerNames = new string[0];
        string[] playerNamesRickandMorty = new string[20] { "Rick", "Morty", "Beth", "Jerry", "Summer", "Birdperson", "Mr. Poopybutthole", "Evil Morty", "Squanchy", "Tammy", "Unity", "Mr. Meeseeks", "Scary Terry", "Krombopulos Michael", "Gearhead", "Abradolf Lincler", "Noob-Noob", "Jessica", "Poopy Diaper", "Mr. Goldenfold" };
        string[] playerNamesBojack = new string[20] { "Bojack", "Diane", "Todd", "Princess Carolyn", "Mr. Peanutbutter", "Wanda", "Ruthie", "Sarah Lynn", "Vincent Adultman", "Emily", "Judah Mannowdog", "Lenny Turteltaub", "Cuddlywhiskers", "Charley Witherspoon", "Yoda", "Groot", "Darth Vader", "Obi-Wan Kenobi", "Leia Organa", "Han Solo" };
        string[] playerNamesSimpsons = new string[20] { "Homer", "Marge", "Bart", "Lisa", "Maggie", "Mr. Burns", "Smithers", "Ned Flanders", "Apu", "Krusty", "Sideshow Bob", "Milhouse", "Ralph Wiggum", "Chief Wiggum", "Comic Book Guy", "Edna Krabappel", "Patty Bouvier", "Selma Bouvier", "Moe Szyslak", "Barney Gumble" };
        string[] playerNamesFamilyGuy = new string[20] { "Peter", "Lois", "Stewie", "Brian", "Meg", "Chris", "Glenn Quagmire", "Cleveland Brown", "Joe Swanson", "Tom Tucker", "Angela", "Carter Pewterschmidt", "Mort Goldman", "Seamus", "Consuela", "Herbert", "Dr. Hartman", "Dr. Elmer Hartman", "Mayor Adam West", "Tricia Takanawa" };
        string[] playerNamesSouthPark = new string[20] { "Stan", "Kyle", "Cartman", "Kenny", "Butters", "Randy", "Sheila", "Mr. Garrison", "Mr. Mackey", "Chef", "Timmy", "Towelie", "Wendy", "Bebe", "Token", "Craig", "Tweek", "PC Principal", "Mr. Hankey", "Satan" };
        playerNames = playerNames.Concat(playerNamesRickandMorty)
            .Concat(playerNamesBojack)
            .Concat(playerNamesSimpsons)
            .Concat(playerNamesFamilyGuy)
            .Concat(playerNamesSouthPark)
            .ToArray();
        return playerNames[Random.Range(0, playerNames.Length)] + "AI";
    }
    public static string GetRegionFromStartupArgs()
    {
        if (System.Environment.CommandLine.Contains("-region asia"))
        {
            return "asia";
        }
        if (System.Environment.CommandLine.Contains("-region cn"))
        {
            return "cn";
        }
        if (System.Environment.CommandLine.Contains("-region us"))
        {
            return "us";
        }
        if (System.Environment.CommandLine.Contains("-region eu"))
        {
            return "eu";
        }
        if (System.Environment.CommandLine.Contains("-region jp"))
        {
            return "jp";
        }
        if (System.Environment.CommandLine.Contains("-region sa"))
        {
            return "sa";
        }
        if (System.Environment.CommandLine.Contains("-region kr"))
        {
            return "kr";
        }
        if (System.Environment.CommandLine.Contains("-region usw"))
        {
            return "usw";
        }

        Debug.Log("No server region porvided, defaulting to eu");
        return "eu";
    }

    public static int GetServerPortFromStartupArgs()
    {
        int port = 9100;

        string[] commandLineArgs = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i].Contains("-port"))
            {
                int.TryParse(commandLineArgs[i + 1], out port);

                Debug.Log($"Found the port arg {commandLineArgs[i]} and port should be {commandLineArgs[i + 1]}");

                return port;
            }
        }
        return port;
    }
    public static string GetServerIDFromStartupArgs()
    {
        string[] commandLineArgs = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i].Contains("-serverID"))
            {
                return commandLineArgs[i + 1];
            }
        }
        return "";
    }

    public static ushort GetMaxPlayersFromStartupArgs()
    {
        int maxPlayers = 10;

        string[] commandLineArgs = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i].Contains("-maxPlayers"))
            {
                int.TryParse(commandLineArgs[i + 1], out maxPlayers);
                return (ushort)maxPlayers;
            }
        }
        return (ushort)maxPlayers;
    }
}
