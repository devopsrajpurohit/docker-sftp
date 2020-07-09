using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ES.SFTP.Host.Interop;
using ES.SFTP.Host.Messages.Configuration;
using ES.SFTP.Host.Messages.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ES.SFTP.Host.SSH
{
    public class HookRunner : INotificationHandler<ServerStartupEvent>, INotificationHandler<UserSessionChangedEvent>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public HookRunner(ILogger<HookRunner> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }


        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task Handle(ServerStartupEvent request, CancellationToken cancellationToken)
        {
            var sftpConfig = await _mediator.Send(new SftpConfigurationRequest());
            var hooks = sftpConfig.Global.Hooks.OnServerStartup ?? new List<string>();
            foreach (var hook in hooks) await RunHook(hook);
        }


        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task Handle(UserSessionChangedEvent request, CancellationToken cancellationToken)
        {
            var sftpConfig = await _mediator.Send(new SftpConfigurationRequest());
            var hooks = sftpConfig.Global.Hooks.OnSessionChange ?? new List<string>();
            var args = string.Join(' ', request.SessionState, request.Username);
            foreach (var hook in hooks) await RunHook(hook, args);
        }

        private async Task RunHook(string hook, string args = null)
        {
            if (!File.Exists(hook))
            {
                _logger.LogInformation("Hook '{hook}' does not exist", hook);
                return;
            }

            var execPermissionOutput = await ProcessUtil.QuickRun("bash",
                $"-c \"if [[ -x {hook} ]]; then echo 'true'; else echo 'false'; fi\"", false);

            if (execPermissionOutput.ExitCode != 0 ||
                !bool.TryParse(execPermissionOutput.Output, out var isExecutable) || 
                !isExecutable)
            {
                await ProcessUtil.QuickRun("chmod", $"+x {hook}");
            }

            _logger.LogDebug("Executing hook '{hook}'", hook);
            var hookRun = await ProcessUtil.QuickRun(hook, args, false);
            var a = new FileInfo(hook);
            

            if (string.IsNullOrWhiteSpace(hookRun.Output))
                _logger.LogDebug("Hook '{hook}' completed with exit code {exitCode}.", hook, hookRun.ExitCode);
            else
                _logger.LogDebug(
                    "Hook '{hook}' completed with exit code {exitCode}." +
                    $"{Environment.NewLine}Output:{Environment.NewLine}{{output}}",
                    hook, hookRun.ExitCode, hookRun.Output);
        }
    }
}