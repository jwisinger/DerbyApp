using DerbyApp.ClippySharp;
using ClippySharp.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DerbyApp.Assistant
{
    public class AgentInterface
    {
        readonly Image AgentImage;
        Agent Agent;
        bool _isVisible = true;

        public AgentInterface(Image agentImage)
        {
            AgentEnvironment.Current.Initialize(new AgentDelegate(), new SoundPlayer());
            AgentImage = agentImage;
        }

        public static List<string> GetAgentList()
        {
            List<string> agentList = [];

            foreach (var item in AgentEnvironment.Current.GetAgents())
            {
                agentList.Add(item[1]);
            }

            return agentList;
        }

        public void ChangeAgent(string agentName)
        {
            if (Agent != null)
            {
                Agent.NeedsRender -= Agent_NeedsRender;
                Agent.Dispose();
            }

            Agent = new Agent(agentName);

            Agent.NeedsRender += Agent_NeedsRender;
            Agent.Play("Greet");
            _isVisible = true;
        }

        public void ClickAgent()
        {
            Agent.Stop();
            if (_isVisible) Agent.Play("Goodbye");
            else Agent.Play("Greet");
            _isVisible = !_isVisible;
        }

        public void AddRacerAction()
        {
            _isVisible = true;
            Agent.Stop();
            Agent.Play("?");
        }

        public void ViewRacerAction()
        {
            _isVisible = true;
            Agent.Stop();
            Agent.Play("?");
        }

        public void SelectRaceAction()
        {
            _isVisible = true;
            Agent.Stop();
            Agent.Play("?");
        }

        public void StartRaceAction()
        {
            _isVisible = true;
            Agent.Stop();
            Agent.Play("?");
        }

        public void ReportAction()
        {
        }

        private void Agent_NeedsRender(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                var image = Agent.GetCurrentImage();
                if (image != null)
                    AgentImage.Source = image.NativeObject as ImageSource;
            });
        }
    }
}
