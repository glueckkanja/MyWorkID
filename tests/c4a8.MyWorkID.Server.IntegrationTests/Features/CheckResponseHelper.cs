﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features
{
    public static class CheckResponseHelper
    {
        public static async Task CheckForInsuffienctClaimsResponse(HttpResponseMessage response)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_INSUFFICIENT_CLAIMS);
        }
    }
}
