using System.CommandLine;

namespace GoogleAdk.Dev.Cli;

public static class EvalCommand
{
    public static Command Create()
    {
        var command = new Command("eval", "Run evaluation sets.");
        command.SetAction(_ =>
        {
            Console.WriteLine("Eval command is not yet implemented.");
        });
        return command;
    }
}
