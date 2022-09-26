using System;
using System.Windows;
using System.Speech.Synthesis;
using System.Collections.Generic;
using System.Linq;

namespace TextToVoice
{
	//written by: Scott Johnson from SoftwareNuggets
	//https://www.youtube.com/c/softwareNuggets
	
    public partial class MainWindow : Window
    {
        private SpeechSynthesizer? synthsizer;
        private List<InstalledVoice>? voiceList;

        public MainWindow()
        {
            InitializeComponent();

            this.synthsizer = null;
            this.voiceList = null;

            SetVoiceRates();

            bool rv = LoadInstalledVoices();
            InitializeEnabledState(rv);
        }

        private void SetVoiceRates()
        {
            cboRate.Items.Add("Slower");
            cboRate.Items.Add("Normal");
            cboRate.Items.Add("Faster");

            cboRate.SelectedIndex = 1;
        }

        private int GetVoiceRate()
        {
            int voiceRate = 0;
            switch (this.cboRate.SelectedValue)
            {
                case "Slower": voiceRate = -2; break;
                case "Normal": voiceRate = 2; break;
                case "Faster": voiceRate = 4; break;
            }
            return (voiceRate);
        }

        private bool LoadInstalledVoices()
        {

            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                voiceList = new List<InstalledVoice>();
                foreach (InstalledVoice voice in synth.GetInstalledVoices())
                {
                    voiceList.Add(voice);
                    cboVoices.Items.Add($"{voice.VoiceInfo.Name}-" +
                        $"{voice.VoiceInfo.Gender}-{voice.VoiceInfo.Culture}");
                }
                if (voiceList.Count == 0)
                    return (false);
            }

            cboVoices.SelectedIndex = 0;
            return (true);

        }

        private void InitializeEnabledState(bool state)
        {
            if (state == false)
            {
                MessageBox.Show("Missing:  Installed Voices");
            }

            this.BtnSay.IsEnabled = state;
            this.BtnPause.IsEnabled = false;
            this.BtnStop.IsEnabled = state;
            this.BtnClear.IsEnabled = state;
            this.BtnPlaySelection.IsEnabled = false;
        }

        private string? GetVoiceName()
        {
            if (this.cboVoices.Items.Count == 0)
            {
                MessageBox.Show("You must first install some voices");
                return null;
            }

            if (this.cboVoices.SelectedIndex < 0)
            {
                MessageBox.Show("You must first install some voices");
                return null;
            }
            else
            {

                string cboVoiceValue = (string)this.cboVoices.SelectedItem;

                var buckets = cboVoiceValue.ToString().Split("-");

                if (voiceList != null)
                {
                    for (int j = 0; j < voiceList.Count; j++)
                    {
                        var _v = voiceList[j];
                        var v_o = _v.VoiceInfo.Name.ToString().Trim().ToLower();
                        var v_b = buckets[0].ToString().Trim().ToLower();
                        if (v_o == v_b)
                            return (_v.VoiceInfo.Name.ToString());
                    }
                }
                return null;
            }
        }

        private void InitializeSynthsizer()
        {
            this.synthsizer = new SpeechSynthesizer();
            this.synthsizer.StateChanged += Synthsizer_StateChanged;
            this.synthsizer.SpeakCompleted += Synthsizer_SpeakCompleted;
            this.synthsizer.SpeakProgress += Synthsizer_SpeakProgress;
            this.synthsizer.SelectVoice(GetVoiceName());
            this.synthsizer.Rate = GetVoiceRate();
        }

        private void Synthsizer_StateChanged(object? sender, StateChangedEventArgs e)
        {
            switch (e.State)
            {
                case SynthesizerState.Paused:
                    if (this.synthsizer != null)
                    {
                        this.synthsizer.Pause();
                    }
                    break;
            }
        }

        private void Synthsizer_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            if (this.synthsizer != null)
            {
                this.synthsizer.Dispose();
                this.synthsizer = null;
            }

            this.BtnPause.Content = "Pause";
            this.BtnSay.IsEnabled = true;
            this.info.Text = String.Empty;
            this.Input.IsInactiveSelectionHighlightEnabled = false;
            this.cboVoices.IsEnabled = true;
            this.cboRate.IsEnabled = true;

