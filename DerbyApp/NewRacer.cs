using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;

#warning PRETTY: Incorporate "New Racer" into WPF

namespace DerbyApp
{
    public partial class NewRacer : Form
    {
        private readonly FilterInfoCollection _videoDevices;
        private static bool _needSnapshot = false;

        public Racer Racer = new Racer();

        public delegate void CaptureSnapshotManifast(Bitmap image);

        public NewRacer()
        {
            InitializeComponent();
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            GetListCameraUSB();
            foreach (string s in GirlScoutLevels.ScoutLevels)
            {
                cbLevel.Items.Add(s);
            }
        }

        ~NewRacer()
        {
            CloseCurrentVideoSource();
        }

        private void GetListCameraUSB()
        {
            if (_videoDevices.Count != 0)
            {
                foreach (FilterInfo device in _videoDevices)
                {
                    cbCameraList.Items.Add(device.Name);
                }
            }
            else
            {
                cbCameraList.Items.Add("No DirectShow devices found");
            }
            cbCameraList.SelectedIndex = 0;
        }

        public void CloseCurrentVideoSource()
        {
            try
            {
                if (videoSourcePlayer1.VideoSource != null)
                {
                    videoSourcePlayer1.SignalToStop();
                    // wait ~ 3 seconds
                    for (int i = 0; i < 30; i++)
                    {
                        if (!videoSourcePlayer1.IsRunning)
                            break;
                        System.Threading.Thread.Sleep(100);
                    }
                    if (videoSourcePlayer1.IsRunning)
                    {
                        videoSourcePlayer1.Stop();
                    }
                    videoSourcePlayer1.VideoSource = null;
                }
            }
            catch { }
        }

        public void OpenVideoSource(IVideoSource source)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                CloseCurrentVideoSource();
                videoSourcePlayer1.VideoSource = source;
                videoSourcePlayer1.Start();
                this.Cursor = Cursors.Default;
            }
            catch { }
        }

        private void OpenCamera()
        {
            try
            {
                VideoCaptureDevice videoDevice = new VideoCaptureDevice(_videoDevices[cbCameraList.SelectedIndex].MonikerString);
                OpenVideoSource(videoDevice);
                buttonCamera.Text = "Capture Image";
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        public void UpdateCaptureSnapshotManifast(Bitmap image)
        {
            try
            {
                _needSnapshot = false;
                pictureBox1.Image = image;
                pictureBox1.Update();
                Racer.Photo = (Image)image.Clone();
            }
            catch { }
        }

        private void VideoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (_needSnapshot)
                {
                    this.Invoke(new CaptureSnapshotManifast(UpdateCaptureSnapshotManifast), image);
                }
            }
            catch
            { }
        }

        private void ButtonCamera_Click(object sender, EventArgs e)
        {
            if (buttonCamera.Text == "Start Camera")
            {
                OpenCamera();
            }
            else
            {
                _needSnapshot = true;
            }
        }

        private void TbName_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                e.Cancel = true;
                tbName.Focus();
                errorProvider1.SetError(tbName, "Name should not be left blank!");
            }
            else
            {
                e.Cancel = false;
                errorProvider1.SetError(tbName, "");
            }
        }

        private void CbLevel_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cbLevel.Text))
            {
                e.Cancel = true;
                cbLevel.Focus();
                errorProvider1.SetError(cbLevel, "Level should not be left blank!");
            }
            else
            {
                e.Cancel = false;
                errorProvider1.SetError(cbLevel, "");
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Racer.RacerName = tbName.Text;
            Racer.Troop = tbTroop.Text;
            Racer.Level = cbLevel.Text;
            Racer.Weight = nuWeight.Value;
            Racer.Email = tbEmail.Text;
            CloseCurrentVideoSource();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            CloseCurrentVideoSource();
        }

        private void NewRacer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                if (!ValidateChildren())
                {
                    e.Cancel = true;
                }
            }
        }
    }
}

