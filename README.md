# WP8.1-W8.1-and-BAND-VLC-remote-control
The library and a demo project for remote HTTP VLC control for windows 8.1, windows phone 8.1 and Microsoft BAND (soon). Library is already good, but UI for phone and windows need some work

There is no configuration page for Windows 8.1 application.

The library also supports YouTube play back.

    var com = new VlcWebControler(this.GetUrl(), this.password.Password);
    var isOk = await com.TestConnexion();
    if (isOk)
    {
        this.connectionResult.Text = "Connection TEST: OK!";
        await com.PlayFile(new VlcLib.Media.VlcMediaItem()
        {
            FileName = "https://www.youtube.com/watch?v=OCy5461BtTg?vq=hd1080"
        });
    }



