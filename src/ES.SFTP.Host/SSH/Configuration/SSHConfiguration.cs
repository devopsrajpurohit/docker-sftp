﻿using System.Collections.Generic;
using System.Text;

namespace ES.SFTP.Host.SSH.Configuration
{
    public class SSHConfiguration
    {
        public List<MatchBlock> MatchBlocks { get; } = new List<MatchBlock>();

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("UsePAM yes");

            builder.AppendLine("# SSH Protocol");
            builder.AppendLine("Protocol 2");
            builder.AppendLine();
            builder.AppendLine("# Host Keys");
            builder.AppendLine("HostKey /etc/ssh/ssh_host_ed25519_key");
            builder.AppendLine("HostKey /etc/ssh/ssh_host_rsa_key");
            builder.AppendLine();
            builder.AppendLine("# Disable DNS for fast connections");
            builder.AppendLine("UseDNS no");
            builder.AppendLine();
            builder.AppendLine("# Logging");
            builder.AppendLine("LogLevel INFO");
            builder.AppendLine();
            builder.AppendLine("# Subsystem");
            builder.AppendLine("Subsystem sftp internal-sftp");
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("# Match blocks");
            foreach (var matchBlock in MatchBlocks)
            {
                builder.Append(matchBlock);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}