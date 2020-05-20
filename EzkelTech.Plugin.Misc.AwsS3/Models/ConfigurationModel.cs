using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace EzkelTech.Plugin.Misc.AwsS3.Models
{
    /// <summary>
    /// Represents plugin configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {

        [NopResourceDisplayName("EzkelTech.Plugin.Misc.AwsS3.Region")]
        public string Region { get; set; }
        [NopResourceDisplayName("EzkelTech.Plugin.Misc.AwsS3.Bucket")]
        public string Bucket { get; set; }
        [NopResourceDisplayName("EzkelTech.Plugin.Misc.AwsS3.SecretAccessKey")]
        public string SecretAccessKey { get; set; }
        [NopResourceDisplayName("EzkelTech.Plugin.Misc.AwsS3.AccessKeyId")]
        public string AccessKeyId { get; set; }
        [NopResourceDisplayName("EzkelTech.Plugin.Misc.AwsS3.ObjectUrlTemplate")]
        public string ObjectUrlTemplate { get; set; }
    }
}