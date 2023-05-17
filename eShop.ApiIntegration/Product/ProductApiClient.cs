﻿using eShop.ApiIntegration.Common;
using eShop.ViewModels.Catalog.Products.Dtos;
using eShop.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace eShop.ApiIntegration.Product
{
    public class ProductApiClient : BaseApiClient, IProductApiClient
    {
        public ProductApiClient(IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(configuration, httpClientFactory, httpContextAccessor) { }

        public async Task<ApiResult<bool>> Create(ProductCreateRequest request)
        {
            HttpClient httpClient = GetHttpClient(_configuration, _httpClientFactory, _httpContextAccessor);

            //form data
            var content = new MultipartFormDataContent();
            foreach (var prop in request.GetType().GetProperties())
            {
                var value = prop.GetValue(request);
                if (value is FormFile)
                {
                    var file = value as FormFile;
                    content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = prop.Name, FileName = file.FileName };
                }
                else
                {
                    content.Add(new StringContent(value.ToString()), prop.Name);
                }
            }

            var response = await httpClient.PostAsync("products", content);
            if (!response.IsSuccessStatusCode) return new ApiErrorResult<bool>(response.Content.ReadAsStringAsync().Result);
            return new ApiSuccessResult<bool>(true);
        }

        public async Task<ApiResult<PagedResult<ProductVm>>> GetPaging(GetProductRequest request)
        {
            return await GetAsync<PagedResult<ProductVm>>($"products?languageId={request.languageId}&pageIndex={request.pageIndex}&pageSize={request.pageSize}&keyword={request.keyword}&categoryId={request.categoryId}");
        }

        public async Task<ApiResult<List<ProductVm>>> GetListFeature(string languageId, int take)
        {
            return await GetAsync<List<ProductVm>>($"products/feature/{languageId}/{take}");
        }

        public async Task<ApiResult<List<ProductVm>>> GetListLatest(string languageId, int take)
        {
            return await GetAsync<List<ProductVm>>($"products/latest/{languageId}/{take}");
        }
    }
}
