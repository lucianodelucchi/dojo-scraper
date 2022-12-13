using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Models;
using Repositories;
using Services;

namespace Commands
{
    class DownloadImagesCommandHandlerBuilder : ICommandHandlerBuilder
    {
        const string CommandName = "download-images";
        private readonly DojoRepository _dojoRepository;
        private readonly ILogger<DownloadImagesCommandHandlerBuilder> _logger;
        private readonly AssetsManager _assetsManager;

        public DownloadImagesCommandHandlerBuilder(DojoRepository dojoRepository, ILogger<DownloadImagesCommandHandlerBuilder> logger, AssetsManager assetsManager)
        {
            _dojoRepository = dojoRepository;
            _logger = logger;
            _assetsManager = assetsManager;
        }

        public Command Build()
        {
            var downloadCommand = new Command(CommandName, "Donwloads images from the timeline");

            downloadCommand.AddOption(
                new Option(
                    new string[] { "--clear-folder", "-cf" },
                    "If set, all the images already downloaded will be removed")
                {
                    Argument = new Argument<bool>()
                }
            );

            downloadCommand.AddOption(
                new Option(
                    new string[] { "--before", "-b" }, 
                    "If provided, it will only download images before this date")
            {
                Argument = new Argument<DateTime>(TryConvertDateTime)
            });

            downloadCommand.Handler = CommandHandler.Create(async (bool clearFolder, DateTime before, CancellationToken token) =>
            {
                try
                {
                    if (clearFolder)
                    {
                        _logger.LogInformation("Removing previously downloaded images");
                        _assetsManager.ClearImages();
                    }

                    _assetsManager.EnsureFolderExists();

                    _logger.LogInformation("Downloading stories");

                    IEnumerable<Story> stories = new List<Story>();

                    // O format is ISO 8601 compliant
                    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-round-trip-o-o-format-specifier
                    var beforeParameter = before == default ? "" : $"{before:O}";

                    do
                    {
                        var storyResponse = await _dojoRepository.GetStoriesAsync(beforeParameter, token);
                        beforeParameter = storyResponse.BeforeParameter;
                        stories = stories.Concat(storyResponse.Items);
                    }
                    while (beforeParameter != null && !token.IsCancellationRequested);

                    var attachmentsWithDate = from story in stories
                                              where story.Type == StoryType.TextAndAttachment && story.HasAttachments && !story.FromDojo
                                              from attachment in story.Contents.Attachments
                                              select (story.PostedAt, attachment);
                    var downloadedImages = 0;

                    foreach (var (postedAt, attachment) in attachmentsWithDate)
                    {
                        if (_assetsManager.ImageExists(attachment.Filename))
                        {
                            _logger.LogInformation($"Skipping image {attachment.Filename}, it's already downloaded");
                            continue;
                        }

                        _logger.LogInformation($"Downloading image: {attachment.Path}");
                        var _ = await _assetsManager.SaveImageAsync(
                                                        attachment.Filename,
                                                        await _dojoRepository.GetImageStreamAsync(attachment.Path, token),
                                                        postedAt,
                                                        token
                                );
                        downloadedImages++;
                    }

                    _logger.LogInformation($"Downloaded {downloadedImages} images");
                    
                    return 0;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning($"The operation was aborted");
                    return 1;
                }
            });

            return downloadCommand;
        }

        DateTime TryConvertDateTime(ArgumentResult result)
        {
            DateTime.TryParse(result.Tokens[0].Value, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var value);
            return value;
        }

    }
}