using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Media;
using Nop.Services.Seo;
using Amazon.S3;
using Amazon.Runtime;
using Amazon;
using Amazon.S3.Model;
using System.IO;

namespace EzkelTech.Plugin.Misc.AwsS3.Services
{
    /// <summary>
    /// Picture service for Windows Azure
    /// </summary>
    public partial class Aws3PictureService : PictureService
    {
        #region Fields
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EzkelTechAwsS3Settings _awsS3Settings;
        #endregion

        #region Ctor

        public Aws3PictureService(INopDataProvider dataProvider,
            IDownloadService downloadService,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            EzkelTechAwsS3Settings awsS3Settings)
            : base(dataProvider,
                  downloadService,
                  eventPublisher,
                  httpContextAccessor,
                  fileProvider,
                  productAttributeParser,
                  pictureRepository,
                  pictureBinaryRepository,
                  productPictureRepository,
                  settingService,
                  urlRecordService,
                  webHelper,
                  mediaSettings)
        {
            _cacheKeyService = cacheKeyService;
            _staticCacheManager = staticCacheManager;
            _httpContextAccessor = httpContextAccessor;
            _awsS3Settings = awsS3Settings;
        }

        #endregion

        #region Utilities

        private bool SettingsIsValid()
        {
            return !string.IsNullOrEmpty(_awsS3Settings.AccessKeyId) &&
                !string.IsNullOrEmpty(_awsS3Settings.SecretAccessKey) &&
                !string.IsNullOrEmpty(_awsS3Settings.Bucket) &&
                !string.IsNullOrEmpty(_awsS3Settings.Region);
        }
        private IAmazonS3 CreateS3Client()
        {
            var awsCredentials = new BasicAWSCredentials(_awsS3Settings.AccessKeyId, _awsS3Settings.SecretAccessKey);
            var s3Config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(_awsS3Settings.Region) };
            return new AmazonS3Client(awsCredentials, s3Config);
        }
        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async void DeletePictureThumbs(Picture picture)
        {
            if (SettingsIsValid())
                await DeletePictureThumbsAsync(picture);
            else
                base.DeletePictureThumbs(picture);
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            if (SettingsIsValid())
            {
                var template = string.IsNullOrEmpty(_awsS3Settings.ObjectUrlTemplate)
                    ? EzkelTechAwsS3Settings.DefaultObjectUrlTemplate
                    : _awsS3Settings.ObjectUrlTemplate;

                return template.Replace($"${nameof(_awsS3Settings.Region)}$", _awsS3Settings.Region)
                    .Replace($"${nameof(_awsS3Settings.Bucket)}$", _awsS3Settings.Bucket)
                    .Replace("$fileName$", thumbFileName);
            }
            return base.GetThumbLocalPath(thumbFileName);
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            if (SettingsIsValid())
                    return GetThumbLocalPath(thumbFileName);
            return base.GetThumbUrl(thumbFileName);
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            if (SettingsIsValid())
                return GeneratedThumbExistsAsync(thumbFilePath, thumbFileName).Result;
            return base.GeneratedThumbExists(thumbFileName, thumbFileName);
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected override async void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            if (SettingsIsValid())
                await SaveThumbAsync(thumbFilePath, thumbFileName, mimeType, binary);
            else 
                base.SaveThumb(thumbFilePath, thumbFileName, mimeType, binary);
        }

        /// <summary>
        /// Initiates an asynchronous operation to delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual async Task DeletePictureThumbsAsync(Picture picture)
        {
            //create a string containing the blob name prefix
            var prefix = $"{picture.Id:0000000}";

            string continuationToken = null;
            using var client = CreateS3Client();
            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _awsS3Settings.Bucket,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                };

                var result = await client.ListObjectsV2Async(request, _httpContextAccessor.HttpContext.RequestAborted);

                //delete files in result segment
                await Task.WhenAll(result.S3Objects.Select(blobItem =>
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = blobItem.BucketName,
                        Key = blobItem.Key
                    };

                    return client.DeleteObjectAsync(deleteObjectRequest);
                }));

                //get the continuation token.
                continuationToken = result.ContinuationToken;
            }
            while (continuationToken != null);
            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefixCacheKey);
        }

        /// <summary>
        /// Initiates an asynchronous operation to get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected virtual async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName)
        {
            try
            {
                var key = _cacheKeyService.PrepareKeyForDefaultCache(NopMediaDefaults.ThumbExistsCacheKey, thumbFileName);

                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    using var client = CreateS3Client();
                    //var result = await _amazonS3Client.GetObjectAsync(
                    //        new GetObjectRequest
                    //        {
                    //            BucketName = _bucketName,
                    //            Key = thumbFileName,
                    //            ByteRange = new ByteRange("bytes=0-3")
                    //        },
                    //        _httpContextAccessor.HttpContext.RequestAborted);

                    var result = await client.GetObjectMetadataAsync(
                       new GetObjectMetadataRequest
                       {
                           BucketName = _awsS3Settings.Bucket,
                           Key = thumbFileName,
                       }, _httpContextAccessor.HttpContext.RequestAborted);

                    return result.HttpStatusCode != System.Net.HttpStatusCode.NotFound;
                });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Initiates an asynchronous operation to save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected virtual async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            using var client = CreateS3Client();
            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _awsS3Settings.Bucket,
                Key = thumbFileName,
                InputStream = new MemoryStream(binary),
                ContentType = mimeType,
            }, _httpContextAccessor.HttpContext.RequestAborted);

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefixCacheKey);
        }

        #endregion

        #region Disposable Implementation
        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (_isisposed)
        //        return;
        //    if (disposing)
        //    {
        //        _amazonS3Client?.Dispose();
        //    }
        //    _isisposed = true;
        //}
        //~Aws3PictureService()
        //{
        //    // Finalizer calls Dispose(false)
        //    Dispose(false);
        //}
        #endregion
        //public async Task<DataStoreRespnse> GetAsync(string key, CancellationToken cancellationToken = default)
        //{
        //    using (var response = await _amazonS3Client.GetObjectAsync(_bucketName, key, cancellationToken))
        //    {
        //        using (var responseStream = response.ResponseStream)
        //        {
        //            var stream = new MemoryStream();
        //            await responseStream.CopyToAsync(stream, cancellationToken);
        //            stream.Position = 0;
        //            return new DataStoreRespnse
        //            {
        //                ContentLength = response.ContentLength,
        //                StatusCode = response.HttpStatusCode,
        //                Headers = response.Headers.Keys.ToDictionary(k => k, v => response.Headers[v]),
        //                ResponseStream = stream,
        //                ETag = response.ETag,
        //                LastModified = response.LastModified
        //            };
        //        }
        //    }
        //}

    }
}