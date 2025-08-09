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
        string[] names = new string[] { "Ted", "Barney", "Robin", "Lily", "Marshall", "Homer", "Bart", "Lisa", "Marge", "Meggie" };
        return names[Random.Range(0, names.Length)];
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
        int maxPlayers = 30;

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