            if(this.Input.SelectedText.Length>0)
                this.BtnPlaySelection.IsEnabled = true;
            else
                this.BtnPlaySelection.IsEnabled = false;
        }

        private void Synthsizer_SpeakProgress(object? sender, SpeakProgressEventArgs e)
        {
            info.Text = e.Text.ToString();


            if (BtnPlaySelection.IsEnabled==false)
            {
                this.Input.Focus();
                this.Input.Select(e.CharacterPosition, e.Text.Length);
            }
            else
            {
                this.Input.Focus();
                this.Input.Select(this.Input.SelectionStart, this.Input.SelectionLength);
            }
        }

        private void BtnFontBigger_Click(object sender, RoutedEventArgs e)
        {
            if (Input.FontSize < 60)
            {
                Input.FontSize = Input.FontSize + 2;
            }
        }

        private void BtnSmallerFont_Click(object sender, RoutedEventArgs e)
        {
            if (Input.FontSize > 10)
            {
                Input.FontSize = Input.FontSize - 2;
            }
        }

        private void BtnSay_Click(object sender, RoutedEventArgs e)
        {
            if (synthsizer != null)
            {
                synthsizer.Dispose();
            }

            if (Input.Text.ToString().Trim().Length == 0)
            {
                MessageBox.Show("Please provide some text in the green input box to continue");
                return;
            }
            
            InitializeSynthsizer();

            if (synthsizer != null)
            {
                synthsizer.SpeakAsync(this.Input.Text);
            }

            this.cboVoices.IsEnabled = false;
            this.cboRate.IsEnabled = false;
            this.BtnPause.IsEnabled = true;
            this.BtnPlaySelection.IsEnabled = false;
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (BtnPause.Content.ToString() == "Pause")
            {
                if (this.synthsizer != null)
                {
                    this.BtnSay.IsEnabled = false;
                    this.synthsizer.Pause();
                    this.BtnPause.Content = "Resume";
                }
            }
            else if (BtnPause.Content.ToString() == "Resume")
            {
                if (this.synthsizer != null)
                {
                    this.BtnSay.IsEnabled = true;
                    this.synthsizer.Resume();
                    this.BtnPause.Content = "Pause";
                }
            }
        }

        private void BtnPlaySelection_Click(object sender, RoutedEventArgs e)
        {
            if (this.Input.SelectionLength > 0)
            {
                PlaySelectedText(this.Input.SelectedText.ToString());
            }
        }

        private void PlaySelectedText(string selectedTxt)
        {
            if (synthsizer != null)
            {
                synthsizer.Dispose();
                synthsizer = null;
            }

            InitializeSynthsizer();
            if (this.synthsizer != null)
                this.synthsizer.SpeakAsync(selectedTxt);
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            
            this.BtnPause.Content = "Pause";
            this.BtnPause.IsEnabled = false;
            this.BtnSay.IsEnabled = true;
            this.info.Text = String.Empty;
            this.Input.IsInactiveSelectionHighlightEnabled = false;
            this.cboVoices.IsEnabled = true;
            this.cboRate.IsEnabled = true;
            this.BtnPlaySelection.IsEnabled = false;
            this.Input.SelectionLength = 0;
            this.Input.SelectionStart = 0;

            if (this.synthsizer != null)
            {
                this.synthsizer.Dispose();
                this.synthsizer = null;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            this.Input.Text = String.Empty;
            this.BtnSay.IsEnabled = true;
            this.BtnPause.Content = "Pause";
            this.info.Text = String.Empty;
            this.Input.SelectedText = String.Empty;
            this.cboVoices.IsEnabled = true;
            this.cboRate.IsEnabled = true;
            this.BtnPlaySelection.IsEnabled = true;

            if (this.synthsizer != null)
            {
                this.synthsizer.Dispose();
                this.synthsizer = null;
            }
        }

        private void Input_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(this.synthsizer == null)
            {
                if (this.Input.SelectedText.Length > 0)
                {
                    BtnPlaySelection.IsEnabled = true;
                }
                else
                {
                    BtnPlaySelection.IsEnabled = false;
                }
            }
            else
            {
                BtnPlaySelection.IsEnabled = false;
            }
            
        }
    }
}
