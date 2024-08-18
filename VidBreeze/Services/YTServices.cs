using System;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace VidBreeze.Services;

public class YTServices
{
    private readonly YoutubeClient youtube;
    public YTServices()
    {
        this.youtube = new YoutubeClient();
    }

    public async Task DownloadVideo(string videoURL, string filePath, string videoquality, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var ffmpegpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "ffmpeg");
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoURL);

        var audioStreamInfo = streamManifest
            .GetAudioOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .GetWithHighestBitrate();


        var videoStreamInfo = streamManifest
            .GetVideoOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .First(s => s.VideoQuality.Label == videoquality);

        var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
        var conversionRequest = new ConversionRequestBuilder(filePath)
            .SetPreset(ConversionPreset.Fast)
            .SetFFmpegPath(ffmpegpath)
            .Build();
        await youtube.Videos.DownloadAsync(streamInfos, conversionRequest, progress, cancellationToken);
    }

    public async Task DownloadAudio(string videoURL, string filePath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoURL);
        var audioStreamInfo = streamManifest
            .GetAudioOnlyStreams()
            .GetWithHighestBitrate();

        await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, filePath, progress, cancellationToken);
    }

    public async Task<List<string>> GetQuality(string videoURL)
    {
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoURL);
        var videoQuality = streamManifest
            .GetVideoOnlyStreams()
            .Select(stream => stream.VideoQuality.Label.ToString())
            .Distinct()
            .ToList();
        return videoQuality;
    }
}
