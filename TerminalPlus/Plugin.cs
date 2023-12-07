using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using UnityEngine;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace TestPlugin
{
    [BepInPlugin("orangefantasma.terminalplus", "TerminalPlus", "1.0.3")]
    [BepInDependency("atomic.terminalapi")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("TerminalPlus Loaded!");

            TerminalAwake += TerminalIsAwake;
            TerminalWaking += TerminalIsWaking;
            TerminalStarting += TerminalIsStarting;
            TerminalStarted += TerminalIsStarted;
            TerminalParsedSentence += TextSubmitted;
            TerminalBeginUsing += OnBeginUsing;
            TerminalBeganUsing += BeganUsing;
            TerminalExited += OnTerminalExit;
            //potentially useful future method
            //TerminalTextChanged += OnTextChange;

            // Will display 'World' when 'hello' is typed into the terminal
            AddCommand("hello", "World\n");
        }
        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal Exited");
        }

        private void TerminalIsAwake(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal is awake");
            AddCommand("detailedscan", "Ship is not Landed!\n\n", "detscan", true);
            AddCommand("tp", "Attempting Teleport...\n\n", "tele", true);
        }

        private void TerminalIsWaking(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal is waking");
        }

        private void TerminalIsStarting(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal is starting");
        }

        private void TerminalIsStarted(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Terminal is started");
        }

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode}");
            Logger.LogMessage($"Text sent by: {sender}");
            if (e.SubmittedText == "tp")
            {
                ShipTeleporter[] teleporters = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();
                for (int i = 0; i < teleporters.Length; i++)
                {
                    if (!teleporters[i].isInverseTeleporter && teleporters[i].buttonTrigger.interactable)
                    {
                        teleporters[i].buttonTrigger.onInteract.Invoke(GameNetworkManager.Instance.localPlayerController);
                    }
                }
            }
        }

        private void OnBeginUsing(object sender, TerminalEventArgs e)
        {
            Logger.LogMessage("Player has just started using the terminal");
            System.Collections.Generic.List<GrabbableObject> sortedItems = new System.Collections.Generic.List<GrabbableObject>();
            GrabbableObject[] unSortedItems = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            int totalValue = 0;
            for (int n = 0; n < unSortedItems.Length; n++)
            {
                if (unSortedItems[n].itemProperties.isScrap && !unSortedItems[n].scrapPersistedThroughRounds && !unSortedItems[n].isInShipRoom)
                {
                    sortedItems.Add(unSortedItems[n]);
                    totalValue += unSortedItems[n].scrapValue;
                }
            }

            string itemStr = string.Join("\n", sortedItems.Select(x => x.itemProperties.itemName + " : " + x.scrapValue.ToString() + " Value"));
            string finStr = "Items not in ship: " + sortedItems.Count().ToString() + "\n\n" + itemStr + "\n\nWith a total value of: " + totalValue.ToString() + "\n\n";

            UpdateKeywordCompatibleNoun("detscan", "detailedscan", CreateTerminalNode($"{finStr}", true));
        }

        private void BeganUsing(object sender, TerminalEventArgs e)
        {
            //sender seems to equal e.Terminal
            Logger.LogMessage("Player is using terminal");
        }

    }
}