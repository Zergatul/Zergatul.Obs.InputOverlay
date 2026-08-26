using System;
using System.CommandLine;
using System.Net;

namespace Zergatul.Obs.InputOverlay
{
    public class ApplicationOptionsParser
    {
        public const string ListenOptionName = "--listen";

        public static int ParseAndRun(string[] args, Func<ApplicationOptions, int> run)
        {
            var rootCommand = new RootCommand("Input Overlay");
            var listenOption = new Option<string>(ListenOptionName)
            {
                Description = "IP to listen. Only localhost is accepted as non-IP value.",
                DefaultValueFactory = _ => "localhost",
                HelpName = "IP"
            };
            listenOption.Validators.Add(result =>
            {
                var listen = result.GetValue(listenOption);
                if (listen == "localhost")
                {
                    return;
                }
                if (!IPAddress.TryParse(listen, out _))
                {
                    result.AddError($"'{listen}' is not a valid IP address");
                }
            });
            rootCommand.Options.Add(listenOption);

            var portOption = new Option<int>("--port")
            {
                Description = "TCP port to listen.",
                DefaultValueFactory = _ => 5001,
                HelpName = "port"
            };
            rootCommand.Options.Add(portOption);

            var enableDebugLogOption = new Option<bool>("--enable-debug-log")
            {
                Description = "Enable debug logging."
            };
            rootCommand.Options.Add(enableDebugLogOption);

            rootCommand.SetAction(parseResult =>
            {
                var options = new ApplicationOptions(
                    parseResult.GetValue(listenOption),
                    parseResult.GetValue(portOption),
                    parseResult.GetValue(enableDebugLogOption));

                return run(options);
            });

            return rootCommand.Parse(args).Invoke();
        }
    }
}