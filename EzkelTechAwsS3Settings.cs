using Nop.Core.Configuration;

namespace EzkelTech.Plugin.Misc.AwsS3
{
    public class EzkelTechAwsS3Settings : ISettings
    {
        public string Region { get; set; }
        public string Bucket { get; set; }
        public string SecretAccessKey { get; set; }
        public string AccessKeyId { get; set; }
        public string ObjectUrlTemplate { get; set; }

        public static string DefaultObjectUrlTemplate = "https://$Bucket$.s3-$Region$.amazonaws.com/$fileName$";
    }
}