using Minio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Minio.DataModel.Args;

namespace Manage.Data.Public
{
    //services.AddSingleton<IFile>(provider => new Files(endpoint,accesskey,secretkey,secure)); 

    public interface IFile
    {
        Task<string> Get(string fullpath, int expiry = 1000);
        Task<bool> Upload(Stream file, string name, string bucket, string type = "none");
        Task<bool> Delete(string fullpath);

    }
    public class Files: IFile
    {
        private readonly IMinioClient minio;
        public Files(string endpoint, string accessKey, string secretKey, bool secure = true)
        {
            minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();
        }

        public string[] supportedtypes_image = { "jpg", "png", "gif", "jpeg", "tiff", "svg", "bmp", "heif", "raw", "jfif" };
        public string[] supportedtypes_video = { "mp4", "mkv", "mpg", "mpeg", "wmv", "flv", "avi", "mov", "avchd", "webm" };

        public  async Task<string> Get(string fullpath, int expiry = 1000)
        {
            try
            {
                if (!string.IsNullOrEmpty(fullpath))
                {
                    var bucket = fullpath.Split("/")[0];
                    var name = fullpath.Split("/")[1];

                    var args = new PresignedGetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(name)
                    .WithExpiry(expiry);
                    return await minio.PresignedGetObjectAsync(args).ConfigureAwait(false);
                }
                return "";
            }
            catch (Exception)
            {

                return "";
            }

        }

        public  async Task<bool> Upload(Stream file, string name, string bucket, string type = "none")
        {
            try
            {
                if (file.Length > 0)
                {
                    var beArgs = new BucketExistsArgs()
                    .WithBucket(bucket);
                    bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                    if (!found)
                    {
                        var mbArgs = new MakeBucketArgs()
                            .WithBucket(bucket);
                        await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                    }


                    if (type == "image-low" && supportedtypes_image.Contains(name.Split('.')[1]) && file.Length >= 1048576)
                    {
                        var thumb = Image.Load(file);
                        var encoder = new JpegEncoder()
                        {
                            Quality = 16 //Use variable to set between 5-30 based on your requirements
                        };
                        await thumb.SaveAsync(file,encoder).ConfigureAwait(false);

                    }
                    else if (type == "thumb")
                    {
                        var thumb = Image.Load(file);
                        thumb.Mutate(x => x.Resize(150, 150));
                        var encoder = new JpegEncoder()
                        {
                            Quality = 100
                        };
                        await thumb.SaveAsync(file, encoder).ConfigureAwait(false);
                    }

                    var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(name)
                    .WithStreamData(file);
                    await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public  async Task<bool> Delete(string fullpath)
        {
            try
            {
                if (!string.IsNullOrEmpty(fullpath))
                {   
                    var bucket = fullpath.Split("/")[0];
                    var name = fullpath.Split("/")[1];
                    RemoveObjectArgs rmArgs = new RemoveObjectArgs()
                              .WithBucket(bucket)
                              .WithObject(name);
                    await minio.RemoveObjectAsync(rmArgs);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
