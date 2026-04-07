using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

public class MusicPlayerForm : Form
{
    private Button btnOpen, btnPlay, btnPause, btnStop;
    private TrackBar volumeSlider;
    private Label statusLabel;
    private string currentFile = "";

    [DllImport("winmm.dll")]
    private static extern long mciSendString(string command, string returnValue, int returnLength, IntPtr winHandle);

    public MusicPlayerForm()
    {
        this.Text = "Simple Music Player";
        this.Size = new Size(400, 250);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        btnOpen = new Button() { Text = "Open", Location = new Point(20, 20), Size = new Size(70, 30) };
        btnPlay = new Button() { Text = "Play", Location = new Point(100, 20), Size = new Size(70, 30) };
        btnPause = new Button() { Text = "Pause", Location = new Point(180, 20), Size = new Size(70, 30) };
        btnStop = new Button() { Text = "Stop", Location = new Point(260, 20), Size = new Size(70, 30) };

        volumeSlider = new TrackBar()
        {
            Location = new Point(20, 70),
            Width = 310,
            Minimum = 0,
            Maximum = 1000,
            Value = 500,
            TickFrequency = 100
        };

        statusLabel = new Label()
        {
            Text = "No file loaded",
            Location = new Point(20, 130),
            AutoSize = true
        };

        btnOpen.Click += OpenFile;
        btnPlay.Click += Play;
        btnPause.Click += Pause;
        btnStop.Click += Stop;
        volumeSlider.Scroll += SetVolume;

        this.Controls.Add(btnOpen);
        this.Controls.Add(btnPlay);
        this.Controls.Add(btnPause);
        this.Controls.Add(btnStop);
        this.Controls.Add(volumeSlider);
        this.Controls.Add(statusLabel);
    }

    private void OpenFile(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Audio Files|*.mp3;*.wav;*.wma";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            currentFile = ofd.FileName;

            mciSendString("close myAudio", null, 0, IntPtr.Zero);
            mciSendString("open \"" + currentFile + "\" type mpegvideo alias myAudio", null, 0, IntPtr.Zero);

            statusLabel.Text = "Loaded: " + Path.GetFileName(currentFile);
        }
    }

    private void Play(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(currentFile))
        {
            mciSendString("play myAudio", null, 0, IntPtr.Zero);
            statusLabel.Text = "Playing";
        }
    }

    private void Pause(object sender, EventArgs e)
    {
        mciSendString("pause myAudio", null, 0, IntPtr.Zero);
        statusLabel.Text = "Paused";
    }

    private void Stop(object sender, EventArgs e)
    {
        mciSendString("stop myAudio", null, 0, IntPtr.Zero);
        statusLabel.Text = "Stopped";
    }

    private void SetVolume(object sender, EventArgs e)
    {
        int volume = volumeSlider.Value;
        mciSendString("setaudio myAudio volume to " + volume, null, 0, IntPtr.Zero);
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new MusicPlayerForm());
    }
}