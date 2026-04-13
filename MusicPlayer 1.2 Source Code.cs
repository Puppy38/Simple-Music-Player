using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class MusicPlayerForm : Form
{
    private Button btnOpen, btnPlay, btnPause, btnStop;
    private TrackBar volumeSlider, positionSlider;
    private Label statusLabel, timeLabel, volumeLabel;

    private Timer timer;

    private string currentFile = "";
    private int totalLength = 0;

    [DllImport("winmm.dll")]
    private static extern int mciSendString(
        string command,
        StringBuilder returnValue,
        int returnLength,
        IntPtr winHandle);

    public MusicPlayerForm(string fileToOpen = "")
    {
        this.Text = "Simple Music Player";
        this.Size = new Size(420, 300);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        btnOpen = new Button() { Text = "Open", Location = new Point(20, 20), Size = new Size(70, 30) };
        btnPlay = new Button() { Text = "Play", Location = new Point(100, 20), Size = new Size(70, 30) };
        btnPause = new Button() { Text = "Pause", Location = new Point(180, 20), Size = new Size(70, 30) };
        btnStop = new Button() { Text = "Stop", Location = new Point(260, 20), Size = new Size(70, 30) };

        positionSlider = new TrackBar()
        {
            Location = new Point(20, 70),
            Width = 350,
            Minimum = 0,
            Maximum = 1000
        };

        volumeSlider = new TrackBar()
        {
            Location = new Point(20, 130),
            Width = 250,
            Minimum = 0,
            Maximum = 1000,
            Value = 500
        };

        volumeLabel = new Label()
        {
            Text = "Volume: 50%",
            Location = new Point(280, 130),
            AutoSize = true
        };

        timeLabel = new Label()
        {
            Text = "00:00 / 00:00",
            Location = new Point(20, 180),
            AutoSize = true
        };

        statusLabel = new Label()
        {
            Text = "No file loaded",
            Location = new Point(20, 210),
            AutoSize = true
        };

        timer = new Timer();
        timer.Interval = 500;
        timer.Tick += UpdatePosition;

        btnOpen.Click += OpenFile;
        btnPlay.Click += Play;
        btnPause.Click += Pause;
        btnStop.Click += Stop;
        volumeSlider.Scroll += SetVolume;
        positionSlider.Scroll += Seek;

        this.Controls.Add(btnOpen);
        this.Controls.Add(btnPlay);
        this.Controls.Add(btnPause);
        this.Controls.Add(btnStop);
        this.Controls.Add(positionSlider);
        this.Controls.Add(volumeSlider);
        this.Controls.Add(volumeLabel);
        this.Controls.Add(timeLabel);
        this.Controls.Add(statusLabel);

        if (!string.IsNullOrEmpty(fileToOpen) && File.Exists(fileToOpen))
        {
            LoadFile(fileToOpen);
        }
    }

    private void OpenFile(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Audio Files|*.mp3;*.wav;*.wma;*.aac;*.mid;*.midi";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            LoadFile(ofd.FileName);
        }
    }

    private void LoadFile(string path)
    {
        currentFile = path;

        mciSendString("close myAudio", null, 0, IntPtr.Zero);

        // IMPORTANT: open file first
        mciSendString("open \"" + currentFile + "\" alias myAudio", null, 0, IntPtr.Zero);
        mciSendString("set myAudio time format milliseconds", null, 0, IntPtr.Zero);

        // try get length
        StringBuilder sb = new StringBuilder(128);
        mciSendString("status myAudio length", sb, sb.Capacity, IntPtr.Zero);

        int length;
        if (!int.TryParse(sb.ToString(), out length))
            length = 0;

        totalLength = length;
        positionSlider.Maximum = totalLength > 0 ? totalLength : 1000;

        statusLabel.Text = "Loaded: " + Path.GetFileName(currentFile);
    }

    private void Play(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(currentFile))
        {
            mciSendString("play myAudio", null, 0, IntPtr.Zero);
            timer.Start();
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
        timer.Stop();
        positionSlider.Value = 0;
        statusLabel.Text = "Stopped";
    }

    private void SetVolume(object sender, EventArgs e)
    {
        int volume = volumeSlider.Value;
        mciSendString("setaudio myAudio volume to " + volume, null, 0, IntPtr.Zero);

        volumeLabel.Text = "Volume: " + (volume / 10) + "%";
    }

    private void Seek(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(currentFile))
        {
            mciSendString(
                "play myAudio from " + positionSlider.Value,
                null,
                0,
                IntPtr.Zero);
        }
    }

    private void UpdatePosition(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(currentFile))
            return;

        StringBuilder sb = new StringBuilder(128);
        mciSendString("status myAudio position", sb, sb.Capacity, IntPtr.Zero);

        int position;
        if (!int.TryParse(sb.ToString(), out position))
            return;

        if (totalLength > 0 && position <= totalLength)
            positionSlider.Value = position;

        timeLabel.Text =
            FormatTime(position) + " / " + FormatTime(totalLength);
    }

    private string FormatTime(int ms)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(ms);
        return t.Hours > 0
            ? string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds)
            : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    [STAThread]
    public static void Main(string[] args)
    {
        Application.EnableVisualStyles();

        string fileToOpen = args.Length > 0 ? args[0] : "";
        Application.Run(new MusicPlayerForm(fileToOpen));
    }
}
