/// <summary>
/// Enum used for sending commands from editor host to mobile client
/// 
/// This enum must be copied between the host/client projects (until I work on a better way to share code)
/// </summary>
public enum Message
{
    RefreshSnapshot,
    StartRecording,
    StopRecording,
    AlignMobileCamWithHoloLensCam,
    QrCodeUiShownOnMobile,
    QrCodeUiHiddenOnMobile,
    HoloLensScannedQR,
}