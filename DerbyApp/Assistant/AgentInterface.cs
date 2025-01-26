using ClippySharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp.Assistant
{
    public class AgentInterface
    {
        readonly Image AgentImage;
        Agent Agent;
        bool _isVisible = false;

        public AgentInterface(Image agentImage)
        {
            AgentEnvironment.Current.Initialize(new SoundPlayer());
            AgentImage = agentImage;
        }

        public static List<string> GetAgentList()
        {
            List<string> agentList = [];

            foreach (var item in AgentEnvironment.GetAgents())
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
            if (_isVisible)
            {
                Agent.Stop();
                Agent.Play("GetAttention");
            }
        }

        public void ViewRacerAction()
        {
            if (_isVisible)
            {
                Agent.Stop();
                Agent.Play("Think");
            }
        }

        public void SelectRaceAction()
        {
            if (_isVisible)
            {
                Agent.Stop();
                Agent.Play("Explain");
            }
        }

        public void StartRaceAction()
        {
            if (_isVisible)
            {
                Agent.Stop();
                Agent.Play("Wave");
            }
        }

        public void ReportAction()
        {
            if (_isVisible)
            {

            }
        }

        private void Agent_NeedsRender(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                var image = Agent.GetCurrentImage();
                if (image != null)
                    AgentImage.Source = image;
            });
        }
    }
}
