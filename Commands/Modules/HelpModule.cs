using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgCountPlusNET.Commands.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        public HelpModule(CommandService cmdService)
        {
            _commandService = cmdService;
        }
        [Command("help")]
        [Summary("Get the list of commands available in this bot, or get the syntax for using a specific command")]
        public async Task help(string commandString = "not_actually_default_ignore_this")
        {
            if (commandString != "not_actually_default_ignore_this")
            {
                await helpCommand(commandString);
                return;
            }
            IReadOnlyCollection<CommandInfo> l = await _commandService.GetExecutableCommandsAsync(Context, null);
            List<string> s = new List<string>();
            foreach (CommandInfo cmd in l)
            {
                string e = "**mc+" + cmd.Name + " ";
                foreach (ParameterInfo p in cmd.Parameters)
                {
                    if (p.DefaultValue != null)
                    {
                        e += "[" + p.Name + "=" + p.DefaultValue + "] ";
                    }
                    else
                    {
                        e += "<" + p.Name + "> ";
                    }
                }
                e += "**";
                if (cmd.Summary != null)
                {
                    e += "\n```" + cmd.Summary + "```";
                }
                s.Add(e);
            }
            //s.Add("If you are seeing this line, send a screennshot of this command to the bot dev.");
            List<string> output = new List<string>();
            //s.ForEach(item => output += item);
            int i = 0;
            foreach (string c in s)
            {
                if (output.Count == 0 || i >= output.Count)
                {
                    output.Add("");
                }
                if ((output[i] + c + "\n").Length > 2000)
                {
                    i++;
                    if (output.Count == 0 || i >= output.Count)
                    {
                        output.Add("");
                    }
                }
                output[i] += c + "\n";
            }
            IDMChannel channel = await Context.User.GetOrCreateDMChannelAsync();
            foreach (string ae in output)
            {
                await channel.SendMessageAsync(">>> " + ae);
            }
            //await Context.Channel.SendMessageAsync(output);
            await Context.Channel.SendMessageAsync("Sent you a DM!");
        }
        private async Task helpCommand(string command)
        {
            IReadOnlyCollection<CommandInfo> l = await _commandService.GetExecutableCommandsAsync(Context, null);
            CommandInfo cmd = null;
            string CommandString = command;
            if (CommandString.StartsWith("mc+"))
            {
                CommandString = CommandString.Substring(3);
            }
            if (l.Where(item => item.Aliases.Contains(CommandString) || item.Name == CommandString).Count() > 0)
            {
                cmd = l.Where(item => item.Aliases.Contains(CommandString) || item.Name == CommandString).First();
            }
            else
            {
                await Context.Channel.SendMessageAsync("Command " + CommandString + " not found.");
                return;
            }

            string e = "**Help for mc+" + cmd.Name + " ";
            string desc = ">>> ";
            foreach (ParameterInfo p in cmd.Parameters)
            {
                if (p.DefaultValue != null)
                {
                    e += "[" + p.Name + "=" + p.DefaultValue + "] ";
                    desc += "**" + p.Name + "**\n```Summary: " + p.Summary + "\nDefaults to: " + p.DefaultValue.ToString() + "\nType: " + p.Type.ToString() + "\n" + "```";
                }
                else
                {
                    e += "<" + p.Name + "> ";
                    desc += "**" + p.Name + "**\n```Summary: " + p.Summary + "\nType: " + p.Type.ToString() + "\n" + "```";
                }
            }
            e += "**";
            await Context.Channel.SendMessageAsync(e + "\n" + desc);
        }
    }
}
