using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql.BackendMessages;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Exceptions;
using static SabiMarket.Infrastructure.Services.CloudinaryService;

namespace SabiMarket.Infrastructure.Services;



public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var cloudName = config.GetSection("Cloudinary:CloudName").Value;
        var apiKey = config.GetSection("Cloudinary:ApiKey").Value;
        var apiSecret = config.GetSection("Cloudinary:ApiSecret").Value;

        Account account = new Account
        {
            ApiKey = apiKey,
            ApiSecret = apiSecret,
            Cloud = cloudName
        };

        cloudinary = new Cloudinary(account);
    }

    public async Task<BaseResponse<Dictionary<string, string>>> UploadImage(IFormFile photo, string folderName)
    {
        var response = new Dictionary<string, string>();
        var defaultSize = 800000;
        var allowedTypes = new List<string>() { "jpeg", "jpg", "png" };
        Console.WriteLine($"Allowed Types: {string.Join(", ", allowedTypes)}");


        if (photo == null)
        {
            response.Add("Code", "400");
            response.Add("Message", "No file uploaded");
            return ResponseFactory.Fail<Dictionary<string, string>>(new NotFoundException("No file uploaded"), "No file uploaded");
        }

        var file = photo;

        if (file.Length < 1 || file.Length > defaultSize)
        {
            response.Add("Code", "400");
            response.Add("Message", "Invalid size");
            return ResponseFactory.Fail<Dictionary<string, string>>(new InvalidDataException("Invalid size"), "Invalid size");
        }

        if (allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            response.Add("Code", "400");
            response.Add("Message", "Invalid type");
            return ResponseFactory.Fail<Dictionary<string, string>>(new InvalidDataException("Invalid Type"), "Invalid Type"); ;
        }

        var uploadResult = new ImageUploadResult();

        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.Name, stream),
                Folder = folderName
            };
            uploadResult = await cloudinary.UploadAsync(uploadParams);
        }

        if (!string.IsNullOrEmpty(uploadResult.PublicId))
        {
            response.Add("Code", "200");
            response.Add("Message", "Upload successful");
            response.Add("PublicId", uploadResult.PublicId);
            response.Add("Url", uploadResult.Url.ToString());

            return ResponseFactory.Success<Dictionary<string, string>>(response, "Success");
        }

        response.Add("Code", "400");
        response.Add("Message", "Failed to upload");
        return ResponseFactory.Fail<Dictionary<string, string>>(new InvalidDataException("Failed to upload"), "Failed to upload");
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicUrl)
    {
        var publicId = publicUrl.Split('/').Last().Split('.')[0];
        var deleteParams = new DeletionParams(publicId);
        return await cloudinary.DestroyAsync(deleteParams);
    }

}
