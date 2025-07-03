using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.Interfaces;

public interface ICloudinaryService
{
    Task<DeletionResult> DeletePhotoAsync(string publicUrl);
    Task<BaseResponse<Dictionary<string, string>>> UploadImage(IFormFile photo, string folderName);
}
